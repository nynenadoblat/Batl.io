using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class Creep : NetworkBehaviour
{
    [SerializeField] HealthComponent health;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] float rateOfFire = 0.7f;

    [Space]

    [SerializeField] MeshRenderer teamIndicator;
    [SerializeField] Material friendly;
    [SerializeField] Material enemy;

    public HealthComponent Health => health;

    float timerUpdateTarget;
    float currentRate;

    NavMeshAgent agent;
    HealthComponent target;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        EventsHolder.creepSpawned?.Invoke(health);

        health.onTeamSet += Team_Seted;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        timerUpdateTarget += Time.deltaTime;
        currentRate += Time.deltaTime;

        if (timerUpdateTarget > 1)
        {
            FindTarget();
            timerUpdateTarget = 0;
        }

        if (target)
        {
            agent.SetDestination(target.transform.position);

            var distance = Vector3.Distance(target.transform.position, transform.position) < agent.stoppingDistance * 1.5f;

            if (distance && agent.desiredVelocity.magnitude < 0.5f)
            {
                Fire();

                Vector3 targetDir = target.transform.position - transform.position;
                targetDir.y = 0.0f;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * 100);

            }
        }
    }

    void Fire()
    {
        if (target && currentRate > rateOfFire && !GameManager.Instance.complete)
        {
            var dir = ((target.transform.position) - transform.position).normalized;
            var pos = transform.position - new Vector3(0, 0.9f, 0);
            var p = Instantiate(projectilePrefab, pos, Quaternion.identity);
            var projectileData = new ProjectileData
            {
                critChance = 10,
                baseDamege = 1,
                direction = dir,
                ownerID = NetworkObjectId,
                team = Health.Team,
                isCosmetic = false
            };
            p.Init(projectileData);
            currentRate = 0;

            FireClientRpc(pos, dir);
        }
    }

    [ClientRpc]
    private void FireClientRpc(Vector3 pos, Vector3 dir)
    {
        if (IsServer) return;

        var p = ProjectilePool.Get(pos);
        var projectileData = new ProjectileData
        {
            critChance = 10,
            baseDamege = 1,
            direction = dir,
            ownerID = NetworkObjectId,
            team = Health.Team,
            isCosmetic = true,
            mat = (GameManager.Instance != null && GameManager.Instance.MineTeam == Health.Team) ? friendly : enemy
        };
        p.Init(projectileData);
    }

    void FindTarget()
    {
        var targets = GameManager.Instance.citadels.Where(c => c && c.Health.Team != health.Team).Select(t => t.Health).ToList();

        if (targets.Count > 0)
        {
            target = targets[0];
        }
        else
        {
            target = null;
        }
    }

    private void LookAt(Vector3 t)
    {
        var dir = t - transform.position;
        var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle, transform.up);
    }

    public void Team_Seted(Team team)
    {
        if (GameManager.Instance.MineTeam == team)
        {
            teamIndicator.material = friendly;
        }
        else
        {
            teamIndicator.material = enemy;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventsHolder.creepAnniged?.Invoke(health);
    }
}