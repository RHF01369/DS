
using ExitGames.Client.Photon;

using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance { get; set; }

    /// <summary>The peer currently in use (to set the network simulation).</summary>

    private void Awake()
    {
        Instance = this;
        new Authentication().Start();
    }

    public void Start()
    {

    }

    public void StartSingleBattle()
    {
        Preparation.Initialize(GameMode.Single);
        Preparation.InitEnemy();

        UI.OnBattle();
        // 3 2 1...
        Battle.StartBattle(GameMode.Single);
    }

    public void StartChallengeBattle()
    {
        Preparation.Initialize(GameMode.Challenge);
    }

    public void StartMultiBattle()
    {
        Setting.MyDeck.Clear();
        Setting.EnemyDeck.Clear();

        Preparation.InitMyDeck();
        Preparation.InitMySummoner();

        Client client = new Client();
        client.Connect();
        client.SendClientData(UserInfo.nickName, UserData.TierScore);

        UI.OnBattle();
    }

    public void Test()
    {
    }
    public void EnterShop()
    {

    }

    public void EnterMyHome()
    {

    }

    public void EnterSetting()
    {

    }
}