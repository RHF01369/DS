//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public enum GameState { Main, Single, Multi, Challenge, }

//public class GameHandler : MonoBehaviour {

//    public static GameHandler Instance { get; private set; }

//    [SerializeField] SingleBattleHandler    singleBattleHandler;
//    [SerializeField] ChallengeBattleHandler challengeBattleHandler;
//    [SerializeField] MultiBattleHandler     multiBattleHandler;
//    [SerializeField] ScreenHandler          screenManager;
    
//    public static GameState gameState { get; private set; }
//    private Authentication authentication;

//    private void Awake()
//    {
//        Instance = this;
//        authentication = new Authentication();
//    }

//    private void Start()
//    {
//        authentication.Start();
//        EnterMain();

//        // 스프라이트 화면 띄우기;
//    }

//    public void EnterMain()
//    {
//        gameState = GameState.Main;
//        screenManager.ChangeScreen(gameState);
//    }

//    public void EnterSingleBattle()
//    {
//        Debug.Log("START !!!! EnterSingleBattle");

//        singleBattleHandler.BattleStart();
//        gameState = GameState.Single;
//        screenManager.ChangeScreen(gameState);
//    }

//    public void EnterChallengeBattle()
//    {
//        //challengeBattleHandler.BattleStart();
//        gameState = GameState.Challenge;
//        screenManager.ChangeScreen(gameState);
//    }

//    public void EnterMultiBattle()
//    {
//        //multiBattleHandler.BattleStart();
//        gameState = GameState.Multi;
//        screenManager.ChangeScreen(gameState);
//    }
//}
