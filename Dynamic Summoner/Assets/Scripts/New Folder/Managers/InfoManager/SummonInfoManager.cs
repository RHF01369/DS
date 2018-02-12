using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonInfoManager : MonoBehaviour
{
    private static SummonInfoManager instance;

    [SerializeField] private SummonInfo[] _summonInfo;

    public static SummonInfo[] summonInfo { get { return instance._summonInfo; } }

    private void Awake()
    {
        instance = this;
    }


    // public SummonInfo GetSummonData(int number) { return _summonInfo[number]; }
}