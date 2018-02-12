//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MySummoner : MonoBehaviour {

//    enum PositionSettingResult { Success, NullPosition, NullObject };

//    [SerializeField] private GameObject _summonArea;
//    [SerializeField] private GameObject _scopeOfRecall;

//    private Dictionary<int, GameObject>[] _summons;
//    private Dictionary<string, int> _collectPatternDeckNum;
//    private SortedDictionary<float, GameObject> _mSortedSummons;
//    private GameObject[] _prefabsOfSummon;
//    private GameObject _summon;
//    private Vector3 _summonPosition;
//    private Summon _summonProperty;
//    private PositionSettingResult _settingResult;

//    private Ray _ray;
//    private BoxCollider _summonCollider;

//    private SummonManager _summonManager;
//    private UserDTO _userDTO;
//    private DeckDTO _deckDTO;
//    private GameHandler _gameHandler;

//    public SortedDictionary<float, GameObject> MSortedSummons { get { return _mSortedSummons; } }

//    private void Awake()
//    {
//        _summons = new Dictionary<int, GameObject>[4];

//        for (int i = 0; i < 4; i++)
//            _summons[i] = new Dictionary<int, GameObject>();

//        _collectPatternDeckNum = new Dictionary<string, int>();

//        _mSortedSummons = new SortedDictionary<float, GameObject>(new ReverseComparer<float>(Comparer<float>.Default));
//        _prefabsOfSummon = new GameObject[4];
//        _ray.direction = new Vector3(0, 0, 1f);
//        _summonManager = SummonManager.Instance;
//        _userDTO = UserDTO.Instance;
//        _deckDTO = DeckDTO.Instance;
//    }

//    #region Public Method

//    public void Init(GameHandler gameHandler)
//    {
//        Debug.Log("MySummoner Init() START");
//        InitGameHandler(gameHandler);
//        SetSummonPrefab();
//        SetInfoOfSummonPrefab();
//        CreateSummonObjectFull();
//    }

//    public void Summon(string pattern)
//    {
//        if (_collectPatternDeckNum.ContainsKey(pattern))
//        {
//            int deckNum = _collectPatternDeckNum[pattern];

//            _settingResult = PositionSetting(pattern, deckNum);

//            if (PositionSettingResult.Success == _settingResult)
//            {
//                _summon.transform.position = new Vector3(_summonPosition.x, _summonPosition.y, 0);
//                _summon.GetComponent<SpriteRenderer>().sortingLayerName = GetSortingLayer(_summonPosition.y);
//                AddSummonedSummon();
//                return;
//            }
//            else if (PositionSettingResult.NullPosition == _settingResult)
//            {
//                Debug.Log("PositionSettingResult.NullPosition");

//                _summonArea.GetComponent<SummonArea>().MoveScopePosition();
//                PositionSetting(pattern, deckNum);
//            }
//            else if (PositionSettingResult.NullObject == _settingResult)
//            {
//                Debug.Log("PositionSettingResult.NullObject");

//                AddSummonObjectFull(deckNum);
//                PositionSetting(pattern, deckNum);
//            }
//        }
//    }

//    public void RemoveSummonedSummon(GameObject summon)
//    {
//        _summonProperty = summon.GetComponent<Summon>();

//        float xPos = summon.transform.position.x;
//        int summonOrder = _summonProperty.ObjectFullOrder;
//        string pattern = _summonProperty.Pattern;
//        int deckNum = _collectPatternDeckNum[pattern];

//        _mSortedSummons.Remove(xPos);
//        _summons[deckNum][summonOrder].SetActive(false);
//    }

//    #endregion

//    #region Private Method

//    #region Init
//    private void InitGameHandler(GameHandler gameHandler)
//    {
//        _gameHandler = gameHandler;
//    }

//    private void SetSummonPrefab()
//    {
//        for (int i = 0; i < 4; i++)
//        {
//            _prefabsOfSummon[i] = _summonManager.GetSummonPrefab((int)_userDTO.Deck[i]);
//            Debug.Log("<color=red> SetSummonPrefab : " + _prefabsOfSummon[i].name + "</color>");
//        }
//    }

//    private void SetInfoOfSummonPrefab()
//    {
//        Debug.Log("SetInfoOfSummonPrefab Start");

//        for (int i = 0; i < 4; i++)
//        {
//            Summon summonInfo = _prefabsOfSummon[i].GetComponent<Summon>();

