using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffInfoManager : MonoBehaviour
{
    private static BuffInfoManager instance;

    // public BuffInfo[] buffInfo { get; private set; } 이와 같이 사용해도 inspector에서 접근할 수 있는가?
    // 나중에 테스트
    [SerializeField] private BuffInfo[] _buffInfo;

    public static BuffInfo[] BuffInfo { get { return instance._buffInfo; } }

    private void Awake()
    {
        instance = this;
    }
}
