//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//public class SummonHandler : MonoBehaviour {

//    //public List<GameObject> playerSpawn  { get { return playerSpawn; } set { if (playerSpawn == null) playerSpawn = value; } }
//    public List<GameObject> opponentSpawn { get; set; }
//    public Text healthState { get; set; }

//    public bool isPlayerSummon;
//    public SummonType type;
//    public float attackSpeed;
//    public float airDamage;
//    public float groundDamage;
//    public float armor;
//    public float health;

//    private int attackCount;
//    private int maxAttackCount;

//    // Use this for initialization
//    void Awake () {
//        attackCount = 0;
//        maxAttackCount = 200;
//	}
	
//	// Update is called once per frame
//	void Update () {

//        if (GameHandler.gameState == GameState.Main)
//            return;

//        Debug.Log("Enter!!!! Attack : " + attackCount);

//        attackCount++;

//        if (attackCount != maxAttackCount)
//            return;

//        Debug.Log("Attack!!!!");

//        if (opponentSpawn == null)
//        {
//            Debug.Log("NULL!!!!");
//            attackCount = 0;
//            return;
//        }

//        GameObject enemy = FindOpponentSummon();

//        if (enemy == null)
//        {
//            attackCount = 0;
//            return;
//        }

//        Attack(enemy);

//        attackCount = 0;
//	}

//    private GameObject FindOpponentSummon()
//    {
//        IEnumerable<GameObject> gameObjects = from opponents in opponentSpawn
//                                              orderby opponents.transform.position.x
//                                              where opponents.activeSelf
//                                              select opponents;
//        if (isPlayerSummon)
//            return gameObjects.LastOrDefault();
//        else
//            return gameObjects.FirstOrDefault();
//    }

//    private void Attack(GameObject enemy)
//    {
//        enemy.GetComponent<SummonHandler>().BeAttacked(airDamage, groundDamage);
//    }

//    public void BeAttacked(float airDamage, float groundDamage)
//    {
//        Debug.Log("health : " + health);

//        if (type == SummonType.Air)
//            health -= airDamage;
//        else if (type == SummonType.Ground)
//            health -= groundDamage;

//        if (health <= 0)
//        {
//            if(gameObject.activeSelf)
//            gameObject.SetActive(false);
//        }

//        healthState.text = health.ToString();
//    }
//}