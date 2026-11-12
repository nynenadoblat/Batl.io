using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using static Settings;

public class BehaviourAI : MonoBehaviour
{
    [SerializeField] List<Perk> allPerks;

    NavMeshAgent agent;
    Transform target;
    Player player;

    public System.Action<int> onProfileSeted;

    public int idxProfile;

    float timerRefrashTarget;

    private void Awake()
    {
        player = GetComponent<Player>();
        agent = GetComponent<NavMeshAgent>();

        player.onPerkAvailable += Perk_Available;
        player.onUpgradeAvailable += Upgrade_Available;
        player.onNextStageAvailable += NextStage_Availabled;
    }

    private void NextStage_Availabled()
    {
        int idxStage = 0;

        if(player.CurrentType == TankType.Simple)
        {
            idxStage = Random.Range(1, 4);
        }

        if(player.CurrentType == TankType.Twin)
        {
            idxStage = (int)TankType.TripleShot;
        }

        if (player.CurrentType == TankType.Trooper)
        {
            idxStage = (int)TankType.DualTrooper;
        }

        if (player.CurrentType == TankType.Guard)
        {
            idxStage = (int)TankType.QuadGuard;
        }
        //---------------------------------------
        if (player.CurrentType == TankType.QuadGuard)
        {
            idxStage = (int)TankType.Octo;
        }
        if (player.CurrentType == TankType.DualTrooper)
        {
            idxStage = (int)TankType.StormTrooper;
        }
        if (player.CurrentType == TankType.TripleShot)
        {
            idxStage = (int)TankType.Triplet;
        }

        player.InitType((TankType)idxStage);
    }

    private void Upgrade_Available()
    {
        if (!MultiplayerManager.IsMaster)
            return;

        bool perkFound = false;
        var perk = allPerks[Random.Range(0, allPerks.Count)];
        int countAttempt = 0;

        while (!perkFound)
        {
            var countSamePerk = player.usedPerks.FindAll(p => p == perk.id).Count;
            if (countSamePerk >= MAX_PERKS)
            {
                perk = allPerks[Random.Range(0, allPerks.Count)];
            }
            else
            {
                perkFound = true;
            }

            countAttempt++;
            if (countAttempt > 30)
                return;
        }

        player.UpgradeProperty(perk.id);
    }

    private void Perk_Available(List<PerkID> usedPerks)
    {
        if (!MultiplayerManager.IsMaster)
            return;

        bool perkFound = false;
        var perk = allPerks[Random.Range(0, allPerks.Count)];
        int countAttempt = 0;

        while (!perkFound)
        {
            var countSamePerk = usedPerks.FindAll(p => p == perk.id).Count;
            if (countSamePerk > perk.countStack)
            {
                perk = allPerks[Random.Range(0, allPerks.Count)];
            }
            else
            {
                perkFound = true;
            }

            countAttempt++;
            if (countAttempt > 10)
                return;
        }

        player.PickPerk(perk.id);
    }

    private void Update()
    {
        UpdateTarget();

        if (target && MultiplayerManager.IsMaster)
        {
            var dist = Vector3.Distance(transform.position, target.position);

            if (dist > agent.stoppingDistance * 0.95f)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.SetDestination(transform.position);
            }

            if (dist < player.DistanceToAttack)
            {
                var dir = target.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.001f)
                {
                    transform.forward = dir.normalized;
                }

                player.Fire();
            }
        }
        else
        {
            timerRefrashTarget = 5;
        }

    }

    void UpdateTarget()
    {
        timerRefrashTarget += Time.deltaTime;
        if (timerRefrashTarget > 3.5f)
        {
            target = GameManager.Instance.FindTarget(player.Health).transform;
            timerRefrashTarget = 0;
        }
    }

    public void SetProfile(AIProfile profile, int idx, bool silentMode = false)
    {
        if (!player || profile == null)
            return;

        player.Nickname = profile.name;

        idxProfile = idx;

        EventsHolder.profileSeted?.Invoke(profile, player);

        if (!silentMode)
            onProfileSeted?.Invoke(idx);
    }
    
}
