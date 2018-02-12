using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExecutionType { Attack, Summon, Skill }

public struct ExecutionData
{
    public ExecutionType type;

    public int order;
    public int playerNumber;
    public int summonDeckNumber;
    public Vector3 position;
    public int SkillNumber;
}

public static class PacketManager
{
    public static int executionDataOrder { get; private set; }

    private static Queue<ExecutionData> executionDatas;
    private static List<ExecutionData> outOfSequenceDatas;

    public static void EnqueueExecutionData(ExecutionData executionData)
    {
        executionDatas.Enqueue(executionData);
        executionDataOrder++;
        FindMatchingExecutionData();
    }

    public static ExecutionData DequeueExecutionData()
    {
        return executionDatas.Dequeue();
    }

    public static void AddOutOfSequenceData(ExecutionData executionData)
    {
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
        outOfSequenceDatas.Remove(executionData);
    }
}