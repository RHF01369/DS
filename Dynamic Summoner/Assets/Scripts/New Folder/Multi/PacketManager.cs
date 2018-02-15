using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExecutionType { Attack, Summon, Skill }

public struct ExecutionData
{
    public ExecutionType type;

    public int order;
    public bool isMine;
    public int summonDeckIndex;
    public Vector3 position;
    public int skillNumber;
}

public struct EnemyDeckData
{
    public bool isReceived;
    public int enemyLevel;
    public int[] summonNumber;
    public int[] summonLevel;
}

public static class MultiBattleDataManager
{
    public static int executionDataOrder { get; private set; }

    private static Client client;
    private static Queue<ExecutionData> executionDatas;
    private static List<ExecutionData> outOfSequenceDatas;

    public static byte[] synchronizationData { get; set; }
    public static bool canSynchronize { get; set; }


    public static EnemyDeckData enemyDeckData = new EnemyDeckData()
    {
        isReceived = false,
        summonNumber = new int[4],
        summonLevel = new int[4],
    };

    static MultiBattleDataManager()
    {
        executionDatas = new Queue<ExecutionData>();
        outOfSequenceDatas = new List<ExecutionData>();

        synchronizationData = new byte[Setting.MaxSize];
        canSynchronize = false;
    }

    public static void Initialize(Client value)
    {
        client = value;
        executionDatas.Clear();
        outOfSequenceDatas.Clear();

        canSynchronize = false;
    }

    public static int GetExecutionQueueCount()
    {
        return executionDatas.Count;
    }

    public static void EnqueueExecutionData(ExecutionData executionData)
    {
        //Debug.Log(LogType.Test, "<Color=Red> EE Order : " + executionData.order + ", Type : " + executionData.type + "</Color>");
        executionDatas.Enqueue(executionData);
        executionDataOrder++;
        FindMatchingExecutionData();
    }

    public static ExecutionData DequeueExecutionData()
    {
        ExecutionData executionData = executionDatas.Dequeue();

        Debug.Log(LogType.Test, "<Color=Red> DD Order : " + executionData.order + ", Type : " + executionData.type + "</Color>");

        if (executionData.type == ExecutionType.Summon)
            Debug.Log(LogType.Test, "<Color=Green> isMine : " + executionData.isMine + "</Color>");

        if(executionData.order > 0 && executionData.order % 10 == 0)
        {
            MultiBattle.SetSynchronized(false);
            client.SendSynchronizationPacket();
        }

        return executionData;
    }

    public static void AddOutOfSequenceData(ExecutionData executionData)
    {
        Debug.Log(LogType.Trace, "AddOutOfSequenceData");

        outOfSequenceDatas.Add(executionData);
    }

    private static void FindMatchingExecutionData()
    {
        foreach (ExecutionData executionData in outOfSequenceDatas)
            if (executionData.order == executionDataOrder)
            {
                EnqueueExecutionData(executionData);
                RemoveOutOfSequenceData(executionData);
            }
        return;
    }
    
    private static void RemoveOutOfSequenceData(ExecutionData executionData)
    {
        Debug.Log(LogType.Trace, "RemoveOutOfSequenceData");

        outOfSequenceDatas.Remove(executionData);
    }
}