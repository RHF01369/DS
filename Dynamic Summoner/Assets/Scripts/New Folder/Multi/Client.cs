using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    Attack, Summon, Skill,
    EnemyDeckData, DeckDataRequest,
    ReadyComplete, BattleStart
};

public class Client
{
    private const int Port = 7000;

    private Socket receiveSocket;

    private byte[] receiveBuffer;
    private byte[] sendBuffer;

    private int playerNumber;

    private Dictionary<PacketType, Action> packetTypeToAction;
    private SocketAsyncEventArgs socketAsyncEventArgs;

    public Client()
    {
        receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        receiveBuffer = new byte[1024];
        sendBuffer = new byte[1024];

        socketAsyncEventArgs = new SocketAsyncEventArgs();
        socketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
        socketAsyncEventArgs.Completed += ReceivePacket;

        packetTypeToAction = new Dictionary<PacketType, Action>()
        {
            {PacketType.Attack,             ReceiveAttack},
            {PacketType.Summon,             ReceiveSummon},
            {PacketType.Skill,              ReceiveSkill},
            {PacketType.DeckDataRequest,    ReceiveDeckDataRequest},
            {PacketType.EnemyDeckData,      ReceiveEnemyDeckData },
            {PacketType.BattleStart,        ReceiveBattleStart }
        };
    }

    private void ReceivePacket(object sender, SocketAsyncEventArgs e)
    {
        //throw new NotImplementedException();
        ClassifyReceivedPacket();
        receiveSocket.ReceiveAsync(e);
    }

    public void Start()
    {   


        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.179"), Port);
        receiveSocket.Connect(ipEndPoint);


        Setting.GameState = GameState.Battle;
        Setting.GameMode = GameMode.Multi;

        receiveSocket.ReceiveAsync(socketAsyncEventArgs);
    }

    private void ClassifyReceivedPacket()
    {
        PacketType packetType = (PacketType)ByteConverter.ToInt(receiveBuffer, 0);
        Debug.Log(LogType.Trace, "ClassifyReceivedPacket {0}", packetType);

        packetTypeToAction[packetType]();
    }

    private void ReceiveDeckDataRequest()
    {
        Debug.Log(LogType.Trace, "ReceiveDeckDataRequest");

        SendDeckData();
    }

    private void ReceiveEnemyDeckData()
    {
        Debug.Log(LogType.Trace, "ReceiveEnemyDeckData");

        int enemyLevel = ByteConverter.ToInt(receiveBuffer, 8);

        Preparation.InitMultiEnemySummoner(enemyLevel, null);

        int summonNumber, summonLevel;
        for (int index = 0; index < 4; index++)
        {
            summonNumber = ByteConverter.ToInt(receiveBuffer, 12 + 8 * index);
            summonLevel = ByteConverter.ToInt(receiveBuffer, 16 + 8 * index);
            DeckSetting.SetMultiEnemyDeck(summonNumber, summonLevel);
        }

        SendReadyCompletePacket();
    }

    private void ReceiveBattleStart()
    {
        Debug.Log(LogType.Trace, "ReceiveBattleStart");
    }

    private void ReceiveAttack()
    {
        int executionOrder = ByteConverter.ToInt(receiveBuffer, 8);

        if (executionOrder < PacketManager.executionDataOrder)
            return;

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Attack,
            order = executionOrder,
        };

        if (PacketManager.executionDataOrder == executionOrder)
            PacketManager.EnqueueExecutionData(executionData);

        if (PacketManager.executionDataOrder < executionOrder)
            PacketManager.AddOutOfSequenceData(executionData);
    }

    private void ReceiveSummon()
    {
        int executionOrder = ByteConverter.ToInt(receiveBuffer, 8);
        int playerNumber = ByteConverter.ToInt(receiveBuffer, 12);

        if (executionOrder < PacketManager.executionDataOrder || playerNumber != this.playerNumber)
            return;

        int summonDeckNumber = ByteConverter.ToInt(receiveBuffer, 16);
        float xPos = ByteConverter.ToFloat(receiveBuffer, 20);
        float yPos = ByteConverter.ToFloat(receiveBuffer, 24);

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Summon,
            order = executionOrder,

            summonDeckNumber = summonDeckNumber,
            position = new Vector3(xPos, yPos)
        };

        if (PacketManager.executionDataOrder == executionOrder)
            PacketManager.EnqueueExecutionData(executionData);

        if (PacketManager.executionDataOrder < executionOrder)
            PacketManager.AddOutOfSequenceData(executionData);
    }

    private void ReceiveSkill()
    {

    }


    public void SendClientData(string nickName, int rankScore)
    {
        Debug.Log(LogType.Trace, "SendClientData");

        int packetSize = 0;
        ByteConverter.FromString(nickName, receiveBuffer, ref packetSize);
        packetSize = 10;
        ByteConverter.FromInt(rankScore, receiveBuffer, ref packetSize);
        receiveSocket.Send(receiveBuffer, packetSize, SocketFlags.None);
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
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
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
