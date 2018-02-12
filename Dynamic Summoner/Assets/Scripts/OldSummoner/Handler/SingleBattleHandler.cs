//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SingleBattleHandler : BattleHandler {

//    [SerializeField] private SingleBattleInfoManager singleBattleInfoManager;

//    private int countForEnemy;
//    private int maxCountForEnemy;

//    private void Awake()
//    {
//        deckDataOfPattern   = new Dictionary<string, UnitData>();
//        playerDeck          = new List<UnitData>();
//        enemyDeck           = new List<UnitData>();
//        userData            = UserData.Instance;

//        countForEnemy = 0;
//        maxCountForEnemy = 200;
//    }

//    private void Update()
//    {
//        if (GameHandler.gameState != GameState.Single)
//            return;

//        countForEnemy++;

//        if (countForEnemy != maxCountForEnemy)
//            return;

//        SummonEnemy();
//        countForEnemy = 0;

//    }

//    public override void BattleStart()
//    {
//        if (WhetherTheDeckHasChanged())
//        {
//            InitPlayerDeck();
//            InitEnemyDeck();
//        }
        
//        InitDeckDataOfPattern();
//        patternLock.battleHandler = this;
//    }

//    public override void InputPattern(string pattern)
//    {
//        Debug.Log("START!!!! InputPattern");

//        if (!deckDataOfPattern.ContainsKey(pattern))
//            return;

//        Debug.Log("ContainsKey!!!!");

//        // 패턴이 있는경우에만 실행
//        UnitData    playerDeckData  = deckDataOfPattern[pattern];
//        Vector3     position        = playerPositionResercher.GetPosition(playerDeckData);

//        Debug.Log("playerSpawn.summonSpawn : " + enemySpawn.summonSpawn);

//        playerSpawn.Summon(playerDeckData, position, enemySpawn.summonSpawn, playerHealthState);
//    }

//    private void InitEnemyDeck()
//    {
//        deckDataManager.SetEnemyDeckOfSingleMode(enemyDeck, userData.SingleRound);
//    } 

//    private void SummonEnemy()
//    {
//        int index = Random.Range(0, enemyDeck.Count);

//        UnitData enemyDeckData = enemyDeck[index];
//        Vector3 position = enemyPositionResercher.GetPosition(enemyDeckData);

//        Debug.Log("playerSpawn.summonSpawn : " + playerSpawn.summonSpawn);

//        enemySpawn.Summon(enemyDeckData, position, playerSpawn.summonSpawn, enemyHealthState);
//    }
//}