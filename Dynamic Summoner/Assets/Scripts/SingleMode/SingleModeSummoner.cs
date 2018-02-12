using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class SingleModeSummoner : MySumonner {

//    enum PositionSettingResult { Success, NullPosition, NullObject };

//    [SerializeField] private GameObject SummonArea;
//    [SerializeField] private GameObject ScopeOfRecall;
//    [SerializeField] private GameObject BottomLeftBoundary;
//    [SerializeField] private GameObject TopRightBoundary;

//    private Dictionary<int, GameObject>[] _summons;
//    private Dictionary<string, int> _collectPatternDeckNum;
//    private SortedDictionary<float, GameObject> _foreFrontSummon;
//    private GameObject[] _prefabsOfSummon;
//    private GameObject _summon;
//    private Vector3 _summonPosition;
//    private Summon _summonProperty;

//    private Ray _ray;
//    private BoxCollider _summonCollider;

//    private SummonManager _summonManager;

//    private PositionSettingResult _settingResult;

//    private void Awake()
//    {
//        _summons = new Dictionary<int, GameObject>[4];

//        for (int i = 0; i < 4; i++)
//            _summons[i] = new Dictionary<int, GameObject>();

//        _collectPatternDeckNum = new Dictionary<string, int>();

//        _foreFrontSummon = new SortedDictionary<float, GameObject>(new ReverseComparer<float>(Comparer<float>.Default));

//        _prefabsOfSummon = new GameObject[4];

//        //for (int i = 0; i < 4; i++)
//        //    _prefabsOfSummon[i] = new GameObject();

//        _ray.direction = new Vector3(0, 0, 1f);

//        _summonManager = SummonManager.Instance;
//    }

//    /// <summary>
//    /// DB에서 가지고 온 deck의 정보를 가지고, summonManager을 이용해서 Prefab정보를 가지고 온다.
//    /// </summary>
//    /// <param name="summonNumbers"></param>
//    public override void SetSummonPrefab(long[] summonNumbers)
//    {
//        for (int i = 0; i < 4; i++)
//        {
//            _prefabsOfSummon[i] = _summonManager.GetSummonPrefab((int)summonNumbers[i]);
//            Debug.Log("<color=red> SetSummonPrefab : " + _prefabsOfSummon[i] + "</color>");
//        }
//    }

//    /// <summary>
//    /// 덱으로 선택된 프리팹을 가지고 와서 내가 가진 소환물을 능력치로 설정한다.
//    /// </summary>
//    /// <param name="deckDTO"></param>
//    public override void SetInfoOfSummonPrefab(DeckDTO deckDTO)
//    {
//        Debug.Log("SetInfoOfSummonPrefab Start");

//        for (int i = 0; i < 4; i++)
//        {
//            Summon summonInfo = _prefabsOfSummon[i].GetComponent<Summon>();

//            if (summonInfo.Number != deckDTO.SummonNumber[i])
//                Debug.Log("Number is not same In SetInfoOfSummonPrefab ");

//            DictionaryOfPattern.Add(deckDTO.Pattern[i], i);
//            summonInfo.Power = deckDTO.Power[i];
//            summonInfo.Armor = deckDTO.Armor[i];
//            summonInfo.AttackSpeed = deckDTO.AttackSpeed[i];
//            summonInfo.Health = deckDTO.Health[i];
//        }

//        Debug.Log("SetInfoOfSummonPrefab End");
//    }

//    /// <summary>
//    /// 프리팹을 인스턴스 시켜서 객체풀을 만든다.
//    /// </summary>
//    public override void CreateSummonObjectFull()
//    {
//        for (int index = 0; index < 4; index++)
//        {
//            for (int num = 0; num < 10; num++)
//            {
//                GameObject instance = Instantiate(_prefabsOfSummon[index]);

//                instance.SetActive(false);

//                _summons[index].Add(num, instance);
//            }
//        }
//    }

//    public override SortedDictionary<float, GameObject> GetFrontSummons()
//    {
//        return _foreFrontSummon;
//    }

//    // 소환
//    override public void Summon(string pattern)
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
//            else if(PositionSettingResult.NullPosition == _settingResult)
//            {
//                Debug.Log("PositionSettingResult.NullPosition");

//                SummonArea.GetComponent<SummonArea>().MoveScopePosition();
//                PositionSetting(pattern, deckNum);
//            }
//            else if(PositionSettingResult.NullObject == _settingResult)
//            {
//                Debug.Log("PositionSettingResult.NullObject");

//                AddSummonObjectFull(deckNum);
//                PositionSetting(pattern, deckNum);
//            }
//        }
//    }

//    override public void RemoveSummonedSummon(GameObject summon)
//    {
//        _summonProperty = summon.GetComponent<Summon>();

//        float xPos = summon.transform.position.x;
//        int summonOrder = _summonProperty.SummoningOrder;
//        string pattern = _summonProperty.Pattern;
//        int deckNum = _collectPatternDeckNum[pattern];

//        Debug.Log("xPos : " + xPos);

//        foreach (var Position in _foreFrontSummon.Keys)
//        {
//            Debug.Log("Dictionary Keys : " + Position);
//        }

//        _foreFrontSummon.Remove(xPos);
//        _summons[deckNum][summonOrder].SetActive(false);
//    }

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
//                _summon.GetComponent<Summon>().SummoningOrder = order;
//                return PositionSettingResult.Success;
//            }
//        }
//        return PositionSettingResult.NullObject;
//    }

//    /// <summary>
//    /// 객체풀로 만든 소환수를 모두 소환했을 때, 객체풀을 10개 더 만든다.
//    /// </summary>
//    /// <param name="deckNum"></param>
//    private void AddSummonObjectFull(int deckNum)
//    {
//        int count = _summons[deckNum].Count;

//        for (int num = count; num < count + 10; num++)
//        {
//            GameObject instance = Instantiate(_prefabsOfSummon[deckNum]);

//            instance.SetActive(false);

//            _summons[deckNum].Add(num, instance);
//        }
//    }

//    /// <summary>
//    /// 소환물을 소환할 위치를 정하는 함수
//    /// </summary>
//    /// <returns></returns>
//    private bool IsRandomPosition( )
//    {
//        // GetTestPosition() 을 통해서 받아온 위치가 충돌을 일으키는지 확인하기 위함
//        _summon.SetActive(true);

//        for (int num = 0; num < 100; num++)
//        {
//            _summon.transform.position = ScopeOfRecall.GetComponent<ScopeOfRecall>().GetTestPosition(num);
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

//    /// <summary>
//    /// Random 으로 선택된 포지션에 이미 소환물이 있는지 충돌을 판정한다.
//    /// </summary>
//    /// <param name="summonCollider"></param>
//    /// <returns></returns>
//    private bool VerifyBoxcast(BoxCollider summonCollider)
//    {
//        if (Physics.BoxCast(summonCollider.bounds.center, summonCollider.size,  Vector3.forward))
//        {
//            Debug.Log("<Color=red> BoxCast OK </Color>");

//            return true;
//        }
//        return false;
//    }

//    /// <summary>
//    /// 정해진 포신션의 Y 좌표를 Layer로 구분하여 아래쪽에 생성된 객체가 
//    /// 화면 위로 올라로도록 만든다.
//    /// </summary>
//    /// <param name="PosY"></param>
//    /// <returns></returns>
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

//    // DB에서 소환물들에 대한 Pattern 정보를 저장한다.
//    private Dictionary<string, int> DictionaryOfPattern
//    {
//        get
//        {
//            return _collectPatternDeckNum;
//        }
//        set
//        {
//            if (_collectPatternDeckNum.Count > 4)
//            {
//                Debug.Log("DictionaryOfPattern.Count is Over 4");
//                return;
//            }

//            _collectPatternDeckNum = value;
//        }
//    }


//    private void AddSummonedSummon()
//    {
//        float xPos = _summon.transform.position.x;

//        _foreFrontSummon.Add(xPos, _summon);

//        //Debug.Log("TEST foreFrontSummon");

//        //foreach (GameObject summon in _foreFrontSummon.Values)
//        //{
//        //    Debug.Log("summonPosition.x : " + summon.transform.position.x);
//        //}
//    }

//    private void DeleteSummonedSummon()
//    {

//    }
//}

//public class SingleModeSummoner : MySumonner
//{


//    override public void RemoveSummonedSummon(GameObject summon)
//    {
//        _summonProperty = summon.GetComponent<Summon>();

//        float xPos = summon.transform.position.x;
//        int summonOrder = _summonProperty.SummoningOrder;
//        string pattern = _summonProperty.Pattern;
//        int deckNum = _collectPatternDeckNum[pattern];

//        Debug.Log("xPos : " + xPos);

//        foreach (var Position in _foreFrontSummon.Keys)
//        {
//            Debug.Log("Dictionary Keys : " + Position);
//        }

//        _foreFrontSummon.Remove(xPos);
//        _summons[deckNum][summonOrder].SetActive(false);
//    }

///////////////////////////////////////////////////////////



//    /// <summary>
//    /// 소환물을 소환할 위치를 정하는 함수
//    /// </summary>
//    /// <returns></returns>
//    private bool IsRandomPosition()
//    {
//        // GetTestPosition() 을 통해서 받아온 위치가 충돌을 일으키는지 확인하기 위함
//        _summon.SetActive(true);

//        for (int num = 0; num < 100; num++)
//        {
//            _summon.transform.position = ScopeOfRecall.GetComponent<ScopeOfRecall>().GetTestPosition(num);
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

//    /// <summary>
//    /// Random 으로 선택된 포지션에 이미 소환물이 있는지 충돌을 판정한다.
//    /// </summary>
//    /// <param name="summonCollider"></param>
//    /// <returns></returns>
//    private bool VerifyBoxcast(BoxCollider summonCollider)
//    {
//        if (Physics.BoxCast(summonCollider.bounds.center, summonCollider.size, Vector3.forward))
//        {
//            Debug.Log("<Color=red> BoxCast OK </Color>");

//            return true;
//        }
//        return false;
//    }

//    /// <summary>
//    /// 정해진 포신션의 Y 좌표를 Layer로 구분하여 아래쪽에 생성된 객체가 
//    /// 화면 위로 올라로도록 만든다.
//    /// </summary>
//    /// <param name="PosY"></param>
//    /// <returns></returns>
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

//    // DB에서 소환물들에 대한 Pattern 정보를 저장한다.
//    private Dictionary<string, int> DictionaryOfPattern
//    {
//        get
//        {
//            return _collectPatternDeckNum;
//        }
//        set
//        {
//            if (_collectPatternDeckNum.Count > 4)
//            {
//                Debug.Log("DictionaryOfPattern.Count is Over 4");
//                return;
//            }

//            _collectPatternDeckNum = value;
//        }
//    }


//    private void AddSummonedSummon()
//    {
//        float xPos = _summon.transform.position.x;

//        _foreFrontSummon.Add(xPos, _summon);

//        //Debug.Log("TEST foreFrontSummon");

//        //foreach (GameObject summon in _foreFrontSummon.Values)
//        //{
//        //    Debug.Log("summonPosition.x : " + summon.transform.position.x);
//        //}
//    }

//    private void DeleteSummonedSummon()
//    {

//    }
//}