//            if (summonInfo.Number != _deckDTO.SummonNumber[i])
//                Debug.Log("Number is not same In SetInfoOfSummonPrefab ");

//            Debug.Log("Pattern[" + i + "] : " + _deckDTO.Pattern[i]);

//            _collectPatternDeckNum.Add(_deckDTO.Pattern[i], i);
//            summonInfo.Power = _deckDTO.Power[i];
//            summonInfo.Armor = _deckDTO.Armor[i];
//            summonInfo.AttackSpeed = _deckDTO.AttackSpeed[i];
//            summonInfo.Health = _deckDTO.Health[i];
//        }

//        Debug.Log("SetInfoOfSummonPrefab End");
//    }

//    private void CreateSummonObjectFull()
//    {
//        Debug.Log("CreateSummonObjectFull() START");

//        for (int index = 0; index < 4; index++)
//        {
//            for (int num = 0; num < 10; num++)
//            {
//                GameObject instance = Instantiate(_prefabsOfSummon[index]);
//                instance.GetComponent<Summon>().bIsMySummon = true;
//                instance.GetComponent<Summon>().InitGameHandler(_gameHandler);
//                instance.SetActive(false);

//                _summons[index].Add(num, instance);
//            }
//        }
//    }

//    #endregion

//    #region Summon

//    private PositionSettingResult PositionSetting(string pattern, int deckNum)
//    {
//        for (int order = 0; order < _summons[deckNum].Count; order++)
//        {
//            _summon = _summons[deckNum][order];

//            if (_summons[deckNum][order].activeSelf == false)
//            {
//                if (false == IsRandomPosition())
//                {
//                    return PositionSettingResult.NullPosition;
//                }

//                _summonPosition = _summon.transform.position;
//                _summon.GetComponent<Summon>().ObjectFullOrder = order;
//                return PositionSettingResult.Success;
//            }
//        }
//        return PositionSettingResult.NullObject;
//    }

//    private void AddSummonObjectFull(int deckNum)
//    {
//        int count = _summons[deckNum].Count;

//        for (int num = count; num < count + 10; num++)
//        {
//            GameObject instance = Instantiate(_prefabsOfSummon[deckNum]);
//            instance.GetComponent<Summon>().InitGameHandler(_gameHandler);
//            instance.SetActive(false);

//            _summons[deckNum].Add(num, instance);
//        }
//    }

//    private bool IsRandomPosition()
//    {
//        // GetTestPosition() 을 통해서 받아온 위치가 충돌을 일으키는지 확인하기 위함
//        _summon.SetActive(true);

//        for (int num = 0; num < 100; num++)

//        {
//            _summon.transform.position = _scopeOfRecall.GetComponent<ScopeOfRecall>().GetTestPosition(num);
//            _summonCollider = _summon.GetComponent<BoxCollider>();

//            if (VerifyBoxcast(_summonCollider))
//                continue;

//            if (num == 99)
//                break;

//            return true;
//        }

//        _summon.SetActive(false);
//        return false;
//    }

//    private bool VerifyBoxcast(BoxCollider summonCollider)
//    {
//        if (Physics.BoxCast(summonCollider.bounds.center, summonCollider.size, Vector3.forward))
//        {
//            Debug.Log("<Color=red> BoxCast OK </Color>");

//            return true;
//        }
//        return false;
//    }

//    // 정해진 포신션의 Y 좌표를 Layer로 구분하여 아래쪽에 생성된 객체가 
//    // 화면 위로 올라로도록 만든다.
//    private string GetSortingLayer(float PosY)
//    {
//        float gap = 0.4f;

//        for (int i = 0; i < 20; i++)
//        {
//            if (1f + (gap * i) <= PosY && PosY < 1.4f + (gap * i))
//            {
//                return SortingLayer.layers[SortingLayer.layers.Length - 1 - i].name;
//            }
//        }

//        return SortingLayer.layers[0].name;
//    }

//    private void AddSummonedSummon()
//    {
//        float xPos = _summon.transform.position.x;

//        _mSortedSummons.Add(xPos, _summon);

//        //Debug.Log("TEST foreFrontSummon");

//        //foreach (GameObject summon in _foreFrontSummon.Values)
//        //{
//        //    Debug.Log("summonPosition.x : " + summon.transform.position.x);
//        //}
//    }

//    #endregion

//    #endregion
//}