using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using System.Threading.Tasks;

/// <summary>
/// Bronse(1-1499), Silver(1500-1999), Gold(2000-2499), Platinum(2500-2999), Diamond(3000-3499), Master(3500-3999), GrandMaster(4000+)
/// </summary>
public enum Tier { Bronse, Silver, Gold, Platinum, Diamond, Master, GrandMaster }
public enum CheckNewUser { Checking, NewUser, OldUser }

public class DBController
{

    public  static DBController Instance { get { return dbController ?? new DBController(); } }
    private static DBController dbController = null;

    private DatabaseReference rootDBReference;
    
    private string id;

    public static event Action CompleteInit;
    public CheckNewUser IsNewUser;

    //Firebase.Auth.FirebaseAuth auth;

    public DBController()
    {
        dbController = this;

        IsNewUser = CheckNewUser.Checking;
    }

    public void Init()
    {
        // 데이터베이스 주소 연결
        //FirebaseApp app = FirebaseApp.DefaultInstance;

        //app.SetEditorDatabaseUrl("https://dynamic-summoner-47605617.firebaseio.com");

        //Debug.Log("GetEditorDatabaseUrl : " + app.GetEditorDatabaseUrl());

        //if (app.Options.DatabaseUrl != null)
        //{
        //    app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        //}
        //else
        //{
        //    Debug.Log("app.Options.DatabaseUrl is null");
        //}

        //rootDBReference = FirebaseDatabase.DefaultInstance.RootReference;

        //id = ((PlayGamesLocalUser)Social.localUser).id;\

        //auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        //Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        //System.Uri photo_url = user.PhotoUrl;
        //Debug.Log("PhotoUrl : " + user.PhotoUrl);
        //Debug.Log("https://dynamic-summoner-47605617.firebaseio.com : ");
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://dynamic-summoner-47605617.firebaseio.com");
        //FirebaseApp.DefaultInstance.Options.DatabaseUrl = photo_url;

        id = "39uLmhypkba4XzXpxSL9RFD2glP2";
    }

    public void UserVerification()
    {
        // 원래 여기 위치가 아니다
        rootDBReference = FirebaseDatabase.DefaultInstance.RootReference;

        UnityEngine.Debug.Log("START UserVerification");

        rootDBReference.Child("Users")
            .GetValueAsync().ContinueWith((Task<DataSnapshot> task) =>
            {
                UnityEngine.Debug.Log("JsonValue : " + task.Result.GetRawJsonValue());

                if (task.IsCanceled || task.IsFaulted)
                {
                    UnityEngine.Debug.Log("GoogleIdToken Of Users is fail");
                }
                else if (task.IsCompleted)
                {
                    UnityEngine.Debug.Log("IsCompleted");
                    if (task.Result.ChildrenCount == 0)
                    {
                        UnityEngine.Debug.Log("task.Result.ChildrenCount == 0");
                        IsNewUser = CheckNewUser.NewUser;
                    }
                    else
                    {
                        foreach (var childSnapShot in task.Result.Children)
                        {
                            UnityEngine.Debug.Log("\n childSnapShot.Reference.Key : " + childSnapShot.Reference.Key + "\n");

                            if (childSnapShot.Reference.Key == id)
                            {
                                UnityEngine.Debug.Log("UserVerification sccess");
                                IsNewUser = CheckNewUser.OldUser;
                            }
                        }

                        if (IsNewUser != CheckNewUser.OldUser)
                            IsNewUser = CheckNewUser.NewUser;

                        InitUserInfo();
                    }
                }
            });
    }


    private void InitUserInfo()
    {
        if (dbController.IsNewUser == CheckNewUser.NewUser)
        {
            // 닉네임을 입력 받아야한다. 일딴 임의의 닉네임을 사용한다.
            UnityEngine.Debug.Log("InitUserInfo = NewUser");
            
            // 임의로 닉네임 설정
            string nickName = "newUser";
            RegisterNewUser(nickName);
            InitNewUserCards();

        }
        else if (dbController.IsNewUser == CheckNewUser.OldUser)
        {
            UnityEngine.Debug.Log("InitUserInfo = Olduser");
            GetUserValue();
            GetUserSummonData();
        }

        // 임시로 3초를 설정하였고, 나중에 수정해야한다.
        new WaitForSeconds(3f);
        Setting.GameState = GameState.Main;
    }

    private void InitNewUserCards()
    {
        
    }

