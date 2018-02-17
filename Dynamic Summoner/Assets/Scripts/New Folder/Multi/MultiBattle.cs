using System;
using System.Collections;
using UnityEngine;

public class MultiBattle : MonoBehaviour, IPatternable
{
    public static MultiBattle instance { get; private set; }

    private Client client;
    private bool isSynchronized;


    private void Awake()
    {
        instance = this;
        isSynchronized = true;
    }

    public static void Initialize(Client client)
    {
        instance.gameObject.SetActive(true);
        PatternLock.Battle = instance;
        instance.client = client;
        instance.isSynchronized = true;
    }

    public static void SetSynchronized(bool value)
    {
        instance.isSynchronized = value;
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

                yield return new WaitForSeconds(0.1f);

                StartCoroutine(StartBattle());
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator StartBattle()
    {
        Debug.Log(LogType.Trace, "MultiBattle StartBattle");

        ExecutionData executionData;
        while(Setting.GameState == GameState.Battle)
        {
            if (MultiBattleDataManager.GetExecutionQueueCount() <= 0 || !isSynchronized)
            {
                if (MultiBattleDataManager.canSynchronize)
                    Synchronization(MultiBattleDataManager.synchronizationData);

                yield return new WaitForSeconds(0.2f);
                continue;
            }

            executionData = MultiBattleDataManager.DequeueExecutionData();
            Command(executionData);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Synchronization(byte[] syncData)
    {
        Debug.Log(LogType.Test, "Synchronization");

        int totalNumber = ByteConverter.ToInt(syncData, 9);

        int number = 1;
        foreach (Unit unit in SpawnManager.Units)
        {
            if (totalNumber < SpawnManager.CountUnits() && totalNumber < number++)
                break;

            if(number % 2 == 0)
                SynchronizeUnit(syncData, unit, number, 18);
            else
                SynchronizeUnit(syncData, unit, number, 13);
        }

        isSynchronized = true;
        MultiBattleDataManager.canSynchronize = false;
        Array.Clear(syncData, 0, syncData.Length);
    }

    private void SynchronizeUnit(byte[] syncData, Unit unit, int number, int startIndex)
    {
        bool isUsed = ByteConverter.ToBool(syncData, startIndex + (10 * number - 2));
        if (!isUsed && !unit.IsUsed)
            return;

        if (!isUsed && unit.IsUsed)
            unit.SetIsUsed(false);

        unit.Health = ByteConverter.ToFloat(syncData, (startIndex + 1) + (10 * number - 2));
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

    private static UnitData GetUnitData(int index, bool isMyTeam)
    {
        if (isMyTeam)
            return Setting.MyDeck[index];
        else
            return Setting.EnemyDeck[index];
    }
}