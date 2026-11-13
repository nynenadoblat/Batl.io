using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Tower : NetworkBehaviour
{
    [SerializeField] HealthComponent health;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] Transform muzzlePoint;

    public HealthComponent Health => health;

    HealthComponent target;

    float timerRefrashTarget;
    private float currentRate;

    private void Start()
    {
        health.onAnniged += Anniged;
    }

    private void Anniged(ulong annigilatorID)
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;

        timerRefrashTarget += Time.deltaTime;
        currentRate += Time.deltaTime;

        if (timerRefrashTarget > 1)
        {
            UpdateTarget();
            timerRefrashTarget = 0;
        }

        if (target && currentRate > 0.7f && !GameManager.Instance.complete)
        {
            Fire();
            currentRate = 0;
        }
        else
        {
            UpdateTarget();
        }
    }

    void Fire()
    {
        Vector3 targetPos = target.transform.position - new Vector3(0, 1, 0);
        var dir = (targetPos - muzzlePoint.position).normalized;
        var pos = muzzlePoint.position;
        var p = Instantiate(projectilePrefab, pos, Quaternion.identity);
        var projectileData = new ProjectileData
        {
            critChance = 7,
            baseDamege = 7,
            direction = dir,
            ownerID = NetworkObjectId,
            team = Health.Team,
            isCosmetic = false
        };
        p.Init(projectileData);

        FireClientRpc(pos, dir);
    }

    [ClientRpc]
    private void FireClientRpc(Vector3 pos, Vector3 dir)
    {
        if (IsServer) return;

        var p = ProjectilePool.Get(pos);
        var projectileData = new ProjectileData
        {
            critChance = 7,
            baseDamege = 7,
            direction = dir,
            ownerID = NetworkObjectId,
            team = Health.Team,
            isCosmetic = true
        };
        p.Init(projectileData);
    }

    void UpdateTarget()
    {
        var target = GameManager.Instance.FindTarget(health);
        if (target && Vector3.Distance(target.transform.position, transform.position) < 7)
        {
            this.target = target;
        }
        else
        {
            this.target = null;
        }
    }
}