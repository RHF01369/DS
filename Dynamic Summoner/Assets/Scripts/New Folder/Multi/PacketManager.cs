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

    private static Queue<ExecutionData> executionDatas;
    private static List<ExecutionData> outOfSequenceDatas;
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
    }

    public static int GetExecutionQueueCount()
    {
        return executionDatas.Count;
    }

    public static void EnqueueExecutionData(ExecutionData executionData)
    {
        Debug.Log(LogType.Test, "<Color=Red> EE Order : " + executionData.order +  "</Color=Red>");
        executionDatas.Enqueue(executionData);
        executionDataOrder++;
        FindMatchingExecutionData();
    }

    public static ExecutionData DequeueExecutionData()
    {
        ExecutionData executionData = executionDatas.Dequeue();

        Debug.Log(LogType.Test, "<Color=Blue> DD Order : " + executionData.order + "</Color=Blue>");

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