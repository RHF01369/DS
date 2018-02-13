using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour, IPatternable
{
    public static MultiBattle instance { get; private set; }

    private Client client;

    private void Awake()
    {
        instance = this;
    }

    public static void Initialize(Client client)
    {
        instance.gameObject.SetActive(true);
        PatternLock.Battle = instance;
        instance.client = client;
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
                for(int index = 0; index < Setting.deckCount; index++)
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
            if (MultiBattleDataManager.GetExecutionQueueCount() <= 0)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            executionData = MultiBattleDataManager.DequeueExecutionData();
            Command(executionData);
            yield return new WaitForSeconds(0.2f);
        }
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
        Debug.Log(LogType.Trace, "OnAttack");

        for (int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!SpawnManager.Units[index].IsUsed)
                continue;

            SpawnManager.Units[index].Activate();
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