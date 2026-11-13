using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using System;

public class Player : NetworkBehaviour
{
    [field: SerializeField]
    public int ScoreTotal { get; set; }
    [field: SerializeField]
    public int Score { get; set; }
    [field: SerializeField]
    public int Lvl { get; set; }
    [field: SerializeField]
    public int Stage { get; set; } = 0;
    [field: SerializeField]
    public TankType CurrentType { get; set; }

    [Space]

    [SerializeField] AnimationCurve perkProgress;
    [SerializeField] AnimationCurve curveStages;
    [SerializeField] float speed = 7f;
    [SerializeField] public float rateOfFire = 0.5f;
    [SerializeField] float distToAtttack = 5f;
    [SerializeField] HealthComponent health;
    [SerializeField] TMPro.TMP_Text testo;
    [SerializeField] ParticleSystem runeEffect;
    [SerializeField] GameObject fireEffectFriendlyPrefab;
    [SerializeField] GameObject fireEffectEnemyPrefab;

    [Space]

    [SerializeField] MeshRenderer teamIndicator;
    [SerializeField] Material friendly;
    [SerializeField] Material enemy;

    [Header("�������� �����")]
    [SerializeField] TankTypeSet[] typesSets;


    [field: SerializeField]
    public string Nickname { get; set; }

    public Action<List<PerkID>> onPerkAvailable;
    public Action<PerkID> onPerkPick;
    public Action<PerkID> onPropertyUpgraded;
    public Action onUpgradeAvailable;
    public Action onNextStageAvailable;

    public List<PerkID> usedPerks = new();
    public List<PerkID> usedProperties = new();

    public int CountAnniges { get; set; }
    [field: SerializeField]
    public int CountAnniged { get; set; }
    public float DistanceToAttack => distToAtttack;
    public HealthComponent Health => health;
    public AnimationCurve PerkProgress => perkProgress;
    public int CountAvailableUpgrades => countAvailableUpgrades;

    [field: SerializeField]
    public int CritChance { get; set; } = 5;
    [field: SerializeField]
    public float Precision { get; set; } = 7f;
    [field: SerializeField]
    public int SpeedProjectile { get; set; } = 11;

    public int baseDamage = 1;

    MuzzleHandler muzzleHandler;

    HealthComponent target;
    float currentRate;
    float findTargetTimer;
    float baseFireRate;
    bool annigedNotifed;
    bool isRuneUsed;
    int countAvailableUpgrades;
    
    private void Start()
    {
        baseFireRate = rateOfFire;
        
        EventsHolder.playerSpawnedAny?.Invoke(this);

        name += NetworkObjectId;

        Health.onAnniged += Anniged;
        Health.onTeamSet += Team_Seted;
        Health.OwnerID = NetworkObjectId;

        EventsHolder.playerAnniged.AddListener(OtherPlayer_Anniged);
        EventsHolder.creepAnniged.AddListener(Creepochek_Anniged);

        InitType(TankType.Simple);
    }

    public void InitType(TankType tankType, bool isRemoteSync = false)
    {
        foreach (var item in typesSets)
        {
            item.handler.gameObject.SetActive(false);
        }

        var set = typesSets.ToList().Find(t => t.tankType == tankType);
        if (set != null && set.handler != null)
        {
            muzzleHandler = set.handler;
            muzzleHandler.gameObject.SetActive(true);
            muzzleHandler.Init(this);
        }

        CurrentType = tankType;

        if (!isRemoteSync)
        {
            Stage++;

            health.HealValue += 10;
            speed -= 0.5f;

            var netPlayer = GetComponent<NetworkPlayer>();
            if (netPlayer != null && IsOwner)
            {
                netPlayer.SetTankTypeServerRpc((int)tankType, Stage);
            }
        }
    }

    public void AddScore(int amount)
    {
        ScoreTotal += amount;
        Score += amount;
        var netPlayer = GetComponent<NetworkPlayer>();
        if (netPlayer != null && IsOwner)
        {
            netPlayer.SyncScoreServerRpc(Score, ScoreTotal);
        }

        CheckAvailableUpgrade();
        CheckAvailableNextStage();
    }

    private void Creepochek_Anniged(HealthComponent health)
    {
        var isMeAnnigilator = NetworkObjectId == health.LastDamagedOwnerID;

        if (isMeAnnigilator && IsOwner)
        {
            AddScore(health.GetComponent<Creepochek>().score);
        }
    }

