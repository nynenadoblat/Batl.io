using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using static Settings;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] bool spawnCreesp;

    [SerializeField] MultiplayerManager multiplayer;
    [SerializeField] Creep creepPrefab;
    [SerializeField] Rune runePrefab;
    [SerializeField] GameObject xeroboro_1_Prefab;
    [SerializeField] GameObject xeroboro_2_Prefab;
    [SerializeField] GameObject xeroboro_3_Prefab;
    [SerializeField] GameObject xeroboro_4_Prefab;
    [SerializeField] GameObject xeroboro_5_Prefab;

    [SerializeField] public int spawnPointRange = 3;
    [SerializeField] public Transform[] spawnCreepPoints;
    [SerializeField] public Transform[] runePoints;
    [SerializeField] public Citadel[] citadels;
    [SerializeField] public Tower[] towers;

    public static GameManager Instance { get; private set; }

    public Rune Rune { get; private set; }
    
    public List<Player> allPlayers = new();
    public List<HealthComponent> allCreeps = new();
    public List<RespawnData> respawnQueue = new();

    public bool complete;

    private Team tima;
    public Team MineTeam 
    {
        get => tima;
        set
        {
            tima = value;
            //print("��� ����");
        } 
    }

    private void Awake()
    {
        Instance = this;

        multiplayer.onPlayerSpawned += Player_Spawned;

        EventsHolder.playerSpawnedAny.AddListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.AddListener(Player_Anniged);
        EventsHolder.creepSpawned.AddListener(Creep_Spawned);
        EventsHolder.creepAnniged.AddListener(Creep_Anniged);
        EventsHolder.mineTeamPicked.AddListener(MineTeam_Picked);
        EventsHolder.runeSpawned.AddListener(Rune_Spawned);
        EventsHolder.runeEnded.AddListener(Rune_Ended);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //towers.ToList().ForEach(t => t.Health.onAnniged += Tower_Anniged);
        //citadels.ToList().ForEach(t => t.Health.onAnniged += Citadel_Anniged);
    }

    private void Start()
    {
        StartCoroutine(XeroboroSpawner());
        StartCoroutine(CheckRespawnQueue());
        //StartCoroutine(SpawnRune());
    }

    private void Citadel_Anniged(int timaAnnigerID)
    {
        complete = true;
        print("���������� ����������");
        allPlayers.RemoveAll(p => !p);

        if (!MultiplayerManager.IsMaster)
        {
            EventsHolder.clientGameCompleted?.Invoke();
            return;
        }

        // Hot Fix
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.3f);

            var citadel = citadels.ToList().Find(c => c);

            if (citadel.Health.Team == MineTeam)
            {
                EventsHolder.onVictory?.Invoke(MineTeam);
            }
            else
            {
                EventsHolder.onDefeat?.Invoke(MineTeam);
            }
        }
    }

    private void MineTeam_Picked(Team team)
    {
        MineTeam = team;
    }

    

    public Vector3 GetSpawnPos()
    {
        var size = spawnPointRange;
        return new Vector3(Random.Range(-size, size), 0, Random.Range(-size, size));
    }

    private IEnumerator CheckRespawnQueue()
    {
        while (true && MultiplayerManager.IsMaster && !complete)
        {
            yield return new WaitForSeconds(0.7f);

            if(respawnQueue.Count > 0 && !complete) 
            {
                var respawnData = respawnQueue.Find(r => Time.time - r.timeAnniged > 10 && r.isAI);
                if (respawnData != null)
                {
                    MultiplayerManager.Instance.SpawnAI(respawnData);

                    respawnQueue.Remove(respawnData);
                }
            }
        }
    }

    private void Tower_Anniged(int annigilatorID)
    {
        print("����� �����");
    }

    private void Creep_Anniged(HealthComponent creep)
    {
        allCreeps.Remove(creep);

        if (!xeroboroSpawnInProgress)
        {
            StartCoroutine(XeroboroSpawner());
        }
    }

    private void Creep_Spawned(HealthComponent creep)
    {
        allCreeps.Add(creep);
    }

    bool xeroboroSpawnInProgress;

    IEnumerator XeroboroSpawner()
    {
        xeroboroSpawnInProgress = true;

        yield return new WaitForSeconds(1f);

        while (spawnCreesp && allCreeps.Count < MAX_CREEPS)
        {
            yield return new WaitForSeconds(0.1f);

            if (!MultiplayerManager.IsMaster)
                continue;

            var randomValue = Random.Range(0, 100);
            if(randomValue < 3)
            {
                MultiplayerManager.Spawn(xeroboro_5_Prefab, GetSpawnPos());
            }
            else if(randomValue < 8)
            {
                MultiplayerManager.Spawn(xeroboro_4_Prefab, GetSpawnPos());
            }
            else if (randomValue < 17)
            {
                MultiplayerManager.Spawn(xeroboro_2_Prefab, GetSpawnPos());
            }
            else if(randomValue < 25)
            {
                MultiplayerManager.Spawn(xeroboro_3_Prefab, GetSpawnPos());
            }
            else
            {
                MultiplayerManager.Spawn(xeroboro_1_Prefab, GetSpawnPos());
            }

        }

        xeroboroSpawnInProgress = false;
    }

    IEnumerator CreepSpawner()
    {
        yield return new WaitForSeconds(7f);

        while (!complete && spawnCreesp && true)
        {
            yield return new WaitForSeconds(3f);

            if (!MultiplayerManager.IsMaster)
                continue;

            for (int i = 0; i < 4; i++)
            {
                int teamIdx = i % 2;

                var pos = spawnCreepPoints[teamIdx].position;
                pos.x += Random.value;
                pos.z += Random.value; 
                var c = MultiplayerManager.Spawn(creepPrefab, pos);
                c.Health.Team = (Team)teamIdx;
                c.Team_Seted((Team)teamIdx);
            }

            
        }
    }

    private void Player_Anniged(Player player)
    {
        StartCoroutine(Delay());
        
        IEnumerator Delay()
        {
            RespawnData respawn = new()
            {
                name = player.Nickname,
                isAI = player.GetComponent<BehaviourAI>(),
                viewID = player.GetComponent<NetworkPlayer>().ViewID,
                team = player.Health.Team,
                score = player.Score,
                countAnniges = player.CountAnniges,
                countAnniged = player.CountAnniged,
                usedPerks = player.usedPerks,

                timeAnniged = Time.time,
            };

            
            print("������� <color=#83C12B>" + player.Nickname + "</color> " + player.Health.Value);
            //allPlayers.Remove(player);

            bool isMaster = MultiplayerManager.IsMaster;
            bool isMine = !respawn.isAI && player.GetComponent<NetworkPlayer>().IsMine();

            if (isMaster || isMine)
            {
                respawnQueue.Add(respawn);
            }

            yield return null;

            allPlayers.RemoveAll(p => !p);
        }
    }

    private void PlayerAny_Spawned(Player player)
    {
        //print(player.Nickname + " Spawned");
        allPlayers.Add(player.GetComponent<Player>());
        respawnQueue.Remove(respawnQueue.Find(p => p.name == player.Nickname));
    }

    private void Player_Spawned(GameObject player)
    {
        var p = player.GetComponent<Player>();
        EventsHolder.playerSpawnedMine?.Invoke(p);
    }

    public HealthComponent FindTarget(HealthComponent health)
    {
        var targets = GetTargets(health);

        float minDistance = float.MaxValue;
        HealthComponent potentialTarget = default;

        foreach (var target in targets)
        {
            var distance = Vector3.Distance(health.transform.position, target.transform.position);
            if (distance < minDistance)
            {
                potentialTarget = target;
                minDistance = distance;
            }
        }

        return potentialTarget;
    }

    public List<HealthComponent> GetTargets(HealthComponent health)
    {
        if (complete)
            return new();

        var targets = allPlayers.Where(p => p).Select(p => p.Health).ToList();
        //targets.AddRange(citadels.Where(c => c).Select(c => c.Health));
        targets.AddRange(allCreeps.Where(t => t));
        //targets.AddRange(towers.Where(t => t).Select(t => t.Health));

        return targets.Where(t => t != health).ToList();
    }

    IEnumerator SpawnRune()
    {
        //int delay = Random.Range(MIN_RUNE_SPAWN_DELAY, MAX_RUNE_SPAWN_DELAY);
        yield return new WaitForSeconds(1);

        if (MultiplayerManager.IsMaster)
        {
            var pos = runePoints[Random.Range(0, runePoints.Length)].position;

            Rune = MultiplayerManager.Spawn(runePrefab, pos);
        }
    }

    private void Rune_Spawned(Rune rune)
    {
        if (Rune == null)
            Rune = rune;
    }

    private void Rune_Ended(Player player)
    {
        StartCoroutine(SpawnRune());
    }
}

[Serializable]
public class RespawnData
{
    public string name;
    public Team team;
    public ulong viewID;
    public bool isAI;
    public int countAnniges;
    public int countAnniged;
    public int score;
    public List<PerkID> usedPerks = new();

    public float timeAnniged;

    //public int baseDamage;
    //public int critChance;
    //public int maxHealth;
    //public int evasion;
    //public float rateIfFire;
}

public enum CompleteStatus
{
    Victory,
    Defeat
}