using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public enum PacketType
{
    ClientData, PlayerNumber,
    Attack, Summon, Skill,
    EnemyDeckData, DeckDataRequest,
    ReadyComplete, BattleStart,
    Synchronizaion, ReconnectionSync,
    Disconnection, Reconnection,
    SynchronizationComplete,
};

public static class SocketInfo
{
    public const int PacketSizeStartIndex = 5;
    public const int ExecutionOrderStartIndex = 9;
    public const byte PacketStartNumber = 255;
}


public class Client
{
    private int Port = 7000;

    private Socket socket;
    private IPEndPoint ipEndPoint;

    private byte[][] receiveBuffer;
    private byte[] chainingReceiveBuffer;
    private byte[] sendBuffer;

    public int playerNumber { get; private set; }

    private Dictionary<PacketType, Action<byte[]>> packetTypeToAction;
    private SocketAsyncEventArgs socketAsyncEventArgs0;
    private SocketAsyncEventArgs socketAsyncEventArgs1;

    public Client()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.179"), Port);

        receiveBuffer = new byte[2][];
        receiveBuffer[0] = new byte[Setting.MaxSize];
        receiveBuffer[1] = new byte[Setting.MaxSize];
        chainingReceiveBuffer = new byte[Setting.MaxSize];
        sendBuffer = new byte[Setting.MaxSize];

        socketAsyncEventArgs0 = new SocketAsyncEventArgs();
        socketAsyncEventArgs0.SetBuffer(receiveBuffer[0], 0, receiveBuffer[0].Length);
        socketAsyncEventArgs0.Completed += ReceivePacket0;

        socketAsyncEventArgs1 = new SocketAsyncEventArgs();
        socketAsyncEventArgs1.SetBuffer(receiveBuffer[1], 0, receiveBuffer[1].Length);
        socketAsyncEventArgs1.Completed += ReceivePacket1;

        packetTypeToAction = new Dictionary<PacketType, Action<byte[]>>()
        {
            { PacketType.PlayerNumber,               ReceivePlayerNumber                 },
            { PacketType.Attack,                     ReceiveAttack                       },
            { PacketType.Summon,                     ReceiveSummon                       },
            { PacketType.Skill,                      ReceiveSkill                        },
            { PacketType.DeckDataRequest,            ReceiveDeckDataRequest              },
            { PacketType.EnemyDeckData,              ReceiveEnemyDeckData                },
            { PacketType.BattleStart,                ReceiveBattleStart                  },
            { PacketType.Synchronizaion,             ReceiveSynchronization              },
            { PacketType.Disconnection,              ReceiveEnemyDisconnection           },
            { PacketType.Reconnection,               ReceiveEnemyReconnection            },
            { PacketType.ReconnectionSync,           ReceiveEnemyAllSynchronizationData  },
            { PacketType.SynchronizationComplete,    ReceiveEnemySynchronizationComplete },
        };

