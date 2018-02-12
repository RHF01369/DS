using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoundInfoOfChallengeMode
{
    public int round;
    public int summonerLevel;
    public Sprite summonerSprite;

    public SummonData[] summonData;

    [Serializable]
    public struct SummonData
    {
        public int number;
        public int level;
    }
}

