using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovingDirect { Left, Right };

public class TotalSummonArea : MonoBehaviour
{
    [SerializeField] private GameObject FixedArea;
    [SerializeField] private MovingDirect movingDirect;

    private Canvas canvas;
    private Vector2 screenSize;
    private Vector2 canvasSize;

    private Rect rectOfTotalArea;
    private Vector2 positionOfTotalArea;

    private Rect rectOfSelectedArea;
    private Vector2 positionOfSelectedArea;

    // Use this for initialization
    void Awake()
    {
        // 소환물 위치를 지정하기위한 설정
        canvas = FindObjectOfType<Canvas>();
        Rect canvasRect = canvas.GetComponent<RectTransform>().rect;
        screenSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);
        canvasSize = new Vector2(canvasRect.width, canvasRect.height);
    }

    /// <summary>
    /// 현재 Selected Area에서 위치를 찾을 수 없다면 Selected Area를 이동시킨 후 위치 찾기
    /// </summary>
    public void MoveAreaPosition()
    {
        UnityEngine.Debug.Log("MoveScopePosition");
        rectOfSelectedArea = FixedArea.GetComponent<RectTransform>().rect;
        positionOfSelectedArea = FixedArea.transform.position;

        rectOfTotalArea = GetComponent<RectTransform>().rect;
        positionOfTotalArea = transform.position;

        if (MovingDirect.Left == movingDirect)
        {
            MoveToLeft();
        }
        else if (MovingDirect.Right == movingDirect)
        {
            MoveToRight();
        }
    }

    public void PressDown()
    {
        Vector2 mousePosition = Input.mousePosition;

        Vector2 tempPosition = new Vector2(WorldPosToCanvasChildPos(mousePosition).x, 0);

        if (IsGetOff(tempPosition))
            return;

        FixedArea.transform.localPosition = tempPosition;

        UnityEngine.Debug.Log("<Color=blue> ScreenToWorldPoint(localPosition) : " + Camera.main.ScreenToWorldPoint(WorldPosToCanvasChildPos(mousePosition)) + "</Color>");
        UnityEngine.Debug.Log("<Color=yellow> ScreenToWorldPoint(mousePosition) : " + Camera.main.ScreenToWorldPoint(mousePosition) + "</Color>");
    }

    public void PressDrag()
    {
        Vector2 mousePosition = Input.mousePosition;

        Vector2 tempPosition = new Vector2(WorldPosToCanvasChildPos(mousePosition).x, 0);

        if (IsGetOff(tempPosition))
            return;

        FixedArea.transform.localPosition = tempPosition;
    }

    private void MoveToRight()
    {
        float rightXPosOfSelectedArea = GetRightXPosOfSelectedArea();
        float rightXPosOfTotalArea = GetRightEdgeXPosOfTotalArea();

        if (rightXPosOfTotalArea < rightXPosOfSelectedArea)
        {
            FixedArea.transform.position = FixedArea.transform.position - new Vector3(rectOfSelectedArea.width, 0, 0);
            movingDirect = MovingDirect.Left;
        }
        else
            FixedArea.transform.position = FixedArea.transform.position + new Vector3(rectOfSelectedArea.width, 0, 0);
    }

    private void MoveToLeft()
    {
        float leftXPosOfSelectedArea = GetLeftXPosOfSelectedArea();
        float leftXPosOfTotalArea = GetLeftEdgeXPosOfTotalArea();

        if (leftXPosOfSelectedArea < leftXPosOfTotalArea)
        {
            FixedArea.transform.position = FixedArea.transform.position + new Vector3(rectOfSelectedArea.width, 0, 0);
            movingDirect = MovingDirect.Right;
        }
        else
            FixedArea.transform.position = FixedArea.transform.position - new Vector3(rectOfSelectedArea.width, 0, 0);
    }

    private float GetRightXPosOfSelectedArea()
    {
        return positionOfSelectedArea.x + rectOfSelectedArea.width / 2;
    }

    private float GetLeftXPosOfSelectedArea()
    {
        return positionOfSelectedArea.x - rectOfSelectedArea.width / 2;
    }

    private float GetRightEdgeXPosOfTotalArea()
    {
        return positionOfTotalArea.x + rectOfTotalArea.width / 2 - rectOfSelectedArea.width;
    }

    private float GetLeftEdgeXPosOfTotalArea()
    {
        return positionOfTotalArea.x - rectOfTotalArea.width / 2 + rectOfSelectedArea.width;
    }

    private bool IsGetOff(Vector2 tempPosition)
    {
        float HalfWidthOfSelectArea = FixedArea.GetComponent<RectTransform>().rect.width / 2;
        float HalfWidthOfTotalArea = GetComponent<RectTransform>().rect.width / 2;

        if (HalfWidthOfTotalArea <= (tempPosition.x + HalfWidthOfSelectArea))
        {
            UnityEngine.Debug.Log("SummonArea Class - IsGetOff Method - return true  1");
            return true;
        }
        else if ((tempPosition.x - HalfWidthOfSelectArea) <= (-HalfWidthOfTotalArea))
        {
            UnityEngine.Debug.Log("SummonArea Class - IsGetOff Method - return true  2");
            return true;
        }
        else
            return false;
    }

    private Vector2 WorldPosToCanvasChildPos(Vector2 mousePosition)
    {
        mousePosition -= screenSize / 2;

        float xRate = canvasSize.x / screenSize.x;
        float yRate = canvasSize.y / screenSize.y;

        // SummonArea 원점을 ScopeOfRecall 원점으로 이동시키기 위한 변수
        Vector2 gap = GetComponent<RectTransform>().localPosition;

        return new Vector2(mousePosition.x * xRate - gap.x,
                           mousePosition.y * yRate - gap.y);
    }
}
