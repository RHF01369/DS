//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class OpponentSummoner : MonoBehaviour {

//    // TEST용으로 SerializeField로 설정하고, 나중에는 DB에서 객체와 객체정보를 가져와서 저장할꺼다.
//    [SerializeField] private GameObject[] _summon;


//    private SortedDictionary<float, GameObject> _oSortedSummons;
//    private Summon _summonInfo;
//    private GameHandler _gameHandler;

//    public SortedDictionary<float, GameObject> OSortedSummons { get { return _oSortedSummons; } }

//    private void Awake()
//    {
//        _oSortedSummons = new SortedDictionary<float, GameObject>();
//    }

//    #region Public Method

//    public void Init(GameHandler gameHandler)
//    {
//        InitGameHandler(gameHandler);
//        SortSummons();
//    }

//    public void RemoveSummonedSummon(GameObject summon)
//    {
//        _summonInfo = summon.GetComponent<Summon>();

//        float xPos = summon.transform.position.x;
//        int summonOrder = _summonInfo.ObjectFullOrder;

//        _oSortedSummons[0].SetActive(false);
//        _oSortedSummons.Remove(xPos);
//    }

//    #endregion

//    #region Private Method

//    private void InitGameHandler(GameHandler gameHandler)
//    {
//        _gameHandler = gameHandler;

//        for (int i = 0; i < _summon.Length; i++)
//        {
//            _summon[i].GetComponent<Summon>().InitGameHandler(_gameHandler);

//        }
//    }

//    private void InitIsMySummon()
//    {
//        for (int i = 0; i < _summon.Length; i++)
//            _summon[i].GetComponent<Summon>().bIsMySummon = false;
//    }       
//    private void SortSummons()
//    {
//        Vector3 position;

//        for(int i = 0; i < _summon.Length; i++)
//        {
//            position = _summon[i].transform.position;

//            _oSortedSummons.Add(position.x, _summon[i]);
//        }
//    }

//    #endregion 
//}