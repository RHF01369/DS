using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Threading;

public class GoogleAuth : MonoBehaviour
{
    private bool _authSuccess;
    private DBController dbController;
    private string googleIdToken;

    private void Awake()
    {
        _authSuccess = false;
        dbController = DBController.Instance;

        dbController.Init();
        dbController.UserVerification();
    }

    // Use this for initialization
    void Start()
    {
        UnityEngine.Debug.Log("GoogleAuth START()");

        StartCoroutine(DBAccess());
       // StartCoroutine(InitDeckDTO());

        //InitGooglePlayService();
        //Login();
    }

    //void InitGooglePlayService()
    //{
    //    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
    //        .RequestIdToken()
    //        .RequestServerAuthCode(false)
    //        .Build();
    //    PlayGamesPlatform.InitializeInstance(config);
    //    PlayGamesPlatform.DebugLogEnabled = true;
    //    PlayGamesPlatform.Activate();
    //}

    //private void Login()
    //{
    //    // 가입이 되어 있지 않을 때
    //    if (!Social.localUser.authenticated)
    //    {
    //        // 구글 계정을 선택하고 로그인
    //        Social.localUser.Authenticate((bool success, string error) =>
    //        {
    //            // 로그인 성공
    //            if (success)
    //            {
    //                // Firebase에 가입하기 위한 구글계정 토큰을 가지고 온다.
    //                googleIdToken = ((GooglePlayGames.PlayGamesLocalUser)Social.localUser).GetIdToken();

    //                googleIdToken = "g06714780888451533032";
    //                // 파이어베이스에 접속
    //                FirebaseAuth();
    //            }
    //            // 로그인 실패
    //            else
    //            {
    //                text.text += "로그인 실패 \n";
    //            }
    //        });
    //    }
    //    else
    //    {
    //        text.text += "Social.localUser.authenticated true \n";
    //    }
    //}

    
    //private void FirebaseAuth()
    //{
    //    Debug.Log("FirebaseAuth");

    //    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

    //    // 구글 토큰을 파이어베이스에 인증하기 위한 준비
    //    Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(googleIdToken, null);
        

    //    // 파이어베이스 접속 시도
    //    auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
    //    {
    //        if (task.IsCanceled || task.IsFaulted)
    //        {
    //            Debug.Log("FirebaseAuth Fail \n");
    //            return;
    //        }
    //        else
    //        {
    //            text.text += "FirebaseAuth Success \n";

    //            // 파이어베이스 접속 완료확인 -> 메인화면으로 넘어간다.
    //            /// User DB에 사용자 데이터가 있으면 데이터를 가지고 와서 Main 화면으로 전환, DB에 정보가 없으면 신규 가입절차를 진행한다.
    //            StartCoroutine(DBAccess());
    //            StartCoroutine(InitDeckDTO());
    //        }
    //    });
    //}
    
    IEnumerator DBAccess() {

        while (true) {

            if (dbController.IsNewUser == CheckNewUser.NewUser)
            {
                // 닉네임을 입력 받아야한다. 일딴 임의의 닉네임을 사용한다.
                UnityEngine.Debug.Log("CheckNewUser.NewUser");
                // 임의로 닉네임 설정
                string nickName = "newUser";
                //dbController.RegisterNewUser(nickName);
                yield break;
            }
            else if (dbController.IsNewUser == CheckNewUser.OldUser)
            {
                UnityEngine.Debug.Log("CheckNewUser.Olduser");
                dbController.GetUserValue();

                yield break;
            }
            yield return null;
        }
    }

    //IEnumerator InitDeckDTO()
    //{
    //    //DeckDTO deckDTO = DeckDTO.Instance;

    //    Debug.Log("InitDeckDTO() START");

    //    while (true)
    //    {
    //        if(string.IsNullOrEmpty(deckDTO.Pattern[3]) == false)
    //        {
    //            Debug.Log("InitDeckDTO Time : " + Time.time);
    //            // MainGameHandler.Instance.ActiveSingleGame();

    //            yield break;
    //        }
    //        yield return null;
    //    }
    //}

    //public void LogOut()
    //{
    //    Debug.Log("LogOut \n");

    //    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

    //    auth.SignOut();

    //    ((GooglePlayGames.PlayGamesPlatform)Social.Active).SignOut();
    //}

}