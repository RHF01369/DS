using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternLockButton : MonoBehaviour {

    public event Action<PatternLockButton> OnPointerDown;
    public event Action<PatternLockButton> OnPointerEnter;
    public event Action OnPointerUp;
    public event Action OnDrag;

    [SerializeField] Image line;

    public string buttonName;

    private int _dragCount;

    /// <summary>
    /// awake나 start를 이용해서 설정을 할 경우에는 Button의 localPosition이 설정되기 이전이기 때문에 
    /// 제대로된 line 위치가 설정되지 않는다. 
    /// </summary>
    public void linePositionSetting()
    {
        line.transform.localPosition = transform.localPosition;
    }

    public void PointerDown()
    {
        if (OnPointerDown != null)
            OnPointerDown(this);
    }

    public void PointerEnter()
    {
        if (OnPointerEnter != null)
            OnPointerEnter(this);
    }

    public void PointerUp()
    {
        if (OnPointerUp != null)
            OnPointerUp();
    }

    public void Drag()
    {
        _dragCount++;

        if (OnDrag != null || 30000 == _dragCount)
        {
            OnDrag();
            _dragCount = 0;
        }
    }

    public void Link(Vector2 destination)
    {
        Vector2 origin = transform.localPosition;

        Draw(origin, destination);
    }

    public void Line(Vector2 origin, Vector2 destination)
    {
        Draw(origin, destination);
    }

    public void ClearLine()
    {
        Transform lineTF = line.transform;

        lineTF.localScale = new Vector3(0, lineTF.localScale.y, lineTF.localScale.z);
        lineTF.rotation = Quaternion.identity;
    }

    private void Draw(Vector2 origin, Vector2 destination)
    {
        float disY = destination.y - origin.y;
        float disX = destination.x - origin.x;
        float distance = Mathf.Sqrt(Mathf.Pow(disY, 2) + Mathf.Pow(disX, 2));
        line.transform.localScale = new Vector3(distance / 100, 1, 1);

        float angle = Mathf.Atan2(disY, disX) * 57.2958f;
        line.transform.rotation = Quaternion.Euler(0, 0, angle);

    }
}   