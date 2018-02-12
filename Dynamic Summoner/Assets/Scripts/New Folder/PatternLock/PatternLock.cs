using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

public class PatternLock : MonoBehaviour {

    private static PatternLock instance;

    [SerializeField] private PatternLockButton[] buttons;
    private IPatternable battle;
    private List<PatternLockButton> linkedButtons = new List<PatternLockButton>();

    private bool isDragging;
    private string pattern;

    private Canvas canvas;
    private Rect canvasRect;
    private Vector2 screenSize;
    private Vector2 canvasSize;

    private void Awake()
    {
        instance = this;

        foreach (var button in buttons)
        {
            button.OnPointerDown += PressDown;
            button.OnPointerEnter += PressEnter;
            button.OnPointerUp += PressUp;
            button.OnDrag += Press;
        }
    }

    private void Start()
    {
        //battleHandler = GameHandler.Instance;
        canvas = FindObjectOfType<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>().rect;

        screenSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);
        canvasSize = new Vector2(canvasRect.width, canvasRect.height);

        PlacementSetting();
        
        foreach(var button in buttons)
            button.linePositionSetting();
    }

    private void Press()
    {
        if (isDragging == false)
            return;

        linkedButtons[linkedButtons.Count - 1].Line(
            linkedButtons[linkedButtons.Count - 1].transform.localPosition,
            WorldPosToPatternLockPos());
    }

    private void PressUp()
    {
        Print();

        // 패턴을 인식하면 mySummoner 로 넘겨준다.
        battle.InputPattern(pattern);

        foreach (var button in linkedButtons)
            button.ClearLine();
        linkedButtons.Clear();
        isDragging = false;
    }

    private void PressDown(PatternLockButton button)
    {
        linkedButtons.Clear();
        isDragging = true;
        linkedButtons.Add(button);
    }

    private void PressEnter(PatternLockButton button)
    {
        if (linkedButtons.Contains(button) || isDragging == false)
            return;

        linkedButtons[linkedButtons.Count - 1].Link(button.transform.localPosition);
        linkedButtons.Add(button);
    }

    private Vector2 WorldPosToPatternLockPos()
    {
        Vector2 mousePosition = Input.mousePosition;
        mousePosition -= screenSize / 2;

        float xRate = canvasSize.x / screenSize.x;
        float yRate = canvasSize.y / screenSize.y;

        Vector2 gap = GetComponent<RectTransform>().localPosition;

        return new Vector2(mousePosition.x * xRate - gap.x,
                           mousePosition.y * yRate - gap.y);
    }

    private void Print()
    {
        pattern = string.Join("",
            (from button in linkedButtons
             select button.buttonName).ToArray());

        UnityEngine.Debug.Log(pattern);
    }

    private void PlacementSetting()
    {
        Rect panelRect = GetComponent<RectTransform>().rect;

        // 정육각형(패턴모양)의 한 변의 길이
        float length;

        if (panelRect.height < panelRect.width)
            length = panelRect.height * (3f / 8f);
        else
            length = panelRect.width * (3f / 8f);

        // length를 한 변으로 하는 정삼각형의 높이  ( 1라디안 = 57.2958 )
        float height = length * (Mathf.Sin(1 * (60f / 57.3f)));

        buttons[0].transform.localPosition = new Vector2(0, length);
        buttons[3].transform.localPosition = new Vector2(0, -length);

        buttons[1].transform.localPosition = new Vector2(-height, length / 2);
        buttons[2].transform.localPosition = new Vector2(-height, -(length / 2));
        buttons[4].transform.localPosition = new Vector2(height, -(length / 2));
        buttons[5].transform.localPosition = new Vector2(height, length / 2);

        buttons[6].transform.localPosition = new Vector2(0, 0);
    }

    public static IPatternable Battle { get { return instance.battle; } set { instance.battle = value; } }
}