        MultiBattleDataManager.Initialize(this);
    }

    private void ReceivePacket0(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log(LogType.Trace, "ReceivePacket-0 : {0} :", ByteConverter.ToInt(e.Buffer, 1));

        //throw new NotImplementedException();
        socket.ReceiveAsync(socketAsyncEventArgs1);
        ClassifyReceivedPacket(e.Buffer);
    }

    private void ReceivePacket1(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log(LogType.Trace, "ReceivePacket-1 : {0} :", ByteConverter.ToInt(e.Buffer, 1));

        //throw new NotImplementedException();
        socket.ReceiveAsync(socketAsyncEventArgs0);
        ClassifyReceivedPacket(e.Buffer);
    }

    public void Start()
    {   
        socket.Connect(ipEndPoint);

        Setting.GameState = GameState.Battle;
        Setting.GameMode = GameMode.Multi;

        socket.ReceiveAsync(socketAsyncEventArgs0);

        MultiBattle.Instance.Initialize(this);
        MultiBattle.Instance.StartMakingEnemyDeck();
    }

    private void ClassifyReceivedPacket(byte[] buffer)
    {
        PacketType packetType = (PacketType)ByteConverter.ToInt(buffer, 1);

        Debug.Log(LogType.Test, "ClassifyReceivedPacket {0}", packetType);

        packetTypeToAction[packetType](buffer);
    }

    private void ReceivePlayerNumber(byte[] buffer)
    {
        playerNumber = ByteConverter.ToInt(buffer, 9);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveDeckDataRequest(byte[] buffer)
    {
        Debug.Log(LogType.Test, "ReceiveDeckDataRequest");

        SendDeckData();

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveEnemyDeckData(byte[] buffer)
    {
        Debug.Log(LogType.Test, "ReceiveEnemyDeckData");

        int startIndex = 9;
        MultiBattleDataManager.enemyDeckData.enemyLevel = ByteConverter.ToInt(buffer, ref startIndex);
        for(int index = 0; index < Setting.DeckCount; index++)
        {
            MultiBattleDataManager.enemyDeckData.summonNumber[index] = ByteConverter.ToInt(buffer, ref startIndex);
            MultiBattleDataManager.enemyDeckData.summonLevel[index] = ByteConverter.ToInt(buffer, ref startIndex);
        }

        MultiBattleDataManager.enemyDeckData.isReceived = true;

        SendReadyCompletePacket();

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveBattleStart(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveBattleStart");

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveAttack(byte[] buffer)
    {
        int executionOrder = ByteConverter.ToInt(buffer, SocketInfo.ExecutionOrderStartIndex);

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Attack,
            order = executionOrder,
        };

        AddExecutionDataToManager(executionData);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveSummon(byte[] buffer)
    {
        int startIndex = SocketInfo.ExecutionOrderStartIndex;
        int executionOrder = ByteConverter.ToInt(buffer, ref startIndex);
        int playerNumber = ByteConverter.ToInt(buffer, ref startIndex);
        Debug.Log(LogType.Test, "<Color=Blue> this playerNumber : " + this.playerNumber + " , packet playerNumber : " + playerNumber + "</Color>");
        bool isMine = this.playerNumber == playerNumber ? true : false;

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        int summonDeckIndex = ByteConverter.ToInt(buffer, ref startIndex);
        float xPos = GetSummonXPosition(ByteConverter.ToFloat(buffer, ref startIndex), isMine);
        float yPos = ByteConverter.ToFloat(buffer, ref startIndex);

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Summon,
            order = executionOrder,
            isMine = isMine,

            summonDeckIndex = summonDeckIndex,
            position = new Vector3(xPos, yPos)
        };

        AddExecutionDataToManager(executionData);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private float GetSummonXPosition(float xPos, bool isMine)
    {
        if (isMine)
            return xPos;

        return (2 * Setting.battleZoneXPosition - xPos);
    }

    private void ReceiveSkill(byte[] buffer)
    {
        //AddExecutionDataToManager(executionData);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveSynchronization(byte[] buffer)
    {
        Debug.Log(LogType.Test, "<Color=Green> ReceiveSynchronization </Color>");

        Array.Copy(buffer, MultiBattleDataManager.synchronizationData, MultiBattleDataManager.synchronizationData.Length);
        MultiBattleDataManager.canSynchronize = true;

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void AddExecutionDataToManager(ExecutionData executionData)
    {
        if (MultiBattleDataManager.executionDataOrder == executionData.order)
            MultiBattleDataManager.EnqueueExecutionData(executionData);

        if (MultiBattleDataManager.executionDataOrder < executionData.order)
            MultiBattleDataManager.AddOutOfSequenceData(executionData);
    }

    private void ReceiveEnemyDisconnection(byte[] buffer)
    {
        MultiBattle.Instance.enemyNetworkingState = EnemyNetworkingState.Disconnected;

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveEnemyReconnection(byte[] buffer)
    {
        MultiBattle.Instance.enemyNetworkingState = EnemyNetworkingState.Reconnected;

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveEnemyAllSynchronizationData(byte[] buffer)
    {
        Debug.Log(LogType.Test, "<Color=Green> ReceiveReconnectionSynchronization </Color>");
        
        Array.Copy(buffer, MultiBattleDataManager.synchronizationData, MultiBattleDataManager.synchronizationData.Length);

        MultiBattle.Instance.enemyNetworkingState = EnemyNetworkingState.Synchronizating;

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void ReceiveEnemySynchronizationComplete(byte[] buffer)
    {
        MultiBattle.Instance.enemyNetworkingState = EnemyNetworkingState.Connected;
        MultiBattle.Instance.myNetworkingState = MyNetworkingState.Connected;

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, SocketInfo.PacketSizeStartIndex));
    }

    private void CheckChainingPacket(byte[] buffer, int startIndex)
    {
        if (buffer[startIndex] != SocketInfo.PacketStartNumber)
        {
            Array.Clear(buffer, 0, buffer.Length);
            Array.Clear(chainingReceiveBuffer, 0, chainingReceiveBuffer.Length);
            return;
        }

        Debug.Log(LogType.Trace, "CheckChainingPacket");

        Array.Copy(buffer, startIndex, chainingReceiveBuffer, 0, buffer.Length - startIndex);
        ClassifyReceivedPacket(chainingReceiveBuffer);
    }


    public void SendClientData(string nickName, int rankScore)
    {
        Debug.Log(LogType.Trace, "SendClientData");

        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.ClientData, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromString(nickName, sendBuffer, ref packetSize);
        ByteConverter.FromInt(rankScore, sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, SocketInfo.PacketSizeStartIndex);

        SendPacket(packetSize);
    }

    private void SendDeckData()
    {
        Debug.Log(LogType.Test, "SendDeckData");

        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.EnemyDeckData, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(UserData.Level, sendBuffer, ref packetSize);

        for (int num = 0; num < Setting.MyDeck.Count; num++)
        {
            ByteConverter.FromInt(Setting.MyDeck[num].Number, sendBuffer, ref packetSize);
            ByteConverter.FromInt(Setting.MyDeck[num].Level, sendBuffer, ref packetSize);
        }

        ByteConverter.FromInt(packetSize, sendBuffer, SocketInfo.PacketSizeStartIndex);

        SendPacket(packetSize);
    }

    public void SendSynchronizationPacket()
    {
        Debug.Log(LogType.Test, "<Color=red> SendSynchronizationPacket </Color>");

        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.Synchronizaion, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);

        SpawnManager.CopyUnitDatasToBuffer(sendBuffer, false, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, SocketInfo.PacketSizeStartIndex);

        SendPacket(packetSize);
    }

    public void SendSummonPacket(int index)
    {
        Vector2 position = PositionResearcher.GetPosition(Setting.MyDeck[index], true);

        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.Summon, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(playerNumber, sendBuffer, ref packetSize);
        ByteConverter.FromInt(index, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.x, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.y, sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, SocketInfo.PacketSizeStartIndex);

        SendPacket(packetSize);
    }

    public void SendSkillPacket()
    {
        // 미구현
    }

    private void SendReadyCompletePacket()
    {
        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.ReadyComplete, sendBuffer, ref packetSize);
        ByteConverter.FromInt(packetSize + sizeof(int), sendBuffer, ref packetSize);

        SendPacket(packetSize);
    }

    

    public void SendReconnectionSynchronization()
    {
        Debug.Log(LogType.Test, "<Color=red> SendReconnectionSynchronization </Color>");

        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.ReconnectionSync, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);

        SpawnManager.CopyUnitDatasToBuffer(sendBuffer, true, ref packetSize);

        MultiBattleDataManager.CopyExecutionDatasToBuffer(sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, SocketInfo.PacketSizeStartIndex);

        SendPacket(packetSize);
    }

    public void SendEnemyCompleteSynchronization()
    {
        int packetSize = 1;
        sendBuffer[0] = SocketInfo.PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.SynchronizationComplete, sendBuffer, ref packetSize);
        ByteConverter.FromInt(packetSize + sizeof(int), sendBuffer, ref packetSize);

        SendPacket(packetSize);
    }


    private void SendPacket(int packetSize)
    {
        try
        {
            socket.Send(sendBuffer, packetSize, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Debug.Log(LogType.Exception, "SocketException");

            MultiBattle.Instance.myNetworkingState = MyNetworkingState.Disconnected;
            MultiBattleDataManager.ClearExecutionQueue();
            MultiBattleDataManager.ClearOutOfSequenceDatas();
            MultiBattleDataManager.ResetExecutionDataOrder();

            TryReconnection();
        }
    }

    private void TryReconnection()
    {
        try
        {
            socket.Connect(ipEndPoint);
            socket.ReceiveAsync(socketAsyncEventArgs0);
            SendClientData(UserInfo.nickName, UserData.TierScore);
        }
        catch(SocketException e)
        {
            Debug.Log(LogType.Exception, "TryReconnection");

            Thread.Sleep(200);
            TryReconnection();
        }
    }
}