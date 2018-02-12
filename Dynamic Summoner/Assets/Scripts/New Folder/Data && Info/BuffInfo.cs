using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffInfo
{
    public string name;
    public string description;

    public int armor;
    public float distance;
    public float groundDmgMultiple;
    public float airDmgMultiple;
    public float speedMultiple;
    public float runTime;

    public bool hasLimitTime;
}