using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoundInfoOfSingleMode
{
    public int round;
    public int summonerLevel;
    public Sprite summonerSprite;

    public SummonInfo[] summonInfo;
    public PositionInfo[] positionInfo;

    [Serializable]
    public struct SummonInfo
    {
        public int number;
        public int level;
    }

    [Serializable]
    public struct PositionInfo
    {
        public int number;
        public Vector2 position;
    }
}