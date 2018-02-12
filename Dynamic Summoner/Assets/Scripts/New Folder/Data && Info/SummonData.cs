using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SummonData
{
    public int cardNumber;
    public int experience;
    public int level;

    public SummonData(int cardNumber, int experience, int level)
    {
        this.cardNumber = cardNumber;
        this.experience = experience;
        this.level = level;
    }
}