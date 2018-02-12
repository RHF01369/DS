//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//public class SummonSpawn : MonoBehaviour {
    
//    [SerializeField] private GameObject summon;
//    [SerializeField] private bool isPlayerSpawn;

//    public List<GameObject> summonSpawn { get; private set; }
//    //public Pool<GameObject> ddd; ddd.Spawn;

//    void Awake()
//    {
//        GameObject gameObject;
//        summonSpawn = new List<GameObject>();

//        for (int i = 0; i < 20; i++)
//        {
//            // Instantiate를 사용하기 위해서는 MomoBehaviour를 사용해야 한다.
//            gameObject = Instantiate(summon);
//            gameObject.SetActive(false);
//            summonSpawn.Add(gameObject);
//        }
//    }

//    GameObject AddPool()
//    {
//        GameObject gameObject;
//        int count = summonSpawn.Count;

//        for (int number = count; number < count + 10; number++)
//        {
//            gameObject = Instantiate(summon);
//            gameObject.SetActive(false);
//            summonSpawn.Add(gameObject);
//        }

//        return summonSpawn[count];
//    }

//    public void Summon(UnitData deckData, Vector2 position, List<GameObject> opponentSpawn, Text healthState)
//    {
//        Debug.Log("opponentSpawn" + opponentSpawn);

//        GameObject      summon          = GetUnusedSummon();
//        SpriteRenderer  spriteRenderer  = summon.GetComponent<SpriteRenderer>();
//        SummonAbility   summonAbility   = summon.GetComponent<SummonAbility>();
//        SummonHandler   summonHandler   = summon.GetComponent<SummonHandler>();
//        BoxCollider     boxCollider     = summon.GetComponent<BoxCollider>();

//        SetSummonInfo(deckData, spriteRenderer, summonAbility, summonHandler, boxCollider);

//        spriteRenderer.sortingLayerName = GetSortingLayerName(position.y);
//        summon.transform.position       = position;
//        summonHandler.opponentSpawn     = opponentSpawn;
//        summonHandler.healthState       = healthState;

//        Debug.Log("summonHandler.opponentSpawn" + summonHandler.opponentSpawn);

//        if (!isPlayerSpawn)
//            summon.transform.localScale = new Vector2(summon.transform.localScale.x * -1f, summon.transform.localScale.y * 1f);

//        summon.SetActive(true);
//    }

//    private GameObject GetUnusedSummon()
//    {
//        while (true)
//        {
//            foreach (GameObject summon in summonSpawn)
//            {
//                if (!summon.activeSelf)
//                    return summon;
//            }

//            return AddPool();
//        }
//    }

//    private void SetSummonInfo(UnitData deckData, SpriteRenderer spriteRenderer, SummonAbility summonAttribute, SummonHandler summonHandler, BoxCollider boxCollider)
//    {
//        spriteRenderer.sprite           = deckData.sprite;

//        summonAttribute.number          = deckData.number;
//        summonAttribute.pattern         = deckData.pattern;
//        summonAttribute.type            = deckData.type;
//        summonAttribute.attackSpeed     = deckData.attackSpeed;
//        summonAttribute.airDamage       = deckData.airDamage;
//        summonAttribute.groundDamage    = deckData.groundDamage;
//        summonAttribute.armor           = deckData.armor;
//        summonAttribute.health          = deckData.health;

//        summonHandler.isPlayerSummon    = isPlayerSpawn;
//        summonHandler.type              = deckData.type;
//        summonHandler.attackSpeed       = deckData.attackSpeed;
//        summonHandler.airDamage         = deckData.airDamage;
//        summonHandler.groundDamage      = deckData.groundDamage;
//        summonHandler.armor             = deckData.armor;
//        summonHandler.health            = deckData.health;

//        boxCollider.center  = deckData.colliderCenter;
//        boxCollider.size    = deckData.colliderSize;
//    }

//    // 정해진 포신션의 Y 좌표를 Layer로 구분하여 아래쪽에 생성된 객체가 
//    // 화면 위로 올라로도록 만든다.
//    private string GetSortingLayerName(float PosY)
//    {
//        float gap = 0.4f;

//        for (int i = 0; i < 20; i++)
//        {
//            if (1f + (gap * i) <= PosY && PosY < 1.4f + (gap * i))
//                return SortingLayer.layers[SortingLayer.layers.Length - 1 - i].name;
//        }

//        return SortingLayer.layers[0].name;
//    }
//}
