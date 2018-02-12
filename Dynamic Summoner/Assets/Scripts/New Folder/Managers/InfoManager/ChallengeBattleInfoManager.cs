using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeBattleInfoManager : MonoBehaviour
{
    private static ChallengeBattleInfoManager instance;

    [SerializeField] private RoundInfoOfChallengeMode[] _roundInfo;

    public static RoundInfoOfChallengeMode[] roundInfo { get { return instance._roundInfo; } }

    private void Awake()
    {
        instance = this;
    }
}