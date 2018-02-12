using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FieldType { Air, Ground }
public enum AttackType { Front, Back, WeakHealth, StrongDmg, BuffUnit, AirFirst, GroundFirst }

public class Unit : IPoolable
{
    private List<string> buffingNames;

    private UnitData data;

    public UnitData     Data                { get { return data; } set { data = value; } }
    public GameObject   UnitObject          { get; set; }
    public float        AttackCool          { get; set; }
    public float        Health              { get; set; }
    public bool         IsMyTeam            { get; set; }
    public bool         IsUsed              { get; private set; }

    public Dictionary<int, Buff> BuffingUnitIndex { get; set; }

    public Unit( GameObject unitObject, bool isMyTeam )
    {
        buffingNames = new List<string>();
        data = new UnitData();

        UnitObject = unitObject;
        AttackCool = 0;
        Health = 0;

        IsMyTeam = isMyTeam;
        SetIsUsed(false);

        if (!IsMyTeam)
            unitObject.transform.localScale = new Vector3(-1, 1, 1);

        BuffingUnitIndex = new Dictionary<int, Buff>();
    }

    public void Initialize(UnitData data, Vector2 position)
    {
        SetIsUsed(true);

        Data = data;
        Health = Data.Health;

        InitUnitObject(position);

        AttackCool = 0;
        BuffingUnitIndex.Clear();
    }

    public void Activate()
    {
        AttackCool += Setting.AttackCool;

        ActivateBuff();

        float attackSpeed = CalculateAttackSpeed();
        if (attackSpeed <= AttackCool)
            OnAttack();
    }

    public void OnSummoned()
    {
        if (data.IsBuff)
            GiveBuff();

        ReceiveBuff();
    }

    public void OnAttacked(Unit unit)
    {
        float damage = CalculateEnemyDamage(unit, Data.FieldType);
        float armor = CalculateArmor();

        //Debug.Log(IsMyTeam + " , Damage : " + damage);

        if (armor != 0)
            damage -= damage * armor / 100;

        Health -= damage;

        if (IsMyTeam)
            UI.MyHealthState.text = Health.ToString();
        else
            UI.EnemyHealthState.text = Health.ToString();

        if (Health <= 0)
            OnDied();
    }

    public void SetIsUsed(bool isUsed)
    {
        IsUsed = isUsed;
        UnitObject.SetActive(isUsed);
    }

    private void OnDied()
    {
        if (data.IsBuff)
            ReleaseBuff();

        SetIsUsed(false);
    }

    private void OnAttack()
    {
        Unit target = TargetResearcher.Find(Data.AttackType, !IsMyTeam);

        if (target == null)
        {
            OnAttackSummoner();
            return;
        }

        target.OnAttacked(this);
        AttackCool = 0;
    }

    private void OnAttackSummoner()
    {
        float damage = CalculateEnemyDamage(this, FieldType.Ground);

        if (IsMyTeam)
            SpawnManager.EnemySummoner.OnAttacked(damage);
        else
            SpawnManager.MySummoner.OnAttacked(damage);
    }

    private void InitUnitObject(Vector2 position)
    {
        BoxCollider boxCollider = UnitObject.GetComponent<BoxCollider>();

        boxCollider.center = Data.ColliderCenter;
        boxCollider.size = Data.ColliderSize;

        UnitObject.GetComponent<SpriteRenderer>().sprite = Data.Sprite;
        UnitObject.GetComponent<SpriteRenderer>().sortingLayerName = GetSortingLayer(position.y);

        UnitObject.transform.position = position;
    }

    private string GetSortingLayer(float PosY)
    {
        float gap = 0.2f;

        for (int i = 0; i < 40; i++)
            if (1f + (gap * i) <= PosY && PosY < 1.4f + (gap * i))
                return SortingLayer.layers[SortingLayer.layers.Length - 1 - i].name;

        return SortingLayer.layers[0].name;
    }

    private void ActivateBuff()
    {
        if (!data.IsBuff)
            return;

        if (!Data.Buff.Activate())
        {
            ReleaseBuff();
            data.IsBuff = false;
        }
    }

