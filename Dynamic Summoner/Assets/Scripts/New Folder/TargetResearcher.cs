using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetResearcher
{
    private static TargetResearcher targetResearcher;
    private static TargetResearcher Instance { get { return targetResearcher ?? new TargetResearcher(); } }

    private TargetResearcher()
    {
        targetResearcher = this;
    }

    public static Unit Find(AttackType attackType, bool enemyTeam)
    {
        Unit unit = null;
        switch (attackType)
        {
            case AttackType.Front:
                unit = Instance.FindFront(enemyTeam);
                break;
            case AttackType.Back:
                unit = Instance.FindBack(enemyTeam);
                break;
            case AttackType.AirFirst:
                unit = Instance.FindAir(enemyTeam);
                break;
            case AttackType.GroundFirst:
                unit = Instance.FindGround(enemyTeam);
                break;
            case AttackType.StrongDmg:
                //unit = FindStrongDmg(enemyTeam);
                break;
            case AttackType.WeakHealth:
                unit = Instance.FindWeakHealth(enemyTeam);
                break;
            case AttackType.BuffUnit:
                unit = Instance.FindBuffUnit(enemyTeam);
                break;
        }

        if (!(unit == null))
            return unit;

        return unit;
    }

    private Unit FindFront(bool enemyTeam)
    {
        if (enemyTeam)
            return FindMyFront();
        else
            return FindEnemyFront();
    }

    private Unit FindMyFront()
    {
        float x = -10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || !SpawnManager.Units[num].IsMyTeam)
                continue;

            if (x == -10000 || x < SpawnManager.Units[num].UnitObject.transform.position.x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    private Unit FindEnemyFront()
    {
        float x = 10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].IsMyTeam)
                continue;

            if (x == 10000 || SpawnManager.Units[num].UnitObject.transform.position.x < x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }


    private Unit FindBack(bool enemyTeam)
    {
        if (enemyTeam)
            return FindMyBack();
        else
            return FindEnemyBack();
    }

    private Unit FindMyBack()
    {
        float x = 10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || !SpawnManager.Units[num].IsMyTeam)
                continue;

            if (x == 10000 || SpawnManager.Units[num].UnitObject.transform.position.x < x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    private Unit FindEnemyBack()
    {
        float x = -10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].IsMyTeam)
                continue;

            if (x == -10000 || x < SpawnManager.Units[num].UnitObject.transform.position.x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }


    private Unit FindAir(bool enemyTeam)
    {
        if (enemyTeam)
            return FindMyAir();
        else
            return FindEnemyAir();
    }

    private Unit FindMyAir()
    {
        float x = -10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || !SpawnManager.Units[num].IsMyTeam || SpawnManager.Units[num].Data.FieldType != FieldType.Air)
                continue;

            if (x == -10000 || x < SpawnManager.Units[num].UnitObject.transform.position.x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    private Unit FindEnemyAir()
    {
        float x = 10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].IsMyTeam || SpawnManager.Units[num].Data.FieldType != FieldType.Air)
                continue;

            if (x == 10000 || SpawnManager.Units[num].UnitObject.transform.position.x < x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }


    private Unit FindGround(bool enemyTeam)
    {
        if (enemyTeam)
            return FindMyGround();
        else
            return FindEnemyGround();
    }

    private Unit FindMyGround()
    {
        float x = -10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || !SpawnManager.Units[num].IsMyTeam || SpawnManager.Units[num].Data.FieldType != FieldType.Ground)
                continue;

            if (x == -10000 || x < SpawnManager.Units[num].UnitObject.transform.position.x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    private Unit FindEnemyGround()
    {
        float x = 10000;
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || !SpawnManager.Units[num].IsMyTeam || SpawnManager.Units[num].Data.FieldType != FieldType.Ground)
                continue;

            if (x == 10000 || SpawnManager.Units[num].UnitObject.transform.position.x < x)
            {
                x = SpawnManager.Units[num].UnitObject.transform.position.x;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    //private Unit FindStrongDmg(bool enemyTeam)
    //{
    //}

    private Unit FindWeakHealth(bool enemyTeam)
    {
        int index = -1;
        float health = 10000;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].IsMyTeam != enemyTeam)
                continue;

            if (SpawnManager.Units[num].Data.Health < health)
            {
                health = SpawnManager.Units[num].Data.Health;
                index = num;
            }
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }

    private Unit FindBuffUnit(bool enemyTeam)
    {
        int index = -1;

        for (int num = 0; num < SpawnManager.Units.Count; num++)
        {
            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].IsMyTeam != enemyTeam)
                continue;

            if (!SpawnManager.Units[num].IsUsed || SpawnManager.Units[num].Data.IsBuff)
                index = num;
        }

        if (index == -1)
            return null;
        return SpawnManager.Units[index];
    }
}
