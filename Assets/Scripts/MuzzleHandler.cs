using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleHandler : MonoBehaviour
{
    [SerializeField] float fireRateMultiplier = 1f;
    [SerializeField] float damageMultiplier = 1f;
    [SerializeField] float precisionMultiplier = 1f;
    [SerializeField] float critChanceMultiplier = 1f;
    [SerializeField] float projectileSpeedMultiplier = 1f;

    [Space]

    [SerializeField] Muzzle[] muzzles;

    Player player;

    float currentRate;

    public void Init(Player player)
    {
        this.player = player;
    }

    private void Update()
    {
        currentRate += Time.deltaTime;
    }

    public void Fire()
    {
        if (currentRate < player.rateOfFire * fireRateMultiplier)
            return;

        currentRate = 0;

        var projectileData = new ProjectileData
        {
            baseDamege = (int)((float)player.baseDamage * damageMultiplier),
            critChance = (int)((float)player.CritChance * critChanceMultiplier),
            speed = (int)((float)player.SpeedProjectile * projectileSpeedMultiplier),
            ownerID = player.NetworkObjectId,
            team = player.Health.Team,
        };

        for (int i = 0; i < muzzles.Length; i++)
        {
            float angle = muzzles[i].Point.rotation.eulerAngles.y;
            var precision = player.Precision * precisionMultiplier;
            angle += Random.Range(-precision, precision);
            var dir = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, Mathf.Cos(Mathf.Deg2Rad * angle));

            dir.Normalize();
            var pos = muzzles[i].Point.position;
            var projectile = ProjectilePool.Get(pos);
            projectileData.direction = dir;
            projectileData.mat = GameManager.Instance.MineTeam == player.Health.Team ? muzzles[i].friendly : muzzles[i].enemy;

            projectile.Init(projectileData);
        }
    }
}