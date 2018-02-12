using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBattleInfoManager : MonoBehaviour
{
    private static SingleBattleInfoManager instance;

    [SerializeField] private RoundInfoOfSingleMode[] _roundInfo;

    public static RoundInfoOfSingleMode[] roundInfo { get { return instance._roundInfo; } }

    private void Awake()
    {
        instance = this;
    }
}