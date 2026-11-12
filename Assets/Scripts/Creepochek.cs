using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Creepochek : NetworkBehaviour
{
    [SerializeField] HealthComponent health;

    NavMeshAgent agent;

    public int score;

    float moveTimer;

    int moveResetTimerThresold = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        health.onAnniged += Anniged;
        health.onDamage += Damaged;
    }

    private void Damaged(int arg1, ulong arg2)
    {
        EventsHolder.onCrepochekDamage?.Invoke(health);
    }

    private void Start()
    {
        EventsHolder.creepSpawned?.Invoke(health);

        moveResetTimerThresold = Random.Range(50, 90);
        moveTimer = moveResetTimerThresold;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        moveTimer += Time.deltaTime;

        if (moveTimer > moveResetTimerThresold)
        {
            var pos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * 5;
            agent.SetDestination(pos + transform.position);
            moveTimer = 0;
        }
    }

    private void Anniged(ulong annigilatorID)
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventsHolder.creepAnniged?.Invoke(health);
    }
}