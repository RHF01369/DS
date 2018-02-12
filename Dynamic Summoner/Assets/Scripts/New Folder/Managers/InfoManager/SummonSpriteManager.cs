using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSpriteManager : MonoBehaviour
{
    private static SummonSpriteManager instance; 

    [SerializeField] private Sprite[] _sprite;

    public static Sprite[] sprite { get { return instance._sprite; } }

    private void Awake()
    {
        instance = this;
    }
}