    private void GiveBuff()
    {
        for(int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!IsSameTeam(SpawnManager.Units[index]))
                continue;

            if (!BeInDistance(index))
                continue;

            AddBuffToOther(index);       
        }
    }

    private void ReceiveBuff()
    {
        for(int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!HasBuff(SpawnManager.Units[index]))
                continue;

            if (!BeInDistance(index))
                continue;

            AddBuffToMe(index);
        }
    }

    private void ReleaseBuff()
    {
        for(int index = 0; index < SpawnManager.Units.Count; index++)
        {
            if (!IsSameTeam(SpawnManager.Units[index]))
                continue;

            RemoveBuffToOther(index);
        }
    }

    private bool BeInDistance(int index)
    {
        Vector2 to = SpawnManager.Units[index].UnitObject.transform.position;
        float distance = MeasureDistanceFromMe(to);

        if (data.Buff.Distance < distance)
            return false;
        return true;
    }

    private void AddBuffToOther(int index)
    {
        int key = SpawnManager.Units.IndexOf(this);

        if (!SpawnManager.Units[index].BuffingUnitIndex.ContainsKey(key))
            SpawnManager.Units[index].BuffingUnitIndex.Add(key, data.Buff);
    }

    private void AddBuffToMe(int index)
    {
        if (!BuffingUnitIndex.ContainsKey(index))
            BuffingUnitIndex.Add(index, SpawnManager.Units[index].data.Buff);
    }

    private void RemoveBuffToOther(int index)
    {
        int myIndex = SpawnManager.Units.IndexOf(this);

        if (SpawnManager.Units[index].BuffingUnitIndex.ContainsKey(myIndex))
            SpawnManager.Units[index].BuffingUnitIndex.Remove(myIndex);
    }

    private bool IsSameTeam(Unit unit)
    {
        if (!unit.IsUsed || (unit.IsMyTeam != IsMyTeam))
            return false;
        return true;
    }

    private bool HasBuff(Unit unit)
    {
        if (!unit.IsUsed || !unit.data.IsBuff || (unit.IsMyTeam != IsMyTeam))
            return false;
        return true;
    }

    private float MeasureDistanceFromMe(Vector2 to)
    {
        Vector2 from = UnitObject.transform.position;
        float xDis = from.x - to.x;
        float yDis = from.y - to.y;
        return Mathf.Sqrt(Mathf.Pow(xDis, 2) + Mathf.Pow(yDis, 2));
    }

    private float CalculateAttackSpeed()
    {
        buffingNames.Clear();

        float buffedSpeed = 0;

        foreach (Buff buff in BuffingUnitIndex.Values)
        {
            if (buffingNames.Contains(buff.Name))
                continue;

            buffedSpeed += data.AttackSpeed * buff.SpeedMultiple;
            buffingNames.Add(buff.Name);
        }

        if (data.AttackSpeed - buffedSpeed < Setting.MaxSpeed)
            return Setting.MaxSpeed;

        return data.AttackSpeed - buffedSpeed;
    }

    private float CalculateEnemyDamage(Unit enemy, FieldType myFieldType)
    {
        buffingNames.Clear();

        if (myFieldType == FieldType.Air)
            return CalculateEnemyAirDamage(enemy);
        else // (myFieldType == FieldType.Ground)
            return CalculateEnemyGroundDamage(enemy);
    }

    private float CalculateEnemyAirDamage(Unit enemy)
    {
        float basicDamage = enemy.data.AirDamage;
        float buffDamage = basicDamage;

        foreach (Buff buff in enemy.BuffingUnitIndex.Values)
        {
            if (buffingNames.Contains(buff.Name))
                continue;

            buffDamage += basicDamage * buff.AirDmgMultiple;
            buffingNames.Add(buff.Name);
        }

        return basicDamage + buffDamage;
    }

    private float CalculateEnemyGroundDamage(Unit enemy)
    {
        float basicDamage = enemy.data.GroundDamage;
        float buffDamage = basicDamage;

        foreach (Buff buff in enemy.BuffingUnitIndex.Values)
        {
            if (buffingNames.Contains(buff.Name))
                continue;

            buffDamage += basicDamage * buff.GroundDmgMultiple;
            buffingNames.Add(buff.Name);
        }

        return basicDamage + buffDamage;
    }

    private int CalculateArmor()
    {
        buffingNames.Clear();

        int buffedArmor = 0;

        foreach (Buff buff in BuffingUnitIndex.Values)
        {
            if (buffingNames.Contains(buff.Name))
                continue;

            buffedArmor +=  buff.Armor;
            buffingNames.Add(buff.Name);
        }

        if ( Setting.MaxArmor < data.Armor + buffedArmor )
            return Setting.MaxArmor;

        return data.Armor + buffedArmor;
    }
}