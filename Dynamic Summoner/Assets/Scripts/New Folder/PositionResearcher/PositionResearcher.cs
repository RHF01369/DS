using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionResearcher : MonoBehaviour
{
    private static PositionResearcher Instance;

    [SerializeField] GameObject myTotalArea;
    [SerializeField] GameObject myFixedArea;
    [SerializeField] GameObject enemyTotalArea;
    [SerializeField] GameObject enemyFixedArea;

    private int searchingCount;

    private Vector3 summonPosition;
    private Vector3 colliderCenter;
    private Vector3 colliderHalfExtents;
    private TotalSummonArea totalArea;
    private FixedSummonArea fixedArea;


    private void Awake()
    {
        Instance = this;
        searchingCount = 100;
    }

    public static Vector2 GetPosition(UnitData unitData, bool isMyTeam)
    {
        Instance.initialize(unitData, isMyTeam);

        while (true)
        {
            if (Instance.CanSetPosition())
                return Instance.summonPosition;

            Instance.totalArea.MoveAreaPosition();
        }
    }

    private void initialize(UnitData unitData, bool isMyTeam)
    {
        colliderCenter = unitData.ColliderCenter;
        colliderHalfExtents = unitData.ColliderSize / 2;

        if (isMyTeam)
        {
            totalArea = myTotalArea.GetComponent<TotalSummonArea>();
            fixedArea = myFixedArea.GetComponent<FixedSummonArea>();
        }
        else
        {
            totalArea = enemyTotalArea.GetComponent<TotalSummonArea>();
            fixedArea = enemyFixedArea.GetComponent<FixedSummonArea>();
        }
    }

    private bool CanSetPosition()
    {
        for (int num = 0; num < searchingCount; num++)
        {
            summonPosition = fixedArea.GetTestPosition(num);

            if (!IsCollidingAtNext())
                return true;
        }

        return false;
    }

    private bool IsCollidingAtNext()
    {
        if (Physics.BoxCast(summonPosition + colliderCenter, colliderHalfExtents, Vector3.forward))
        {
            UnityEngine.Debug.Log("<Color=red> BoxCast OK </Color>");

            return true;
        }
        return false;
    }
}