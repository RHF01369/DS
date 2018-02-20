using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour, IPatternable
{   
    public static Battle instance;

    private GameMode mode;

    private void Awake()
    {
        instance = this;

        mode = GameMode.Ready;
    }
    
    public static void SetActive(bool value)
    {
        instance.gameObject.SetActive(value);
    }

    public static void StartBattle(GameMode mode)
    {
        Mode = mode;
        instance.StartCoroutine(instance.SummonEnemy());
        instance.StartCoroutine(instance.StartAttack());
    }

    public void InputPattern(string pattern)
    {
        if (mode == GameMode.Ready)
            return;

        int index = GetMyDeckIndex(pattern);

        if (index == -1)
            return;
        OnSummon(index, true);
    }

    public static void InitEnemyBeforeBattle(int number, Vector2 position)
    {
        int index = instance.GetEnemyDeckIndex(number);

        Unit unit = SpawnManager.PullUnit(false);

        UnitData unitData = instance.GetUnitData(index, false);

        unit.Initialize(unitData, position);

        unit.OnSummoned();
    }


    private IEnumerator StartAttack()
    {
        while (true)
        {
            // 
            for(int index = 0; index < SpawnManager.Units.Count; index++)
            {
                if (!SpawnManager.Units[index].IsUsed)
                    continue;

                SpawnManager.Units[index].Activate();
            }
            // 
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator SummonEnemy()
    {
        int index;
        while (true)
        {
            index = Random.Range(0, 4);
            OnSummon(index, false);
            yield return new WaitForSeconds(2);
        }
    }

    private int GetMyDeckIndex(string pattern)
    {
        for(int index = 0; index < Setting.MyDeck.Count; index++)
        {
            if (Setting.MyDeck[index].Pattern == pattern)
                return index;
        }
        return -1;
    }

    private int GetEnemyDeckIndex(int number)
    {
        for (int index = 0; index < Setting.EnemyDeck.Count; index++)
        {
            if (Setting.EnemyDeck[index].Number == number)
                return index;
        }
        return -1;
    }

    private void OnSummon(int index, bool isMyTeam)
    {
        Unit unit = SpawnManager.PullUnit(isMyTeam);

        UnitData unitData = GetUnitData(index, isMyTeam);

        Vector2 position = PositionResearcher.GetPosition(unitData, isMyTeam);

        unit.Initialize(unitData, position);

        unit.OnSummoned();
    }

    private UnitData GetUnitData(int index, bool isMyTeam)
    {
        if(isMyTeam)
            return Setting.MyDeck[index];
        else
            return Setting.EnemyDeck[index];
    }


    public static void GameOver(bool DidMyTeamWin)
    {
        UnityEngine.Debug.LogError("Game Over");

        instance.StopAllCoroutines();

        if (DidMyTeamWin)
        {
            instance.UpdateRound();
            instance.RegainHealth();
        }
        else
        {
            SpawnManager.ResetUnit();
        }

        instance.mode = GameMode.Ready;

        GameHandler.Instance.StartSingleBattle();
    }

    private void UpdateRound()
    {
        if (mode == GameMode.Single)
            UserData.SingleRound++;
        else if (mode == GameMode.Challenge)
            UserData.ChallengeRound++;
    }

    private void RegainHealth()
    {
        for (int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!SpawnManager.Units[index].IsUsed)
                continue;

            if (!SpawnManager.Units[index].IsMyTeam)
                SpawnManager.Units[index].SetIsUsed(false);
            else
                SpawnManager.Units[index].Health = SpawnManager.Units[index].Data.Health;
        }
    }

    #region Getter Setter

    public static GameMode Mode { get { return instance.mode; } set { instance.mode = value; } }

    #endregion
}