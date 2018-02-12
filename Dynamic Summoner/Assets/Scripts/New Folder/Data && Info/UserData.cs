using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    private static UserData userData;
    private static UserData instance { get { if (userData == null) return new UserData(); return userData; } }

    public UserData()
    {
        userData = this;
        SummonNumbersOfDeck = new int[4];
    }

    private string tier;
    private int gameMoney;
    private int realMoney;
    private int level;
    private int tierScore;
    private int experience;
    private int[] summonNumberOfDeck;
    private int singleRound;
    private int challengeRound;

    public static string Tier
    {
        get
        {
            return instance.tier;
        }

        set
        {
            instance.tier = value;
        }
    }
    public static int GameMoney
    {
        get
        {
            return instance.gameMoney;
        }

        set
        {
            instance.gameMoney = value;
        }
    }
    public static int RealMoney
    {
        get
        {
            return instance.realMoney;
        }

        set
        {
            instance.realMoney = value;
        }
    }
    public static int Level
    {
        get
        {
            return instance.level;
        }

        set
        {
            instance.level = value;
        }
    }
    public static int TierScore
    {
        get
        {
            return instance.tierScore;
        }

        set
        {
            instance.tierScore = value;
        }
    }
    public static int Experience
    {
        get
        {
            return instance.experience;
        }

        set
        {
            instance.experience = value;
        }
    }
    public static int[] SummonNumbersOfDeck
    {
        get
        {
            return instance.summonNumberOfDeck;
        }

        set
        {
            instance.summonNumberOfDeck = value;
        }
    }
    public static int SingleRound
    {
        get
        {
            return instance.singleRound;
        }

        set
        {
            instance.singleRound = value;
        }
    }
    public static int ChallengeRound
    {
        get
        {
            return instance.challengeRound;
        }

        set
        {
            instance.challengeRound = value;
        }
    }
}