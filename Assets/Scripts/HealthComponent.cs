using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class HealthComponent : MonoBehaviour
{
    public int MaxValue = 100;
    [field: SerializeField]
    public int Value { get; private set; }
    public bool IsAlive { get; set; }
    [field: SerializeField]
    public int Evasion { get; set; }
    [field: SerializeField]
    public int Armor { get; set; }
    [field: SerializeField]
    public int HealRate { get; set; } = 5;
    [field: SerializeField]
    public int HealValue { get; set; } = 3;

    public ulong OwnerID { get; set; }

    public Action<ulong> onAnniged;
    public Action<int> onIncreaseHealth;
    public Action<int, ulong> onDamage;
    public Action<Team> onTeamSet;

    [field: SerializeField]
    Team team;
    public Team Team 
    {
        get => team;
        set
        {
            team = value;
            onTeamSet?.Invoke(value);
        } 
    }

    public ulong LastDamagedOwnerID { get; set; }
    public float TimeLastDamage { get; set; }
    public int AnnigedTime { get; set; }

    float healTimer;

    private void Start()
    {
        IsAlive = true;

        Value = MaxValue;
    }

    public void Damage(int value, ulong ownerID)
    {
        if (Random.Range(0, 100) < Evasion)
            return;

        value -= Armor;
        Value -= value;

        LastDamagedOwnerID = ownerID;

        TimeLastDamage = Time.time;

        onDamage?.Invoke(value, ownerID);        
    }

    public void DamageNetworkReceived(int value, ulong ownerID, int damageTime)
    {
        Value -= value;
        LastDamagedOwnerID = ownerID;
        TimeLastDamage = Time.time;
    }

    public void Heal()
    {
        Value += HealValue;
    }

    public void IncreaseMaxHealth(int value, bool networkMode = false)
    {
        MaxValue += value;
        Value += value;
        Value = Mathf.Clamp(Value, 0, MaxValue);
    }

    public void DecreaseMaxHealth(int value)
    {
        MaxValue -= value;
        if (Value > MaxValue)
        {
            Value = MaxValue;
        }
    }

    public void Annigilaion(bool silentMode = false)
    {
        Value = 0;

        IsAlive = false;

        onAnniged?.Invoke(LastDamagedOwnerID);
    }

    private void Update()
    {
        CheckHealTimer();

        if (IsAlive && Value <= 0)
        {
            IsAlive = false;

            AnnigedTime = MultiplayerManager.ServerTime;

            StartCoroutine(WaitClientAnnigedData());

            IEnumerator WaitClientAnnigedData()
            {
                yield return new WaitForSeconds(0.3f);

                onAnniged?.Invoke(LastDamagedOwnerID);
            }

        }
    }

    void CheckHealTimer()
    {
        healTimer += Time.deltaTime;

        if (TimeLastDamage + HealRate > Time.time)
            return;
        
        if(Value < MaxValue && healTimer > 1f)
        {
            Heal();
            healTimer = 0;
        }
        
    }
}