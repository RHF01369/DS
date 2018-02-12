// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonHandler.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3) || UNITY_2017
#define UNITY_MIN_5_4
#endif

using System;
using System.Collections;
using System.Diagnostics;
using ExitGames.Client.Photon;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using SupportClassPun = ExitGames.Client.Photon.SupportClass;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

/// <summary>
/// Internal Monobehaviour that allows Photon to run an Update loop.
/// </summary>
internal class PhotonHandler : MonoBehaviour
{
    public static PhotonHandler SP;

    public int updateInterval;  // time [ms] between consecutive SendOutgoingCommands calls

    public int updateIntervalOnSerialize;  // time [ms] between consecutive RunViewUpdate calls (sending syncs, etc)

    private int nextSendTickCount = 0;

    private int nextSendTickCountOnSerialize = 0;

    private static bool sendThreadShouldRun;

    private static Stopwatch timerToStopConnectionInBackground;

    protected internal static bool AppQuits;

    protected internal static Type PingImplementation = null;

    protected void Awake()
    {
        if (SP != null && SP != this && SP.gameObject != null)
        {
            GameObject.DestroyImmediate(SP.gameObject);
        }

        SP = this;
        DontDestroyOnLoad(this.gameObject);

        this.updateInterval = 1000 / PhotonNetwork.sendRate;
        this.updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;

        PhotonHandler.StartFallbackSendAckThread();
    }


    #if UNITY_MIN_5_4

