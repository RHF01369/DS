using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour, IPatternable
{
    private static MultiBattle instance;

    public static bool isPlaying { get; private set; }
    public static bool isNetworkingDisconnected;

    private Client client;

    private void Awake()
    {
        instance = this;
        isNetworkingDisconnected = false;
    }

    public static void Initialize(Client client)
    {
        instance.gameObject.SetActive(true);
        PatternLock.Battle = instance;
        instance.client = client;
    }

    public static void StartBattle()
    {
        Debug.Log(LogType.Trace, "StartBattle");

        isPlaying = true;
    }

    public static void OnAttack()
    {
        for (int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!SpawnManager.Units[index].IsUsed)
                continue;

            SpawnManager.Units[index].Activate();
        }
    }

    public static void OnSummon(int deckIndex, Vector2 position, bool isMyTeam)
    {
        Unit unit = SpawnManager.PullUnit(isMyTeam);

        UnitData unitData = GetUnitData(deckIndex, isMyTeam);

        unit.Initialize(unitData, position);

        unit.OnSummoned();
    }

    public static void OnSkill(bool isMyTeam)
    {

    }

    public static void GameOver()
    {

    }

    public void InputPattern(string pattern)
    {
        Debug.Log(LogType.Trace, "MultiBattle InputPattern");

        if (!isPlaying)
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