    private void GetUserSummonData()
    {
        rootDBReference
            .Child("SummonData")
            .Child(id)
            .GetValueAsync()
            .ContinueWith((Task<DataSnapshot> task) =>
            {
                UnityEngine.Debug.Log("GetSummonData");
                int index = 0;
                foreach(DataSnapshot summonData in task.Result.Children)
                {

                    Setting.summonDataByNumber.Add(index, new SummonData(int.Parse(summonData.Child("SummonNumber").Value.ToString()),
                                                                         int.Parse(summonData.Child("Experience").Value.ToString()),
                                                                         int.Parse(summonData.Child("Level").Value.ToString())));
                    index++;
                }

            });
    }

    private void RegisterNewUser(string nickName)
    {

        JSONObject information = new JSONObject(JSONObject.Type.OBJECT);
        information.AddField("NickName", nickName);

        JSONObject arrayOfDeck = new JSONObject(JSONObject.Type.ARRAY);
        arrayOfDeck.Add(0);
        arrayOfDeck.Add(1);
        arrayOfDeck.Add(2);
        arrayOfDeck.Add(3);

        JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
        data.AddField("ChallengeRound", 1);
        data.AddField("Experience", 0);
        data.AddField("GameMoney", 0);
        data.AddField("Level", 1);
        data.AddField("RealMoney", 0);
        data.AddField("SingleRound", 1);
        data.AddField("Tier", "Gold");
        data.AddField("TierScore", 2000);
        data.AddField("Deck", arrayOfDeck);

        JSONObject user = new JSONObject(JSONObject.Type.OBJECT);
        user.AddField("Information", information);
        user.AddField("Data", data);

        rootDBReference.Child("Users").Child(id).SetRawJsonValueAsync(user.Print()).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                UnityEngine.Debug.Log("RegisterNewuser Fail \n");
            }
            else
            {
                UnityEngine.Debug.Log("RegisterNewuser Success \n");
                GetUserValue();
            }
        });
    }

    //public void GetDeckInfo()
    //{
    //    Debug.Log("Start GetDeckInfo \n");

    //    DatabaseReference userDeck = FirebaseDatabase.DefaultInstance
    //                                                 .GetReference("Summons")
    //                                                 .Child(id);
    //    for(int i = 0; i < 4; i++)
    //    {
    //        Debug.Log("\n userDTO.Deck[" + i + "]" + userInfo.Deck[i].ToString());
    //        userDeck.Child(userInfo.Deck[i].ToString()).ValueChanged += HandleDeckValueChanged;
    //    }

    //    FirebaseDatabase.DefaultInstance
    //        .GetReference("Summons")
    //        .Child(id)
    //        .GetValueAsync().ContinueWith(task =>
    //        {
    //            if (task.IsCanceled || task.IsFaulted)
    //            {
    //                text.text += "GetDeckInfo Fail \n";
    //            }
    //            else if (task.IsCompleted)
    //            {
    //                text.text += "GetDeckInfo Success \n";
    //                DataSnapshot deckSnapshot = task.Result;

    //                //SetDeckInfo(deckSnapshot);

    //                // 사용자 데이터 가지고 오면 Init 씬을 마치고 Main 씬으로 넘어간다.
    //                //SceneManager.LoadScene("Scenes/SingleBattle");
    //            }
    //        });

    //}

    //private void HandleDeckValueChanged(object sender, ValueChangedEventArgs args)
    //{
    //    if (args.DatabaseError != null)
    //    {
    //        Debug.Log(args.DatabaseError.Message);

    //        Debug.Log("\n HandleDeckValueChanged Error : " + args.DatabaseError.Message + "\n");
    //    }

    //    Debug.Log("\n Input DeckValueChanged \n");

    //    DataSnapshot deckSnapshot = args.Snapshot;
    //    SetDeckInfo(deckSnapshot);
    //}

    //private void SetDeckInfo(DataSnapshot deckSnapshot)
    //{
    //    int summonNumber = deckSnapshot.Key;
    //    int deckNum = SummonManager.GetSummonData(summonNumber);

    //    deckDTO.SummonNumber[deckNum] = userInfo.Deck[deckNum];
    //    deckDTO.Pattern[deckNum] = deckSnapshot.Child("pattern").Value.ToString();
    //    deckDTO.Power[deckNum] = (float)deckSnapshot.Child("power").Value;
    //    deckDTO.Armor[deckNum] = (float)deckSnapshot.Child("armor").Value;
    //    deckDTO.AttackSpeed[deckNum] = (float)deckSnapshot.Child("attackSpeed").Value;
    //    deckDTO.Health[deckNum] = (float)deckSnapshot.Child("health").Value;

    //    //text.text += "\n" + deckDTO.SummonNumber[deckNum];
    //    //text.text += "\n" + deckDTO.Pattern[deckNum];
    //    //text.text += "\n" + deckDTO.Power[deckNum];
    //    //text.text += "\n" + deckDTO.Armor[deckNum];
    //    //text.text += "\n" + deckDTO.AttackSpeed[deckNum];
    //    //text.text += "\n" + deckDTO.Health[deckNum] + "\n";
    //    Debug.Log("EndSetDeckInfo Time : " + Time.time);
    //    Debug.Log("End SetDeckInfo");
    //    //text.text += "\n SetDeckInfo Execute \n";
    //}

	public void GetUserValue()
	{
        UnityEngine.Debug.Log("Start!!!! GetUserInfo \n");

        FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .Child(id)
            .ValueChanged += HandleUserValueChanged;

        //FirebaseDatabase.DefaultInstance
        //    .GetReference("Users")
        //    .Child(id)
        //    .GetValueAsync().ContinueWith(task =>
        //    {
        //        if (task.IsCanceled || task.IsFaulted)
        //        {
        //            text.text += "GetUserInfo Fail \n";
        //        }
        //        else if (task.IsCompleted)
        //        {
        //            text.text += "GetUserInfo Success \n";
        //            DataSnapshot userSnapshot = task.Result;
        //            SetUserInfo(userSnapshot);

        //            // 사용자 데이터 가지고 오면 Init 씬을 마치고 Main 씬으로 넘어간다.
        //            // 이 부분을 GetDeckInfo() 실행 이후로 이동한다.
        //            SceneManager.LoadScene("Scenes/Main");
        //        }
        //    });
    }

	private void HandleUserValueChanged(object sender, ValueChangedEventArgs args)
	{
        if (args.DatabaseError != null)
        {
            UnityEngine.Debug.LogError(args.DatabaseError.Message);
            UnityEngine.Debug.Log("\n HandleValueChanged Error : " + args.DatabaseError.Message + "\n");
        }

        UnityEngine.Debug.Log("START!!!! HandleUserValueChanged");
        DataSnapshot userSnapshot = args.Snapshot;
        UnityEngine.Debug.Log("User DataSnapshot : " + userSnapshot.GetRawJsonValue());
        
        SetUserInfo(userSnapshot);
    }

    private void SetUserInfo(DataSnapshot userSnapshot)
    {
        UnityEngine.Debug.Log("START!!!! SetUserInfo");

        UserInfo.nickName = userSnapshot.Child("Information").Child("NickName").Value.ToString();

        DataSnapshot userData = userSnapshot.Child("Data");
        UserData.Level             = Int32.Parse(userData.Child("Level").Value.ToString());
        UserData.GameMoney         = Int32.Parse(userData.Child("GameMoney").Value.ToString());
        UserData.RealMoney         = Int32.Parse(userData.Child("RealMoney").Value.ToString());
        UserData.Experience        = Int32.Parse(userData.Child("Experience").Value.ToString());
        UserData.SingleRound       = Int32.Parse(userData.Child("SingleRound").Value.ToString());
        UserData.ChallengeRound    = Int32.Parse(userData.Child("ChallengeRound").Value.ToString());
        UserData.TierScore         = Int32.Parse(userData.Child("TierScore").Value.ToString());
        UserData.Tier              = userData.Child("Tier").Value.ToString();

        DataSnapshot userDeck = userData.Child("Deck");
        UserData.SummonNumbersOfDeck[0] = Int32.Parse(userDeck.Child("0").Value.ToString());
        UserData.SummonNumbersOfDeck[1] = Int32.Parse(userDeck.Child("1").Value.ToString());
        UserData.SummonNumbersOfDeck[2] = Int32.Parse(userDeck.Child("2").Value.ToString());
        UserData.SummonNumbersOfDeck[3] = Int32.Parse(userDeck.Child("3").Value.ToString());

        UnityEngine.Debug.Log("this.userData.summonNumberOfDeck[0] : " + UserData.SummonNumbersOfDeck[0]);
        UnityEngine.Debug.Log("this.userData.summonNumberOfDeck[1] : " + UserData.SummonNumbersOfDeck[1]);
        UnityEngine.Debug.Log("this.userData.summonNumberOfDeck[2] : " + UserData.SummonNumbersOfDeck[2]);
        UnityEngine.Debug.Log("this.userData.summonNumberOfDeck[3] : " + UserData.SummonNumbersOfDeck[3]);

        UnityEngine.Debug.Log("\nSetUserInfo Execute\n"); 
    }                                         
}