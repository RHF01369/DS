using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Authentication {

    private bool            isAuthSuccess;
    private string          googleIdToken;
    private DBController    dbController;

    public Authentication()
    {
        isAuthSuccess = false;
        dbController = DBController.Instance;
    }

    public void Start()
    {
        UnityEngine.Debug.Log("GoogleAuth START()");

        dbController.Init();
        dbController.UserVerification();

    }
}