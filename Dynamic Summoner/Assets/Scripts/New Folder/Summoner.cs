using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner
{
    private GameObject summonerPrefab;
    private GameObject summonerObject;
    private float defaultHealth;
    private int defaultArmor;
    private int growthRateOfHealth;
    private int growthRateOfArmor;
    private bool isMySummoner;

    public float Health { get; set; }
    public int Armor { get; set; }

    public Summoner(bool isMySummoner)
    {
        summonerPrefab = (GameObject)Resources.Load("Unit");
        summonerObject = GameObject.Instantiate(summonerPrefab);
        SetActive(false);

        defaultHealth = 3000;
        defaultArmor = 10;

        growthRateOfHealth = 100;
        growthRateOfArmor = 1;

        this.isMySummoner = isMySummoner;

        if (!isMySummoner)
            summonerObject.transform.localScale = new Vector3(-1, 1, 1);

        Health = 0;
        Armor = 0;
    }

    public void Initiate(int level)
    {
        Health = defaultHealth + growthRateOfHealth * level;
        Armor = defaultArmor + growthRateOfArmor * level;
        SetActive(true);
    }

    public void OnAttacked(float damage)
    {
        damage -= damage * Armor / 100;
        Health -= damage;

        if (isMySummoner)
            UI.MyHealthState.text = "Summoner Health : " + Mathf.Floor(Health);
        else
            UI.EnemyHealthState.text = "Summoner Health : " + Mathf.Floor(Health);

        if (Health <= 0)
        {
            Battle.GameOver(!isMySummoner);
        }
    }

    public void SetPosition(Vector2 position)
    {
        summonerObject.transform.position = position;
    }

    public void SetSprite(Sprite sprite)
    {
        summonerObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void SetActive(bool active)
    {
        summonerObject.gameObject.SetActive(active);
    }
}
