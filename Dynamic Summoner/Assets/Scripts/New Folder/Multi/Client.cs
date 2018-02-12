using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    PlayerNumber,
    Attack, Summon, Skill,
    EnemyDeckData, DeckDataRequest,
    ReadyComplete, BattleStart
};

public class Client
{
    private const int Port = 7000;

    private Socket receiveSocket;

    private byte[][] receiveBuffer;
    private byte[] sendBuffer;

    private int playerNumber;

    private Dictionary<PacketType, Action<byte[]>> packetTypeToAction;
    private SocketAsyncEventArgs socketAsyncEventArgs0;
    private SocketAsyncEventArgs socketAsyncEventArgs1;

    public Client()
    {
        receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        receiveBuffer = new byte[2][];
        receiveBuffer[0] = new byte[1024];
        receiveBuffer[1] = new byte[1024];
        sendBuffer = new byte[1024];

        socketAsyncEventArgs0 = new SocketAsyncEventArgs();
        socketAsyncEventArgs0.SetBuffer(receiveBuffer[0], 0, receiveBuffer[0].Length);
        socketAsyncEventArgs0.Completed += ReceivePacket0;

        socketAsyncEventArgs1 = new SocketAsyncEventArgs();
        socketAsyncEventArgs1.SetBuffer(receiveBuffer[1], 0, receiveBuffer[1].Length);
        socketAsyncEventArgs1.Completed += ReceivePacket1;

        packetTypeToAction = new Dictionary<PacketType, Action<byte[]>>()
        {
            {PacketType.PlayerNumber,       ReceivePlayerNumber},
            {PacketType.Attack,             ReceiveAttack},
            {PacketType.Summon,             ReceiveSummon},
            {PacketType.Skill,              ReceiveSkill},
            {PacketType.DeckDataRequest,    ReceiveDeckDataRequest},
            {PacketType.EnemyDeckData,      ReceiveEnemyDeckData },
            {PacketType.BattleStart,        ReceiveBattleStart }
        };
    }

    private void ReceivePacket0(object sender, SocketAsyncEventArgs e)
    {
        //throw new NotImplementedException();
        receiveSocket.ReceiveAsync(socketAsyncEventArgs1);
        ClassifyReceivedPacket(e.Buffer);
    }

    private void ReceivePacket1(object sender, SocketAsyncEventArgs e)
    {
        //throw new NotImplementedException();
        receiveSocket.ReceiveAsync(socketAsyncEventArgs0);
        ClassifyReceivedPacket(e.Buffer);
    }

    public void Start()
    {   
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.179"), Port);
        receiveSocket.Connect(ipEndPoint);


        Setting.GameState = GameState.Battle;
        Setting.GameMode = GameMode.Multi;

        receiveSocket.ReceiveAsync(socketAsyncEventArgs0);

        MultiBattle.Initialize(this);
        MultiBattle.instance.StartMakingEnemyDeck();
    }

    private void ClassifyReceivedPacket(byte[] buffer)
    {
        PacketType packetType = (PacketType)ByteConverter.ToInt(buffer, 0);
        Debug.Log(LogType.Trace, "ClassifyReceivedPacket {0}", packetType);

        packetTypeToAction[packetType](buffer);
    }

    private void ReceivePlayerNumber(byte[] buffer)
    {
        playerNumber = ByteConverter.ToInt(buffer, 4);
    }

    private void ReceiveDeckDataRequest(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveDeckDataRequest");

        SendDeckData();
    }

    private void ReceiveEnemyDeckData(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveEnemyDeckData");

        MultiBattleDataManager.enemyDeckData.enemyLevel = ByteConverter.ToInt(buffer, 8);
        for(int index = 0; index < Setting.DeckCount; index++)
        {
            MultiBattleDataManager.enemyDeckData.summonNumber[index] = ByteConverter.ToInt(buffer, 12 + index * 8);
            MultiBattleDataManager.enemyDeckData.summonLevel[index] = ByteConverter.ToInt(buffer, 16 + index * 8);
        }

        MultiBattleDataManager.enemyDeckData.isReceived = true;

        SendReadyCompletePacket();
    }

    private void ReceiveBattleStart(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveBattleStart");
    }

    private void ReceiveAttack(byte[] buffer)
    {
        int executionOrder = ByteConverter.ToInt(buffer, 8);

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Attack,
            order = executionOrder,
        };

        if (MultiBattleDataManager.executionDataOrder == executionOrder)
            MultiBattleDataManager.EnqueueExecutionData(executionData);

        if (MultiBattleDataManager.executionDataOrder < executionOrder)
            MultiBattleDataManager.AddOutOfSequenceData(executionData);
    }

    private void ReceiveSummon(byte[] buffer)
    {
        int executionOrder = ByteConverter.ToInt(buffer, 8);
        int playerNumber = ByteConverter.ToInt(buffer, 12);

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        int summonDeckNumber = ByteConverter.ToInt(buffer, 16);
        float xPos = ByteConverter.ToFloat(buffer, 20);
        float yPos = ByteConverter.ToFloat(buffer, 24);

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Summon,
            order = executionOrder,
            isMine = this.playerNumber == playerNumber ? true : false,

            summonDeckIndex = summonDeckNumber,
            position = new Vector3(xPos, yPos)
        };

        if (MultiBattleDataManager.executionDataOrder == executionOrder)
            MultiBattleDataManager.EnqueueExecutionData(executionData);

        if (MultiBattleDataManager.executionDataOrder < executionOrder)
            MultiBattleDataManager.AddOutOfSequenceData(executionData);
    }

    private void ReceiveSkill(byte[] buffer)
    {

    }


    public void SendClientData(string nickName, int rankScore)
    {
        Debug.Log(LogType.Trace, "SendClientData");

        int packetSize = 0;
        ByteConverter.FromString(nickName, sendBuffer, ref packetSize);
        packetSize = 10;
        ByteConverter.FromInt(rankScore, sendBuffer, ref packetSize);
        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendDeckData()
    {
        int packetSize = 0;
        ByteConverter.FromInt((int)PacketType.EnemyDeckData, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(UserData.Level, sendBuffer, ref packetSize);

        for (int num = 0; num < Setting.MyDeck.Count; num++)
        {
            ByteConverter.FromInt(Setting.MyDeck[num].Number, sendBuffer, ref packetSize);
            ByteConverter.FromInt(Setting.MyDeck[num].Level, sendBuffer, ref packetSize);
        }

        ByteConverter.FromInt(packetSize, sendBuffer, 4);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendSummonPacket(int index)
    {
        Vector2 position = PositionResearcher.GetPosition(Setting.MyDeck[index], true);

        int packetSize = 0;
        ByteConverter.FromInt((int)PacketType.Summon, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(playerNumber, sendBuffer, ref packetSize);
        ByteConverter.FromInt(index, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.x, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.y, sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, 4);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendSkillPacket()
    {
        // 미구현
    }

    public void SendReadyCompletePacket()
    {
        int packetSize = 0;
        ByteConverter.FromInt((int)PacketType.ReadyComplete, sendBuffer, ref packetSize);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }
}
