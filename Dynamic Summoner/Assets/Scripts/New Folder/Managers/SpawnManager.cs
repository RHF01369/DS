using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnManager
{
    private static int spawnsNumber;
    private static GameObject unitPrefab;
    private static UnitSpawn<Unit> spawns;
    private static Summoner mySummoner;
    private static Summoner enemySummoner;

    static SpawnManager()
    {
        spawnsNumber = 4;
        unitPrefab = Resources.Load("Unit") as GameObject;
        spawns = new UnitSpawn<Unit>();

        mySummoner = new Summoner(true);
        enemySummoner = new Summoner(false);

        InitSpawn();
    }

    private static void InitSpawn()
    {
        for (int num = 0; num < spawnsNumber; num++)
        {
            spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform),  true));
            spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform), false));
        }
    }

    public static Unit PullUnit(bool isMyTeam)
    {
        Unit unit = Spawns.Pull(isMyTeam);

        if (unit == null)
        {
            // 부족한 유닛이 우리팀 유닛인 경우 : 우리팀 유닛, 상대팀 유닛순으로 유닛을 생성하고 우리팀 유닛 반환
            if(isMyTeam)
            {
                unit = Spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform), isMyTeam));
                Spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform), !isMyTeam));
                return unit;
            }
            // 부족한 유닛이 상대팀 유닛인 경우 : 우리팀 유닛, 상대팀 유닛순으로 유닛을 생성하고 상대팀 유닛 반환
            if (!isMyTeam)
            {
                Spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform), !isMyTeam));
                return Spawns.AddUnit(new Unit(Object.Instantiate(unitPrefab, Setting.BattleZone.transform), isMyTeam));
            }
        }

        return unit;
    }

    /// <summary>
    /// Main화면에서 전투를 시작하기전에 유닛을 초기화 해준다.
    /// </summary>
    public static void ResetUnit()
    {
        for (int index = 0; index < Units.Count; index++)
        {
            if (Units[index].IsUsed)
                Units[index].SetIsUsed(false);
        }

        MySummoner.SetActive(false);
        EnemySummoner.SetActive(false);
    }

    public static int CountUnits()
    {
        return spawns.Units.Count;
    }

    public static void CopyUnitDatasToBuffer(byte[] buffer, bool isEnemyAllData, ref int packetSize)
    {
        ByteConverter.FromInt(CountUnits(), buffer, ref packetSize);

        foreach (Unit unit in Units)
        {
            ByteConverter.FromBool(unit.IsUsed, buffer, ref packetSize);

            if (!unit.IsUsed)
            {
                if (isEnemyAllData)
                    packetSize += 16;
                else
                    packetSize += 4;

                continue;
            }

            ByteConverter.FromFloat(unit.Health, buffer, ref packetSize);

            if(isEnemyAllData)
            {
                ByteConverter.FromInt(unit.Data.Number, buffer, ref packetSize);
                ByteConverter.FromFloat(unit.UnitObject.transform.position.x, buffer, ref packetSize);
                ByteConverter.FromFloat(unit.UnitObject.transform.position.y, buffer, ref packetSize);
            }
        }
    }

    #region Getter Setter

    public static UnitSpawn<Unit> Spawns { get { return spawns; } }

    public static List<Unit> Units { get { return spawns.Units; } }

    public static Summoner MySummoner { get { return mySummoner; } }

    public static Summoner EnemySummoner { get { return enemySummoner; } }

    #endregion

}