    public void Team_Seted(Team team)
    {
        //print($"���� ������� {team}");
        if(GameManager.Instance.MineTeam == team)
        {
            teamIndicator.material = friendly;
        }
        else
        {
            teamIndicator.material = enemy;
        }
    }

    private void OtherPlayer_Anniged(Player annigedPlayer)
    {
        var isMeAnnigilator = NetworkObjectId == annigedPlayer.Health.LastDamagedOwnerID;
        
        if (isMeAnnigilator && IsOwner)
        {
            CountAnniges++;
            var netPlayer = GetComponent<NetworkPlayer>();
            if (netPlayer != null)
            {
                netPlayer.SyncKillsServerRpc(CountAnniges);
            }

            AddScore((int)((float)annigedPlayer.ScoreTotal / 1.3f));
        }
    }

    void CheckAvailableUpgrade()
    {
        if (Lvl >= Settings.MAX_UPGRADE_LVL)
            return;

        var thresold = 10 + Mathf.Pow(Lvl, 1.3f);
        if(Score >= thresold)
        {
            Score -= (int)thresold;
            Lvl++;

            health.IncreaseMaxHealth(10);

            transform.localScale = Vector3.one * (1f + (Lvl - 1) * 0.08f);

            countAvailableUpgrades++;

            var netPlayer = GetComponent<NetworkPlayer>();
            if (netPlayer != null && IsOwner)
            {
                netPlayer.SyncLvlServerRpc(Lvl, Score, ScoreTotal);
            }

            onUpgradeAvailable?.Invoke();

            CheckAvailableUpgrade();
            CheckAvailableNextStage();
        }
    }

    void CheckAvailableNextStage()
    {
        if (Stage >= Settings.MAX_STAGE)
            return;

        var lvlThesold = Mathf.FloorToInt(curveStages.Evaluate(Stage));
        if(Lvl >= lvlThesold)
        {
            onNextStageAvailable?.Invoke();
        }
    }

    void CheckAvailablePerk()
    {
        var thresold = perkProgress.Evaluate(usedPerks.Count);

        if(Score >= thresold && !GameManager.Instance.complete && usedPerks.Count < Settings.MAX_PERKS)
        {
            Score = 0;

            onPerkAvailable?.Invoke(usedPerks);
        }
    }

    public void PickPerk(PerkID perk, bool silentMode = false)
    {
        switch (perk)
        {
            case PerkID.Attack:
                baseDamage += 3;
                break;
            case PerkID.AttackPower:
                baseDamage += 5;
                break;
            case PerkID.Health:
                Health.IncreaseMaxHealth(50);
                break;
            case PerkID.FireRate:
                if (!isRuneUsed)
                {
                    rateOfFire -= 0.08f;
                    rateOfFire = Mathf.Clamp(rateOfFire, 0.03f, 1f);
                }
                break;
            case PerkID.Evasion:
                Health.Evasion += 30;
                break;
            case PerkID.CritChance:
                CritChance += 10;
                break;
            case PerkID.CritChancePower:
                CritChance += 30;
                break;
            case PerkID.Armor:
                Health.Armor += 3;
                break;
            default:
                break;
        }

        usedPerks.Add(perk);
       
        if (!silentMode)
            onPerkPick?.Invoke(perk);

        //print($"�� ����� ������ {perk}");
    }

    public void UpgradeProperty(PerkID perk, bool silentMode = false)
    {
        //print(Nickname + " �������� " + perk + " ==========================");
        switch (perk)
        {
            case PerkID.Attack:
                baseDamage += 3;
                break;
            
            case PerkID.Health:
                Health.IncreaseMaxHealth(30);
                break;

            case PerkID.FireRate:
                if (!isRuneUsed)
                {
                    rateOfFire -= 0.038f;
                    rateOfFire = Mathf.Clamp(rateOfFire, 0.03f, 1f);
                }
                break;

            case PerkID.Evasion:
                Health.Evasion += 5;
                break;

            case PerkID.CritChance:
                CritChance += 5;
                break;

            case PerkID.SpeedProjectile:
                SpeedProjectile += 1;
                break;

            case PerkID.Precision:
                Precision -= 0.57f;
                break;
        }

        usedProperties.Add(perk);

        countAvailableUpgrades--;

        if (!silentMode)
            onPropertyUpgraded?.Invoke(perk);
    }

    public void TakeRune()
    {
        rateOfFire = 0.087f;
        baseDamage += 5;
        speed += 3f;
        distToAtttack += 3f;
        health.IncreaseMaxHealth(1300);

        runeEffect.Play();

        isRuneUsed = true;
    }

