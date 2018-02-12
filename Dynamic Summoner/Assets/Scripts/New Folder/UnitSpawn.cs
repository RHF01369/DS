using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void Initialize(UnitData data, Vector2 position);
    bool IsMyTeam { get; }
    bool IsUsed { get; }
}

public class UnitSpawn<T> where T : IPoolable
{
    private int initNumber;

    public  List<T> Units { get; private set; }

    public UnitSpawn()
    {
        initNumber = 30;
        Units = new List<T>();
    }

    public T Pull(bool isMyTeam)
    {
        for(int index=0; index < Units.Count; index++)
        {
            if (!Units[index].IsUsed && Units[index].IsMyTeam == isMyTeam)
                return Units[index];
        }

        return default(T);
    }

    public T AddUnit(T unit)
    {
        Units.Add(unit);
        return unit;
    }
}
