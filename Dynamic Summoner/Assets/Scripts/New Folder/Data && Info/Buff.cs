using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Buff
{
    public string   Name                { get; set; }
    public string   Description         { get; set; }
    public int      Armor               { get; set; }
    public float    Distance            { get; set; }
    public float    GroundDmgMultiple   { get; set; }
    public float    AirDmgMultiple      { get; set; }
    public float    SpeedMultiple       { get; set; }
    public float    RunTime             { get; set; }
    public float    NowTime             { get; set; }
    public bool     HasLimitTime        { get; set; }
    public bool     IsTimeOver          { get; set; }

    public Buff(string name, string description, int armor,
                float distance, float gDmg, float aDmg, 
                float speed, float runTime, bool hasLimitTime,
                float nowTime = 0, bool isTimeOver = false )
    {
        Name                = name;
        Description         = description;
        Armor               = armor;
        Distance            = distance;
        GroundDmgMultiple   = gDmg;
        AirDmgMultiple      = aDmg;
        SpeedMultiple       = speed;
        RunTime             = runTime;
        NowTime             = nowTime;
        HasLimitTime        = hasLimitTime;
        IsTimeOver          = isTimeOver;
    }

    public bool Activate()
    {
        if (IsTimeOver)
            return false;

        if (HasLimitTime)
            NowTime += Setting.AttackCool;

        if (RunTime < NowTime)
            IsTimeOver = true;

        return true;
    }
}