    public void RuneEnded()
    {
        rateOfFire = baseFireRate;
        foreach (var ratePerk in usedPerks.FindAll(p => p == PerkID.FireRate))
        {
            rateOfFire -= 0.08f;
        } 

        rateOfFire = Mathf.Clamp(rateOfFire, 0.03f, 1f);
        baseDamage -= 5;
        speed -= 3f;
        distToAtttack -= 3f;
        health.DecreaseMaxHealth(1300);

        runeEffect.Stop();

        isRuneUsed = false;
    }

    private void Anniged(ulong annigilatorID)
    {
        CountAnniged++;
        var netPlayer = GetComponent<NetworkPlayer>();
        if (netPlayer != null)
        {
            netPlayer.SyncDeathsServerRpc(CountAnniged);
        }

        EventsHolder.playerAnniged?.Invoke(this);
        annigedNotifed = true;

        if (IsOwner)
        {
            if (IsServer) { NetworkObject.Despawn(true); } else { RequestDespawnServerRpc(); }
        }
    }

    public void OnFired(Vector3[] positions, Vector3[] directions, int speed, int damage, int critChance)
    {
        var netPlayer = GetComponent<NetworkPlayer>();
        if (netPlayer != null)
        {
            netPlayer.BroadcastFire(positions, directions, speed, damage, critChance);
        }
    }

    public void SpawnRemoteProjectiles(Vector3[] positions, Vector3[] directions, int speed, int damage, int critChance)
    {
        if (muzzleHandler != null)
        {
            muzzleHandler.FireRemote(positions, directions, speed, damage, critChance);
        }
        else
        {
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                var dir = directions[i];
                var projectile = ProjectilePool.Get(pos);
                var projectileData = new ProjectileData
                {
                    baseDamege = damage,
                    critChance = critChance,
                    speed = speed,
                    ownerID = NetworkObjectId,
                    team = Health.Team,
                    direction = dir,
                    isCosmetic = true,
                    mat = (GameManager.Instance != null && GameManager.Instance.MineTeam == Health.Team) ? friendly : enemy
                };
                projectile.Init(projectileData);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestDespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    public void Move(Vector2 value)
    {
        transform.position += speed * Time.deltaTime * new Vector3(value.x, 0, value.y);
    }

    private void Update()
    {
        currentRate += Time.deltaTime;
        findTargetTimer += Time.deltaTime;

        //var rot = testo.transform.rotation.eulerAngles;
        //testo.transform.rotation = Quaternion.Euler(rot.x, 0, rot.z);

        //CheckNearestTarget();

        //if(usedPerks.Count > 0)
        //{
        //    testo.text = usedPerks.Count.ToString();
        //}
        //else
        //{
        //    testo.text = string.Empty;
        //}

        
    }

    public void Fire()
    {
        muzzleHandler.Fire();        
    }

    void FindTarget()
    {
        var targets = GameManager.Instance.allPlayers.Select(p => p.health).ToList();
        //var citadel = GameManager.Instance.citadels.ToList().Find(c => c.Health.Team != Health.Team);
        //var tower = GameManager.Instance.towers.ToList().Find(c => c.Health.Team != Health.Team);
        //targets.Add(citadel.Health);
        //targets.AddRange(GameManager.Instance.allCreeps.Select(c => c.Health));
        //targets.Add(tower.Health);

        float minDist = float.MaxValue;
        HealthComponent potentialTarget = default;
        
        foreach (var h in targets)
        {
            if (health == h)
                continue;
            
            if (h.Team == Health.Team)
                continue;

            //HOT FIX
            if (!h)
                continue;

            var dist = Vector3.Distance(h.transform.position, transform.position);
            
            if (dist > distToAtttack)
                continue;

            if (dist < minDist)
            {
                potentialTarget = h;
                minDist = dist;
            }
            
        }
        
        target = potentialTarget;

    }

    public void SetNickname(string value)
    {
        Nickname = value;
        //onSetNickname?.Invoke
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        //print("���������, ����.. " + Nickname);
        if (!IsOwner && !annigedNotifed && Time.time > 7)
        {
            EventsHolder.playerAnniged?.Invoke(this);
        }
    }

    void CheckNearestTarget()
    {
        if(findTargetTimer > 1f)
        {
            FindTarget();

            findTargetTimer = 0;
        }
    }
}

public enum Team
{
    One = 0, 
    Two = 1,
}
