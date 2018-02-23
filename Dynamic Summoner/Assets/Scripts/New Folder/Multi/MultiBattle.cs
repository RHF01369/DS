using System;
using System.Collections;
using UnityEngine;

public enum EnemyNetworkingState { Disconnected, Reconnected, WaitingSynchronization, Synchronizating ,Connected, }
public enum MyNetworkingState { Disconnected, Connected }

public class MultiBattle : MonoBehaviour, IPatternable
{
    public static MultiBattle Instance { get; private set; }

    public EnemyNetworkingState enemyNetworkingState { get; set; }
    public MyNetworkingState myNetworkingState { get; set; }

    private bool isSynchronized;
    private Client client;


    private void Awake()
    {
        Instance = this;
        isSynchronized = true;
        enemyNetworkingState = EnemyNetworkingState.Connected;
        myNetworkingState = MyNetworkingState.Connected;
    }

    public void Initialize(Client client)
    {
        Instance.gameObject.SetActive(true);
        PatternLock.Battle = Instance;
        Instance.client = client;
        Instance.isSynchronized = true;
    }

    public void SetSynchronized(bool value)
    {
        Instance.isSynchronized = value;
    }

    public void StartMakingEnemyDeck()
    {
        StartCoroutine(MakeEnemyDeck());
    }

