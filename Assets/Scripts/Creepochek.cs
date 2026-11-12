using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Creepochek : MonoBehaviourPun
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

    private void Damaged(int arg1, int arg2)
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
        if (!MultiplayerManager.IsMaster)
            return;

        moveTimer += Time.deltaTime;

        if(moveTimer > moveResetTimerThresold)
        {
            var pos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * 5;
            agent.SetDestination(pos + transform.position);
            moveTimer = 0;
        }
    }

    private void Anniged(int annigilatorID)
    {
        if (MultiplayerManager.IsMaster)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        EventsHolder.creepAnniged?.Invoke(health);
    }

}
