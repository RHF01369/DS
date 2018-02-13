using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeckSetting
{
    static DeckSetting()
    {
    }

    public static void SetMyDeck()
    {
        int[] numbers = UserData.SummonNumbersOfDeck;

        if (!HasChangedMyDeck(numbers))
            return;

        Setting.MyDeck.Clear();

        int number, level;
        SummonInfo summonInfo;
        for (int index = 0; index < numbers.Length; index++)
        {
            number = numbers[index];
            summonInfo = SummonInfoManager.summonInfo[number];
            level = Setting.summonDataByNumber[number].level;

            Setting.MyDeck.Add(GetUnitData(summonInfo, level));
        }
    }

    public static void SetMultiEnemyDeck(int number, int level)
    {
        if (4 <= Setting.EnemyDeck.Count)
            Setting.EnemyDeck.Clear();

        SummonInfo summonInfo = SummonInfoManager.summonInfo[number];

        Setting.EnemyDeck.Add(GetUnitData(summonInfo, level));

        UnityEngine.Debug.Log("EnemyDeck.Add");
    }

    public static void SetEnemyDeckOfSingleMode(int round)
    {
        UnityEngine.Debug.Log("Round : " + round);

        Setting.EnemyDeck.Clear();

        RoundInfoOfSingleMode roundInfo = SingleBattleInfoManager.roundInfo[round];

        int number, level;
        SummonInfo summonInfo;
        for (int num = 0; num < roundInfo.summonInfo.Length; num++)
        {
            number = roundInfo.summonInfo[num].number;
            summonInfo = SummonInfoManager.summonInfo[number];
            level = roundInfo.summonInfo[num].level;

            Setting.EnemyDeck.Add(GetUnitData(summonInfo, level));
        }
    }

    public static void SetEnemyDeckOfChallengeMode(int round)
    {
        Setting.EnemyDeck.Clear();

        RoundInfoOfChallengeMode roundInfo = ChallengeBattleInfoManager.roundInfo[round];

        int number, level;
        SummonInfo summonInfo;
        for (int num = 0; num < roundInfo.summonData.Length; num++)
        {
            number = roundInfo.summonData[num].number;
            summonInfo = SummonInfoManager.summonInfo[number];
            level = roundInfo.summonData[num].level;

            Setting.EnemyDeck.Add(GetUnitData(summonInfo, level));
        }
    }

    private static bool HasChangedMyDeck(int[] now)
    {
        if (Setting.MyDeck.Count < 4)
            return true;

        for (int index = 0; index < now.Length; index++)
        {
            if (now[index] != Setting.MyDeck[index].Number)
                return true;
        }
        return false;
    }

    private static UnitData GetUnitData(SummonInfo summonInfo, int level)
    {
        return new UnitData(summonInfo.number,
                             level,
                             summonInfo.pattern,
                             summonInfo.fieldType,
                             summonInfo.attackType,
                             summonInfo.colliderCenter,
                             summonInfo.colliderSize,
                             summonInfo.attackSpeed,
                             summonInfo.airDamage * summonInfo.growthRateOfAirDam * level,
                             summonInfo.groundDamage * summonInfo.growthRateOfGroundDam * level,
                             summonInfo.health * summonInfo.growthRateOfHealth * level,
                             summonInfo.armor * summonInfo.growthRateOfArmor * level,
                             summonInfo.isBuff,
                             ConstructBuff(summonInfo.number),
                             SummonSpriteManager.sprite[summonInfo.number]
                             );
    }

    private static Buff ConstructBuff(int number)
    {
        BuffInfo buffInfo = BuffInfoManager.BuffInfo[number];
        return new Buff(buffInfo.name,
                         buffInfo.description,
                         buffInfo.armor,
                         buffInfo.distance,
                         buffInfo.groundDmgMultiple,
                         buffInfo.airDmgMultiple,
                         buffInfo.speedMultiple,
                         buffInfo.runTime,
                         buffInfo.hasLimitTime
                        );
    }
}