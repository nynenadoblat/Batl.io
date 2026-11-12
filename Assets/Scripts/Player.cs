using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using System;

public class Player : MonoBehaviourPun
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

    [Header("Ёволюци€ танка")]
    [SerializeField] TankTypeSet[] typesSets;


    [field: SerializeField]
    public string Nickname { get; set; }

    public Action<List<PerkID>> onPerkAvailable;
    public Action<PerkID> onPerkPick;
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

        name += photonView.ViewID;

        Health.onAnniged += Anniged;
        Health.onTeamSet += Team_Seted;
        Health.OwnerID = photonView.ViewID;

        EventsHolder.playerAnniged.AddListener(OtherPlayer_Anniged);
        EventsHolder.creepAnniged.AddListener(Creepochek_Anniged);

        InitType(TankType.Simple);
    }

    public void InitType(TankType tankType)
    {
        foreach (var item in typesSets)
        {
            item.handler.gameObject.SetActive(false);
        }

        muzzleHandler = typesSets.ToList().Find(t => t.tankType == tankType).handler;
        muzzleHandler.gameObject.SetActive(true);
        muzzleHandler.Init(this);

        CurrentType = tankType;

        Stage++;

        health.HealValue += 10;
        speed -= 0.5f;
    }

    private void Creepochek_Anniged(HealthComponent health)
    {
        var isMeAnnigilator = photonView.ViewID == health.LastDamagedOwnerID;

        if (isMeAnnigilator && photonView.IsMine)
        {
            ScoreTotal += health.GetComponent<Creepochek>().score;
            Score += health.GetComponent<Creepochek>().score;

            CheckAvailableUpgrade();
            CheckAvailableNextStage();
        }
    }

    public void Team_Seted(Team team)
    {
        //print($"тима выбрана {team}");
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
        var isMeAnnigilator = photonView.ViewID == annigedPlayer.Health.LastDamagedOwnerID;
        
        if (isMeAnnigilator && photonView.IsMine)
        {
            CountAnniges++;
            ScoreTotal += (int)((float)annigedPlayer.ScoreTotal / 1.3f);
            Score += (int)((float)annigedPlayer.ScoreTotal / 1.3f);

            CheckAvailableUpgrade();
            CheckAvailableNextStage();
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

            transform.localScale += Vector3.one * 0.08f;

            countAvailableUpgrades++;

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

        //print($"ща будем жарить {perk}");
    }

    public void UpgradeProperty(PerkID perk, bool silentMode = false)
    {
        //print(Nickname + " заебенил " + perk + " ==========================");
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
            onPerkPick?.Invoke(perk);
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

    private void Anniged(int annigilatorID)
    {
        CountAnniged++;
        EventsHolder.playerAnniged?.Invoke(this);
        annigedNotifed = true;

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView);
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

    private void OnDestroy()
    {
        //print("«аху€рили, суки.. " + Nickname);
        if (!photonView.IsMine && !annigedNotifed && Time.time > 7)
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
