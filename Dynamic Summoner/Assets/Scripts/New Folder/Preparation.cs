using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preparation
{
    private static Preparation preparation;
    private static Preparation Instance { get { return preparation ?? new Preparation(); } }

    private Vector2 mySummonerPosition;
    private Vector2 enemySummonerPosition;

    private Preparation()
    {
        mySummonerPosition = new Vector2(-55.8f, 2.3f);
        enemySummonerPosition = new Vector2(-44.3f, 2.3f);
    }

    public static void Initialize(GameMode mode)
    {
        InitMyDeck();
        InitEnemyDeck(mode);
        InitMySummoner();
        InitEnemySummoner(mode);
    }

    public static void InitMyDeck()
    {
        DeckSetting.SetMyDeck();
    }

    public static void InitEnemyDeck(GameMode mode)
    {
        if (mode == GameMode.Single)
            DeckSetting.SetEnemyDeckOfSingleMode(UserData.SingleRound);
        else if (mode == GameMode.Challenge)
            DeckSetting.SetEnemyDeckOfChallengeMode(UserData.ChallengeRound);
    }

    public static void InitMySummoner()
    {
        SpawnManager.MySummoner.Initiate(UserData.Level);
        SpawnManager.MySummoner.SetPosition(MySummonerPosition);
        SpawnManager.MySummoner.SetSprite(SingleBattleInfoManager.roundInfo[UserData.SingleRound].summonerSprite);
        SpawnManager.MySummoner.SetActive(true);
    }

    public static void InitEnemySummoner(GameMode mode)
    {
        if (mode == GameMode.Single)
            InitSingleEnemySummoner();
        if (mode == GameMode.Challenge)
            InitChallengeEnemySummoner();
    }

    // 나중에 스프라이트 어떻게 처리할건지 확인해야함
    public static void InitMultiEnemySummoner(int level, Sprite sprite)
    {
        SpawnManager.EnemySummoner.Initiate(level);
        SpawnManager.EnemySummoner.SetPosition(EnemySummonerPosition);
        SpawnManager.EnemySummoner.SetSprite(SingleBattleInfoManager.roundInfo[UserData.SingleRound].summonerSprite);
        SpawnManager.EnemySummoner.SetActive(true);
    }

    public static void InitEnemy()
    {
        int number;
        Vector2 position;
        RoundInfoOfSingleMode roundInfo = SingleBattleInfoManager.roundInfo[UserData.SingleRound];

        float x = Camera.main.transform.position.x;
        float y = Camera.main.transform.position.y;

        for (int index = 0; index < roundInfo.positionInfo.Length; index++)
        {
            number = roundInfo.positionInfo[index].number;
            position = roundInfo.positionInfo[index].position + new Vector2(x, y);
            Battle.InitEnemyBeforeBattle(number, position);
        }
    }

    private static void InitSingleEnemySummoner()
    {
        RoundInfoOfSingleMode roundInfo = SingleBattleInfoManager.roundInfo[UserData.SingleRound];

        SpawnManager.EnemySummoner.Initiate(roundInfo.summonerLevel);
        SpawnManager.EnemySummoner.SetPosition(EnemySummonerPosition);
        SpawnManager.EnemySummoner.SetSprite(roundInfo.summonerSprite);
        SpawnManager.EnemySummoner.SetActive(true);
    }

    private static void InitChallengeEnemySummoner()
    {
        RoundInfoOfChallengeMode roundInfo = ChallengeBattleInfoManager.roundInfo[UserData.ChallengeRound];

        SpawnManager.EnemySummoner.Initiate(roundInfo.summonerLevel);
        SpawnManager.EnemySummoner.SetPosition(EnemySummonerPosition);
        SpawnManager.EnemySummoner.SetSprite(roundInfo.summonerSprite);
        SpawnManager.EnemySummoner.SetActive(true);
    }

    private static Vector2 MySummonerPosition { get { return Instance.mySummonerPosition; } }
    private static Vector2 EnemySummonerPosition { get { return Instance.enemySummonerPosition; } }
}