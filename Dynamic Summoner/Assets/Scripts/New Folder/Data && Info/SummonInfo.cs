using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SummonInfo
{
    public int           number;
    public string        pattern;
    public FieldType     fieldType;
    public AttackType    attackType;
    public Vector3       colliderCenter;
    public Vector3       colliderSize;

    public float attackSpeed;
    public float airDamage;
    public float groundDamage;
    public float health;
    public int   armor;

    public float growthRateOfAirDam;
    public float growthRateOfGroundDam;
    public float growthRateOfHealth;
    public int   growthRateOfArmor;

    public bool isBuff;
}