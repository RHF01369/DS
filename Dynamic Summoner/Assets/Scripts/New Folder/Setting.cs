using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Init, Main, Battle }

public enum GameMode { Ready, Single, Challenge, Multi }

public struct GameSetting
{
    public GameState gameState;
    public GameMode gameMode;
}

public struct BattleSetting
{
    public float attackCool;
    public float maxSpeed;
    public int maxArmor;
}

public struct SummonDeck
{
    public List<UnitData> My;
    public List<UnitData> Enemy;
}

public class Setting : MonoBehaviour
{
    private static Setting instance;

    [SerializeField] GameSetting gameSetting;
    [SerializeField] BattleSetting battleSetting;
    [SerializeField] GameObject battleZone;

    private SummonDeck summonDeck;
    private Dictionary<int, SummonData> summonDataByNumber;
    private int deckCount;

    private void Awake()
    {
        instance = this;
        summonDeck.My = new List<UnitData>();
        summonDeck.Enemy = new List<UnitData>();
        summonDataByNumber = new Dictionary<int, SummonData>();
        deckCount = 4;
    }

    public static GameState GameState
    {
        get { return instance.gameSetting.gameState; }
        set { instance.gameSetting.gameState = value; }
    }

    public static GameMode GameMode
    {
        get { return instance.gameSetting.gameMode; }
        set { instance.gameSetting.gameMode = value; }
    }

    public static float AttackCool
    {
        get { return instance.battleSetting.attackCool; }
    }

    public static float MaxSpeed
    {
        get { return instance.battleSetting.maxSpeed; }
    }

    public static int MaxArmor
    {
        get { return instance.battleSetting.maxArmor; }
    }

    public static List<UnitData> MyDeck
    {
        get { return instance.summonDeck.My; }
    }

    public static List<UnitData> EnemyDeck
    {
        get { return instance.summonDeck.Enemy; }
    }

    public static Dictionary<int, SummonData> SummonDataByNumber
    {
        get { return instance.summonDataByNumber; }
    }

    public static GameObject BattleZone { get { return instance.battleZone; } } 

    public static int DeckCount { get { return instance.deckCount; } }
}