    private IEnumerator MakeEnemyDeck()
    {
        while(true)
        {
            if(MultiBattleDataManager.enemyDeckData.isReceived)
            {
                Preparation.InitMultiEnemySummoner(MultiBattleDataManager.enemyDeckData.enemyLevel, null);
                for(int index = 0; index < Setting.DeckCount; index++)
                    DeckSetting.SetMultiEnemyDeck(MultiBattleDataManager.enemyDeckData.summonNumber[index], MultiBattleDataManager.enemyDeckData.summonLevel[index]);

                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle()
    {
        Debug.Log(LogType.Test, "MultiBattle StartBattle");

        ExecutionData executionData;
        while(Setting.GameState == GameState.Battle)
        {
            yield return new WaitForSeconds(0.2f);

            //if (enemyNetworkingState == EnemyNetworkingState.Synchronizating)
            //    SynchronizeEnemyBattleData(MultiBattleDataManager.synchronizationData);

            //if (enemyNetworkingState == EnemyNetworkingState.WaitingSynchronization || myNetworkingState == MyNetworkingState.Disconnected)
            //    continue;

            //if (enemyNetworkingState == EnemyNetworkingState.Reconnected)
            //{
            //    client.SendReconnectionSynchronization();
            //    enemyNetworkingState = EnemyNetworkingState.WaitingSynchronization;
            //    continue;
            //}

            if ((enemyNetworkingState == EnemyNetworkingState.Connected) && (MultiBattleDataManager.GetExecutionQueueCount() <= 0 || !isSynchronized))
            {
                if (MultiBattleDataManager.canSynchronize)
                    Synchronize(MultiBattleDataManager.synchronizationData);

                continue;
            }

            executionData = MultiBattleDataManager.DequeueExecutionData();
            Command(executionData);
        }
    }
                        
    private void Synchronize(byte[] syncData)
    {
        Debug.Log(LogType.Trace, "Synchronization");

        int unitCount = ByteConverter.ToInt(syncData, 9);

        int number = 1;
        foreach (Unit unit in SpawnManager.Units)
        {
            if (unitCount < number++ && unitCount < SpawnManager.CountUnits())
                break;

            if(number % 2 == 0)
                SynchronizeUnitHealth(syncData, unit, number - 2, 18);
            else
                SynchronizeUnitHealth(syncData, unit, number - 3, 13);
        }

        isSynchronized = true;
        MultiBattleDataManager.canSynchronize = false;
        Array.Clear(syncData, 0, syncData.Length);
    }

    private void SynchronizeUnitHealth(byte[] syncData, Unit unit, int number, int startIndex)
    {
        bool isUsed = ByteConverter.ToBool(syncData, startIndex + (5 * number));
        if (!isUsed && !unit.IsUsed)
            return;

        if (!isUsed && unit.IsUsed)
        {
            Debug.Log(LogType.Exception, "<Color=Blue> unit.SetIsUsed(false) </color>");
            unit.SetIsUsed(false);
            return;
        }

        unit.Health = ByteConverter.ToFloat(syncData, (startIndex + 1) + (5 * number));
    }


    private void SynchronizeEnemyBattleData(byte[] syncData)
    {
        Debug.Log(LogType.Test, "SynchronizeEnemyBattleData");

        SpawnManager.ResetUnit();

        int unitCount = ByteConverter.ToInt(syncData, 9);

        SynchronizeUnits(syncData, unitCount);

        SynchronizeExecutionDatas(syncData, unitCount);

        client.SendEnemyCompleteSynchronization();
    }

    private void SynchronizeUnits(byte[] syncData, int unitCount)
    {
        Debug.Log(LogType.Test, "SynchronizeUnits");

        for (int number = 1; number < unitCount; number++)
        {
            // 유닛을 사용하지 않을 경우 다음 유닛으로 넘어간다.
            if (!ByteConverter.ToBool(syncData, 13 + (number - 1) * 17))
                return;

            bool isMyUnit = number % 2 == 1 ? true : false;

            if (isMyUnit)
                SynchronizeUnit(syncData, isMyUnit, number / 2, 31);
            else
                SynchronizeUnit(syncData, !isMyUnit, number / 2 - 1, 14);
        }
    }

    private void SynchronizeUnit(byte[] syncData, bool isMyUnit, int number, int startIndex)
    {
        Debug.Log(LogType.Test, "SynchronizeUnit");

        Unit unit = SpawnManager.PullUnit(isMyUnit);

        int deckIndex = GetDeckIndex(ByteConverter.ToInt(syncData, startIndex + 4 + (number - 1) * 34), isMyUnit);

        UnitData unitData = GetUnitData(deckIndex, isMyUnit);

        float xPos = ByteConverter.ToFloat(syncData, startIndex + 8 + (number - 1) * 34);
        float yPos = ByteConverter.ToFloat(syncData, startIndex + 12 + (number - 1) * 34);

        unit.Initialize(unitData, new Vector3(xPos, yPos, 0));

        unit.Health = ByteConverter.ToFloat(syncData, startIndex + (number - 1) * 34);
    }

    private void SynchronizeExecutionDatas(byte[] syncData, int unitCount)
    {
        Debug.Log(LogType.Test, "SynchronizeExecutionDatas");

        int index = 13 + 17 * unitCount;
        int order = ByteConverter.ToInt(syncData, ref index);
        int executionDataCount = ByteConverter.ToInt(syncData, ref index);

        ExecutionData executionData = new ExecutionData();

        for (int count = 1; count < executionDataCount; count++)
        {
            ExecutionType executionType = (ExecutionType)ByteConverter.ToInt(syncData, ref index);

            if (executionType == ExecutionType.Attack)
            {
                executionData.type = executionType;
                executionData.order = (order - executionDataCount) + (count - 1);
            }

            else if (executionType == ExecutionType.Summon)
            {
                executionData.type = executionType;
                executionData.order = (order - executionDataCount) + (count - 1);
                executionData.isMine = count / 2 == 1 ? true : false;
                executionData.summonDeckIndex = ByteConverter.ToInt(syncData, ref index);
                executionData.position = new Vector3(ByteConverter.ToInt(syncData, ref index), ByteConverter.ToInt(syncData, ref index), 0);
            }
            else if (executionType == ExecutionType.Skill)
            {
                executionData.type = executionType;
                executionData.order = (order - executionDataCount) + (count - 1);
            }
            else
            {
                Debug.Log(LogType.Exception, "SynchronizeEnemyBattleData " + "OufOfExecutionTypeRange");
            }
        }

        MultiBattleDataManager.EnqueueExecutionData(executionData);
    }


    private void Command(ExecutionData executionData)
    {
        switch(executionData.type)
        {
            case ExecutionType.Attack:
                OnAttack();
                return;
            case ExecutionType.Summon:
                OnSummon(executionData.summonDeckIndex, executionData.position, executionData.isMine);
                return;
            case ExecutionType.Skill:
                OnSkill(executionData.isMine);
                return;
        }
    }

    private void OnAttack()
    {
        Debug.Log(LogType.Test, "OnAttack");

        for (int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (client.playerNumber == 1)
                AttackAsPlayer1(index);
            else
                AttackAsPlayer2(index);
        }
    }

    private void AttackAsPlayer1(int index)
    {
        if (!SpawnManager.Units[index].IsUsed)
            return;

        SpawnManager.Units[index].Activate();
        return;
    }

    private void AttackAsPlayer2(int index)
    {
        if (index % 2 == 1)
        {
            if (!SpawnManager.Units[index - 1].IsUsed)
                return;

            SpawnManager.Units[index - 1].Activate();
            return;
        }
        else
        {
            if (!SpawnManager.Units[index + 1].IsUsed)
                return;

            SpawnManager.Units[index + 1].Activate();
            return;
        }
    }

    private void OnSummon(int deckIndex, Vector2 position, bool isMyTeam)
    {
        Debug.Log(LogType.Trace, "OnSummon");

        Unit unit = SpawnManager.PullUnit(isMyTeam);

        UnitData unitData = GetUnitData(deckIndex, isMyTeam);

        unit.Initialize(unitData, position);

        unit.OnSummoned();
    }

    private void OnSkill(bool isMyTeam)
    {
        Debug.Log(LogType.Trace, "OnSkill");

    }

    public void GameOver()
    {

    }

    public void InputPattern(string pattern)
    {
        Debug.Log(LogType.Trace, "MultiBattle InputPattern");

        if (Setting.GameState != GameState.Battle)
            return;

        int index = GetMyDeckIndex(pattern);

        if (index == -1)
            return;

        client.SendSummonPacket(index);
    }

    private int GetMyDeckIndex(string pattern)
    {
        for (int index = 0; index < Setting.MyDeck.Count; index++)
        {
            if (Setting.MyDeck[index].Pattern == pattern)
                return index;
        }
        return -1;
    }

    private int GetDeckIndex(int summonNumber, bool isMyTeam)
    {
        if(isMyTeam)
            for (int myIndex = 0; myIndex < Setting.MyDeck.Count; myIndex++)
                if (Setting.MyDeck[myIndex].Number == summonNumber)
                    return myIndex;
        else
            for (int enemyIndex = 0; enemyIndex < Setting.EnemyDeck.Count; enemyIndex++)
                if (Setting.MyDeck[enemyIndex].Number == summonNumber)
                    return enemyIndex;
        return -1;
    }

    private static UnitData GetUnitData(int index, bool isMyTeam)
    {
        if (isMyTeam)
            return Setting.MyDeck[index];
        else
            return Setting.EnemyDeck[index];
    }
}