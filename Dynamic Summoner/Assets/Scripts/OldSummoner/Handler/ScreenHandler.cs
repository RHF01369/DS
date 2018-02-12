//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ScreenHandler : MonoBehaviour
//{
//    public static ScreenHandler Instance { get; private set; }

//    [SerializeField] GameObject mainCanvas;
//    [SerializeField] GameObject battleCanvas;

//    [SerializeField] Vector3 mainCameraPosition;
//    [SerializeField] Vector3 singleCameraPosition;
//    [SerializeField] Vector3 challengeCameraPosition;
//    [SerializeField] Vector3 multiCameraPosition;

//    private Dictionary<GameState, Vector3> cameraPosition;

//    void Awake()
//    {
//        Instance = this;

//        cameraPosition = new Dictionary<GameState, Vector3>()
//        {
//            { GameState.Main,        mainCameraPosition      },
//            { GameState.Single,      singleCameraPosition    },
//            { GameState.Challenge,   challengeCameraPosition },
//            { GameState.Multi,       multiCameraPosition     }
//        };
//    }

//    public void ChangeScreen(GameState gameState)
//    {
//        Camera.main.transform.position = cameraPosition[gameState];

//        if (gameState == GameState.Main)
//            ActiveMain();
//        else
//            ActiveBattle();
//    }

//    private void ActiveMain()
//    {
//        mainCanvas.SetActive(true);
//        battleCanvas.SetActive(false);
//    }

//    private void ActiveBattle()
//    {
//        mainCanvas.SetActive(false);
//        battleCanvas.SetActive(true);
//    }
//}