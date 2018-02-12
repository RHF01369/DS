using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhotonMultiBattleRelay
{
    static PhotonMultiBattleRelay()
    {
        isPlaying = false;
        isDisconnected = false;
    }

    public static bool isPlaying { get; set; }
    public static bool isDisconnected { get; set; }
    public static event Action myDisconnectAction;
    public static event Action myReConnectAction;
    public static event Action enemyDisconnectAction;
    public static event Action enemyReconnectAction;

    private static PhotonPeer peer;

    public static void SetPeerOption()
    {
        peer = PhotonNetwork.networkingPeer;
        peer.DisconnectTimeout = 50000;
    }

    // 나의 인터넷 연결이 멈췄을 때
    public static void StopMyAttackCoroutine()
    {
        myDisconnectAction();
    }

    // 상대방의 인터넷 연결이 멈췄을 때
    public static void StartEnemyAttackCoroutine()
    {
        enemyDisconnectAction();
    }

    // 나의 인터넷이 다시연결 됐을 때
    public static void ActiveMyReConnect()
    {
        // 지금까지 상대가 실행했던 명령 데이터를 요청한다.

        myReConnectAction();
    }

    // 상대방의 인터넷이 다시연결 됐을 때 ( 재연결은 상대방의 명령 데이터 요청에 의해서 시작된다. )
    public static void StopEnemyAttackCoroutine()
    {
        // 내쪽에서 실행하던 상대방의 공격명령을 중지시킨다.

        enemyReconnectAction();
    }
}