using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UnitData
{
    public int         Number { get; private set; }
    public int         Level { get; private set; }
    public string      Pattern { get; private set; }
    public FieldType   FieldType { get; private set; }
    public AttackType  AttackType { get; private set; }
    public Vector3     ColliderCenter { get; private set; }
    public Vector3     ColliderSize { get; private set; }
    public float       AttackSpeed { get; private set; }
    public float       AirDamage { get; private set; }
    public float       GroundDamage { get; private set; }
    public float       Health { get; set; }
    public int         Armor { get; private set; }
    public Sprite      Sprite { get; private set; }
    public Buff        Buff { get; set; }
    public bool        IsBuff { get; set; }

    public UnitData(int number, int level, string pattern,
                    FieldType fieldType, AttackType attackType,
                    Vector3 colliderCenter, Vector3 colliderSize,
                    float attackSpeed, float airDamage, float groundDamage,
                    float health, int armor,
                    bool isBuff, Buff buff, Sprite sprite)
    {
        Number = number;
        Level = level;
        Pattern = pattern;
        FieldType = fieldType;
        AttackType = attackType;
        ColliderCenter = colliderCenter;
        ColliderSize = colliderSize;
        AttackSpeed = attackSpeed;
        AirDamage = airDamage;
        GroundDamage = groundDamage;
        Armor = armor;
        Health = health;
        IsBuff = isBuff;
        Buff = buff;
        Sprite = sprite;
    }
}