    protected void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
        {
            PhotonNetwork.networkingPeer.NewSceneLoaded();
            PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
        };
    }

    #else

    /// <summary>Called by Unity after a new level was loaded.</summary>
    protected void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.networkingPeer.NewSceneLoaded();
        PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
    }

    #endif


    /// <summary>Called by Unity when the application is closed. Disconnects.</summary>
    protected void OnApplicationQuit()
    {
        PhotonHandler.AppQuits = true;
        PhotonHandler.StopFallbackSendAckThread();
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Called by Unity when the application gets paused (e.g. on Android when in background).
    /// </summary>
    /// <remarks>
    /// Sets a disconnect timer when PhotonNetwork.BackgroundTimeout > 0.1f. See PhotonNetwork.BackgroundTimeout.
    ///
    /// Some versions of Unity will give false values for pause on Android (and possibly on other platforms).
    /// </remarks>
    /// <param name="pause">If the app pauses.</param>
    protected void OnApplicationPause(bool pause)
    {
        if (PhotonNetwork.BackgroundTimeout > 0.1f)
        {
            if (timerToStopConnectionInBackground == null)
            {
                timerToStopConnectionInBackground = new Stopwatch();
            }
            timerToStopConnectionInBackground.Reset();

            if (pause)
            {
                timerToStopConnectionInBackground.Start();
            }
            else
            {
                timerToStopConnectionInBackground.Stop();
            }
        }
    }

    /// <summary>Called by Unity when the play mode ends. Used to cleanup.</summary>
    protected void OnDestroy()
    {
        //Debug.Log("OnDestroy on PhotonHandler.");
        PhotonHandler.StopFallbackSendAckThread();
        //PhotonNetwork.Disconnect();
    }

    protected void Update()
    {
        if (PhotonNetwork.networkingPeer == null)
        {
            Debug.LogError("NetworkPeer broke!");
            return;
        }

        if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated || PhotonNetwork.connectionStateDetailed == ClientState.Disconnected || PhotonNetwork.offlineMode)
        {
            return;
        }

        // the messageQueue might be paused. in that case a thread will send acknowledgements only. nothing else to do here.
        if (!PhotonNetwork.isMessageQueueRunning)
        {
            return;
        }

        
        bool doDispatch = true;
        while (PhotonNetwork.isMessageQueueRunning && doDispatch)
        {
            // DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
            Profiler.BeginSample("DispatchIncomingCommands");
            doDispatch = PhotonNetwork.networkingPeer.DispatchIncomingCommands();

            if (PhotonMultiBattleRelay.isPlaying)
                CheckDisconnect(doDispatch);

            Profiler.EndSample();
        }

        int currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);  // avoiding Environment.TickCount, which could be negative on long-running platforms
        if (PhotonNetwork.isMessageQueueRunning && currentMsSinceStart > this.nextSendTickCountOnSerialize)
        {
            PhotonNetwork.networkingPeer.RunViewUpdate();
            this.nextSendTickCountOnSerialize = currentMsSinceStart + this.updateIntervalOnSerialize;
            this.nextSendTickCount = 0;     // immediately send when synchronization code was running
        }

        currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);
        if (currentMsSinceStart > this.nextSendTickCount)
        {
            bool doSend = true;
            while (PhotonNetwork.isMessageQueueRunning && doSend)
            {
                // Send all outgoing commands
                Profiler.BeginSample("SendOutgoingCommands");
                doSend = PhotonNetwork.networkingPeer.SendOutgoingCommands();
                Profiler.EndSample();
            }

            this.nextSendTickCount = currentMsSinceStart + this.updateInterval;
        }
    }


    private enum ConnectState { None, Disconnect,  Connect } ;
    private int disconnectCheckingValue = 200;
    private int disconnectPlayerJudgmentValue = 100;
    private int dispatchIncomingFalseCount = 0;
    private int countAfterDisconnect = 0;
    private int preLastAckTime = 0;
    private ConnectState myNetworkState = ConnectState.None;
    private void CheckDisconnect(bool doDispatch)
    {
        Debug.Log("doDispatch : " + doDispatch);

        Debug.Log("PhotonNetwork.networkingPeer.LastSendAckTime : " + PhotonNetwork.networkingPeer.LastSendAckTime);

        if (doDispatch)
        {
            dispatchIncomingFalseCount = 0;

            // Disconnect 이후에 재접속이 되었다면
            if (IsReconnect())
                Reconnect();
        }   

        // 연결해제시 연결해제 플레이어를 판단하기까지 시간체크를 함
        if (PhotonMultiBattleRelay.isDisconnected)
            if (++countAfterDisconnect > disconnectPlayerJudgmentValue)
                JudegeDisconnectPlayer();


        // 네트워크 연결이 끊어졌는지 판단하고, 마지막 패킷전송시간을 확인해서 연결해제 플레이어를 판단할 수 있게한다.
        if (++dispatchIncomingFalseCount > disconnectCheckingValue)
        {
            PhotonMultiBattleRelay.isDisconnected = true;
            preLastAckTime = PhotonNetwork.networkingPeer.LastSendAckTime;

            Debug.Log("connect fail :" + preLastAckTime);
        }
    }

    private bool IsReconnect()
    {
        if (preLastAckTime == 0)
            return false;
        else
            return true;
    }
   
    private void Reconnect()
    {
        preLastAckTime = 0;
        countAfterDisconnect = 0;
        PhotonMultiBattleRelay.isDisconnected = false;
        if (myNetworkState == ConnectState.Connect)
        {
            PhotonMultiBattleRelay.StopEnemyAttackCoroutine();
            // 데이터 요청을 받는다, 상대방에서 연결해제 되어있는 동안 실행되었던 데이터를 보낸다.
        }
        else
        {
            // 데이터를 요청한다. 
            // 연결해제 되어있는 동안 실행되었던 데이터를 받아오고 이후에 공격명령 코루틴을 실행한다.
            //PhotonMultiBattleRelay.ActiveMyReConnect();

        }

        myNetworkState = ConnectState.None;

    }

    private void JudegeDisconnectPlayer()
    {
        if (!PhotonMultiBattleRelay.isDisconnected)
            return;

        if (PhotonNetwork.networkingPeer.LastSendAckTime > preLastAckTime)
        {
            Debug.Log("My Networking is connecting");
            myNetworkState = ConnectState.Connect;
            PhotonMultiBattleRelay.StartEnemyAttackCoroutine();
            // 코루틴을 실행해서 공격명령을 계속 진행한다. 이때 주기적인 전체 동기화는 하지 않는다.
        }
        else
        {
            Debug.Log("My Networking is disconnected");
            myNetworkState = ConnectState.Disconnect;
            PhotonMultiBattleRelay.StopMyAttackCoroutine();
            // 공격명령을 중지시키고 재연결이 이루어지면 받아야하는 패킷 순서를 보낸다.
            // 받아야하는 패킷 순서이후의 명령들은 지운다.
        }
    }


    protected void OnJoinedRoom()
    {
        PhotonNetwork.networkingPeer.LoadLevelIfSynced();
    }

    protected void OnCreatedRoom()
    {
        PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
    }

    public static void StartFallbackSendAckThread()
    {
	    #if !UNITY_WEBGL
        if (sendThreadShouldRun)
        {
            return;
        }

        sendThreadShouldRun = true;
        SupportClassPun.StartBackgroundCalls(FallbackSendAckThread);   // thread will call this every 100ms until method returns false
	    #endif
    }

    public static void StopFallbackSendAckThread()
    {
	    #if !UNITY_WEBGL
        sendThreadShouldRun = false;
	    #endif
    }

    /// <summary>A thread which runs independent from the Update() calls. Keeps connections online while loading or in background. See PhotonNetwork.BackgroundTimeout.</summary>
    public static bool FallbackSendAckThread()
    {
        if (sendThreadShouldRun && !PhotonNetwork.offlineMode && PhotonNetwork.networkingPeer != null)
        {
            // check if the client should disconnect after some seconds in background
            if (timerToStopConnectionInBackground != null && PhotonNetwork.BackgroundTimeout > 0.1f)
            {
                if (timerToStopConnectionInBackground.ElapsedMilliseconds > PhotonNetwork.BackgroundTimeout * 1000)
                {
                    if (PhotonNetwork.connected)
                    {
                        PhotonNetwork.Disconnect();
                    }
                    timerToStopConnectionInBackground.Stop();
                    timerToStopConnectionInBackground.Reset();
                    return sendThreadShouldRun;
                }
            }

            if (!PhotonNetwork.isMessageQueueRunning || PhotonNetwork.networkingPeer.ConnectionTime - PhotonNetwork.networkingPeer.LastSendOutgoingTime > 200)
            {
                PhotonNetwork.networkingPeer.SendAcksOnly();
            }
        }

        return sendThreadShouldRun;
    }


    #region Photon Cloud Ping Evaluation


    private const string PlayerPrefsKey = "PUNCloudBestRegion";

    internal static CloudRegionCode BestRegionCodeInPreferences
    {
        get
        {
            string prefsRegionCode = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (!string.IsNullOrEmpty(prefsRegionCode))
            {
                CloudRegionCode loadedRegion = Region.Parse(prefsRegionCode);
                return loadedRegion;
            }

            return CloudRegionCode.none;
        }
        set
        {
            if (value == CloudRegionCode.none)
            {
                PlayerPrefs.DeleteKey(PlayerPrefsKey);
            }
            else
            {
                PlayerPrefs.SetString(PlayerPrefsKey, value.ToString());
            }
        }
    }


    internal protected static void PingAvailableRegionsAndConnectToBest()
    {
        SP.StartCoroutine(SP.PingAvailableRegionsCoroutine(true));
    }


    internal IEnumerator PingAvailableRegionsCoroutine(bool connectToBest)
    {
        while (PhotonNetwork.networkingPeer.AvailableRegions == null)
        {
            if (PhotonNetwork.connectionStateDetailed != ClientState.ConnectingToNameServer && PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToNameServer)
            {
                Debug.LogError("Call ConnectToNameServer to ping available regions.");
                yield break; // break if we don't connect to the nameserver at all
            }

            Debug.Log("Waiting for AvailableRegions. State: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.Server + " PhotonNetwork.networkingPeer.AvailableRegions " + (PhotonNetwork.networkingPeer.AvailableRegions != null));
            yield return new WaitForSeconds(0.25f); // wait until pinging finished (offline mode won't ping)
        }

        if (PhotonNetwork.networkingPeer.AvailableRegions == null || PhotonNetwork.networkingPeer.AvailableRegions.Count == 0)
        {
            Debug.LogError("No regions available. Are you sure your appid is valid and setup?");
            yield break; // break if we don't get regions at all
        }

        PhotonPingManager pingManager = new PhotonPingManager();
        foreach (Region region in PhotonNetwork.networkingPeer.AvailableRegions)
        {
            SP.StartCoroutine(pingManager.PingSocket(region));
        }

        while (!pingManager.Done)
        {
            yield return new WaitForSeconds(0.1f); // wait until pinging finished (offline mode won't ping)
        }


        Region best = pingManager.BestRegion;
        PhotonHandler.BestRegionCodeInPreferences = best.Code;

        Debug.Log("Found best region: '" + best.Code + "' ping: " + best.Ping + ". Calling ConnectToRegionMaster() is: " + connectToBest);

        if (connectToBest)
        {
            PhotonNetwork.networkingPeer.ConnectToRegionMaster(best.Code);
        }
    }



    #endregion

}
