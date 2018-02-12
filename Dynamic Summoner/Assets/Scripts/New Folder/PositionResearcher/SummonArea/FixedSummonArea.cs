using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class FixedSummonArea : MonoBehaviour
{
    /// <summary>
    /// Random 으로 좌표를 선택할 때, Random 좌표가 포함될 수 있는 
    /// 범위를 지정하는 퍼센트이다.
    /// </summary>
    [SerializeField] private int UpperAreaPercent;
    [SerializeField] private int LowerAreaPercent;
    [SerializeField] private int RightAreaPercent;
    [SerializeField] private int LeftAreaPercent;

    private float[,] ranges;

    private void Awake()
    {
        ranges = new float[,]
        {
            {LowerAreaPercent            , LowerAreaPercent * (3f / 4f)   },
            {LowerAreaPercent * (3f / 4f), LowerAreaPercent / 2           },
            {UpperAreaPercent / 2        , UpperAreaPercent * (3f / 4f)   },
            {UpperAreaPercent * (3f / 4f), UpperAreaPercent               }
        };
    }

    public Vector3 GetTestPosition(int num)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(transform.position);

        float halfWidth = GetEdgeWorldPos().x - worldPosition.x;
        float halfHeight = GetEdgeWorldPos().y - worldPosition.y;

        float percentY = GetPercentY(num);
        float percentX = GetPercentX(num);

        float posX = worldPosition.x + (halfWidth * percentX / 100f);
        float posY = worldPosition.y + (halfHeight * percentY / 100f);

        return new Vector3(posX, posY, -10f);
    }

    private Vector2 GetEdgeWorldPos()
    {
        Rect rect = GetComponent<RectTransform>().rect;
        Vector2 position = transform.position;

        Vector2 edgePosition = new Vector2(position.x + (rect.width / 2), position.y + (rect.height / 2));
        Vector2 edgeWorldPos = Camera.main.ScreenToWorldPoint(edgePosition);

        return edgeWorldPos;
    }

    private float GetPercentY(int num)
    {
        if (num < 50)
            return Random.Range(LowerAreaPercent / 2, UpperAreaPercent / 2);

        getPercentY = getPercentY ?? GetPercentY();
        float ret = getPercentY.Current;
        getPercentY.MoveNext();
        return ret;
    }

    IEnumerator<float> getPercentY;
    IEnumerator<float> GetPercentY()
    {
        while (true)
        {
            int random = Random.Range(0, 4);
            float min = ranges[random, 0];
            float max = ranges[random, 1];

            float toRet = Random.Range(min, max);
            yield return toRet;
        }
    }

    private float GetPercentX(int num)
    {
        int random = Random.Range(0, 4);

        if (0 == random)
            return Random.Range(LeftAreaPercent, LeftAreaPercent / 2);
        else if (1 == random)
            return Random.Range(LeftAreaPercent / 2, 0);
        else if (2 == random)
            return Random.Range(0, RightAreaPercent / 2);
        else
            return Random.Range(RightAreaPercent / 2, RightAreaPercent);
    }
}
