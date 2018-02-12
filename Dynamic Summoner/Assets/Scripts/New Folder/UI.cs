using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {

    private static UI Instance;

    [SerializeField] Text myHealState;
    [SerializeField] Text enemyHealState;

    public static Text MyHealthState { get { return Instance.myHealState; } }
    public static Text EnemyHealthState { get { return Instance.enemyHealState; } }

    public GameObject main;
    public GameObject battle;
    public GameObject single;

    public static void OnMain()
    {
        Instance.single.SetActive(false);
        Instance.battle.SetActive(false);
        Instance.main.SetActive(true);
    }

    public static void OnBattle()
    {
        Instance.main.SetActive(false);
        Instance.battle.SetActive(true);
        Instance.single.SetActive(true);
    }

    private void Awake()
    {
        Instance = this;
        Camera.main.transform.position = new Vector3(-50f, 0, -10f);
        OnMain();
    }
}