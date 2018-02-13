﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    ClientData, PlayerNumber,
    Attack, Summon, Skill,
    EnemyDeckData, DeckDataRequest,
    ReadyComplete, BattleStart
};

public class Client
{
    private const int Port = 7000;
    private const int MaxSize = 1024;
    private const int PacketSizeStartIndex = 5;
    private const int ExecutionOrderStartIndex = 9;
    private const byte PacketStartNumber = 255;

    private Socket receiveSocket;

    private byte[][] receiveBuffer;
    private byte[] chainingReceiveBuffer;
    private byte[] sendBuffer;

    private int playerNumber;

    private Dictionary<PacketType, Action<byte[]>> packetTypeToAction;
    private SocketAsyncEventArgs socketAsyncEventArgs0;
    private SocketAsyncEventArgs socketAsyncEventArgs1;

    public Client()
    {
        receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        receiveBuffer = new byte[2][];
        receiveBuffer[0] = new byte[MaxSize];
        receiveBuffer[1] = new byte[MaxSize];
        chainingReceiveBuffer = new byte[MaxSize];
        sendBuffer = new byte[MaxSize];

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
        Debug.Log(LogType.Trace, "ReceivePacket-0 : {0} :", ByteConverter.ToInt(e.Buffer, 1));

        //throw new NotImplementedException();
        receiveSocket.ReceiveAsync(socketAsyncEventArgs1);
        ClassifyReceivedPacket(e.Buffer);
    }

    private void ReceivePacket1(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log(LogType.Trace, "ReceivePacket-1 : {0} :", ByteConverter.ToInt(e.Buffer, 1));

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
        PacketType packetType = (PacketType)ByteConverter.ToInt(buffer, 1);
        Debug.Log(LogType.Trace, "ClassifyReceivedPacket {0}", packetType);

        packetTypeToAction[packetType](buffer);
    }

    private void ReceivePlayerNumber(byte[] buffer)
    {
        playerNumber = ByteConverter.ToInt(buffer, 9);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void ReceiveDeckDataRequest(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveDeckDataRequest");

        SendDeckData();

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void ReceiveEnemyDeckData(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveEnemyDeckData");

        int startIndex = 9;
        MultiBattleDataManager.enemyDeckData.enemyLevel = ByteConverter.ToInt(buffer, ref startIndex);
        for(int index = 0; index < Setting.deckCount; index++)
        {
            MultiBattleDataManager.enemyDeckData.summonNumber[index] = ByteConverter.ToInt(buffer, ref startIndex);
            MultiBattleDataManager.enemyDeckData.summonLevel[index] = ByteConverter.ToInt(buffer, ref startIndex);
        }

        MultiBattleDataManager.enemyDeckData.isReceived = true;

        SendReadyCompletePacket();

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void ReceiveBattleStart(byte[] buffer)
    {
        Debug.Log(LogType.Trace, "ReceiveBattleStart");

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void ReceiveAttack(byte[] buffer)
    {
        int executionOrder = ByteConverter.ToInt(buffer, ExecutionOrderStartIndex);

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Attack,
            order = executionOrder,
        };

        AddExecutionDataToManager(executionData);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void ReceiveSummon(byte[] buffer)
    {
        int startIndex = ExecutionOrderStartIndex;
        int executionOrder = ByteConverter.ToInt(buffer, ref startIndex);
        int playerNumber = ByteConverter.ToInt(buffer, ref startIndex);
        bool isMine = this.playerNumber == playerNumber ? true : false;

        if (executionOrder < MultiBattleDataManager.executionDataOrder)
            return;

        int summonDeckNumber = ByteConverter.ToInt(buffer, ref startIndex);
        float xPos = GetSummonXPosition(ByteConverter.ToFloat(buffer, ref startIndex), isMine);
        float yPos = ByteConverter.ToFloat(buffer, ref startIndex);

        ExecutionData executionData = new ExecutionData()
        {
            type = ExecutionType.Summon,
            order = executionOrder,
            isMine = isMine,

            summonDeckIndex = summonDeckNumber,
            position = new Vector3(xPos, yPos)
        };

        AddExecutionDataToManager(executionData);

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
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

        CheckChainingPacket(buffer, ByteConverter.ToInt(buffer, PacketSizeStartIndex));
    }

    private void AddExecutionDataToManager(ExecutionData executionData)
    {
        if (MultiBattleDataManager.executionDataOrder == executionData.order)
            MultiBattleDataManager.EnqueueExecutionData(executionData);

        if (MultiBattleDataManager.executionDataOrder < executionData.order)
            MultiBattleDataManager.AddOutOfSequenceData(executionData);
    }

    private void CheckChainingPacket(byte[] buffer, int startIndex)
    {
        if (buffer[startIndex] != PacketStartNumber)
        {
            Array.Clear(buffer, 0, buffer.Length);
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
        sendBuffer[0] = PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.ClientData, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromString(nickName, sendBuffer, ref packetSize);
        ByteConverter.FromInt(rankScore, sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, PacketSizeStartIndex);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendDeckData()
    {
        int packetSize = 1;
        sendBuffer[0] = PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.EnemyDeckData, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(UserData.Level, sendBuffer, ref packetSize);

        for (int num = 0; num < Setting.MyDeck.Count; num++)
        {
            ByteConverter.FromInt(Setting.MyDeck[num].Number, sendBuffer, ref packetSize);
            ByteConverter.FromInt(Setting.MyDeck[num].Level, sendBuffer, ref packetSize);
        }

        ByteConverter.FromInt(packetSize, sendBuffer, PacketSizeStartIndex);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendSummonPacket(int index)
    {
        Vector2 position = PositionResearcher.GetPosition(Setting.MyDeck[index], true);

        int packetSize = 1;
        sendBuffer[0] = PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.Summon, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(0, sendBuffer, ref packetSize);
        ByteConverter.FromInt(playerNumber, sendBuffer, ref packetSize);
        ByteConverter.FromInt(index, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.x, sendBuffer, ref packetSize);
        ByteConverter.FromFloat(position.y, sendBuffer, ref packetSize);

        ByteConverter.FromInt(packetSize, sendBuffer, PacketSizeStartIndex);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }

    public void SendSkillPacket()
    {
        // 미구현
    }

    public void SendReadyCompletePacket()
    {
        int packetSize = 1;
        sendBuffer[0] = PacketStartNumber;
        ByteConverter.FromInt((int)PacketType.ReadyComplete, sendBuffer, ref packetSize);
        ByteConverter.FromInt(packetSize + 4, sendBuffer, ref packetSize);

        receiveSocket.Send(sendBuffer, packetSize, SocketFlags.None);
    }
}