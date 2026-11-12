using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;
using TMPro;
using static Settings;

public class UI : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] Button btnCam;
    [SerializeField] Transform healthbarParent;
    [SerializeField] ProgressBar progressBarPrefab;
    [SerializeField] ProgressBar citadelHealthBarPrefab;
    [SerializeField] ProgressBar towerHealthBarPrefab;
    [SerializeField] ProgressBar perkProgressBar;
    [SerializeField] ProgressBar creepochekBarPrefab;
    [SerializeField] UpgradePropertyUI propertyPrefab;
    [SerializeField] StageUI stagePrefab;
    [SerializeField] RespawnTimerUI respawnTimer;
    [SerializeField] GameObject infoRuneSpawned;
    [SerializeField] GameObject runeEffect;
    [SerializeField] TMP_Text runeTimeLeft;
    [SerializeField] Transform liderboard;
    [SerializeField] TMP_Text entryLiderboardPrefab;
    [SerializeField] TMP_Text labelMaxScore;
    [SerializeField] TMP_Text labelCountUpgrades;
    [SerializeField] TMP_Text labelLvl;
    [SerializeField] ProgressBar expBar;

    [Space]

    [SerializeField] PerkHandlerUI perkHandler;
    [SerializeField] CompleteUI complete;
    [SerializeField] GameObject panelUpgrades;
    [SerializeField] GameObject panelNextStage;

    [Space]
    [SerializeField] Perk[] properties;

    readonly List<BarHealthPair> healthBars = new();
    readonly List<BarPlayerPair> barsPlayers = new();
    List<LiderboardEntry> playersEntryes = new();

    public Action<Vector2> onJoystick;
    public Action<PerkID> onPerkClick;

    //bool isSleep;
    int maxScore;

    Player playerMine;
    // HOT FIX
    CompleteStatus status;

    private void Awake()
    {
        //isSleep = true;

        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
        EventsHolder.playerSpawnedAny.AddListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.AddListener(Player_Anniged);
        EventsHolder.onCrepochekDamage.AddListener(Crepochek_Damaged);
        EventsHolder.profileSeted.AddListener(Profile_Seted);
        EventsHolder.creepAnniged.AddListener(Creepochek_Anniged);
        //EventsHolder.runeSpawned.AddListener(Rune_Spawned);
        //EventsHolder.runeTaked.AddListener(Rune_Taked);
        //EventsHolder.runeEnded.AddListener(Rune_Ended);

        //btnCam.onClick.AddListener(Cam_Clicked);

        //perkHandler.onPerkClick += Perk_Clicked;
        //respawnTimer.onTimerNotify += RespawnTimer;

        if (MultiplayerManager.IsMaster)
            EventsHolder.clientCompleteDataReceived.AddListener(ClientData_Received);
    }

    private void Start()
    {
        foreach (var item in properties)
        {
            var p = Instantiate(propertyPrefab, panelUpgrades.transform);
            p.Init(item);
            p.onPropertyCliked += UpgradeProperty_Clicked;
        }

        panelUpgrades.GetComponent<CanvasGroup>().alpha = 0;
        panelUpgrades.SetActive(false);

        panelNextStage.SetActive(false);
    }

    private void Creepochek_Anniged(HealthComponent health)
    {
        var pair = healthBars.Find(p => p.health == health);
        if (pair != null)
        {
            Destroy(pair.bar.gameObject);
            healthBars.Remove(pair);
        }
    }

    private void Crepochek_Damaged(HealthComponent health)
    {
        var pair = healthBars.Find(p => p.health == health);
        if (pair == null)
        {
            var bar = Instantiate(creepochekBarPrefab, healthbarParent);
            healthBars.Add(new() { health = health, bar = bar, canvas = bar.GetComponent<CanvasGroup>() });
        }
    }

    private void ClientData_Received()
    {
        LeanTween.delayedCall(1f, () =>
        {
            if (complete)
            {
                complete.gameObject.SetActive(true);
                complete.Init(status, perkHandler.AllPerks);
            }
        });
    }

    private void RespawnTimer()
    {
        if (GameManager.Instance.complete)
            return;

        respawnTimer.gameObject.SetActive(false);
        //joystick.enabled = true;
        joystick.gameObject.SetActive(true);
        joystick.Reset();

        MultiplayerManager.RespawnPlayer();
    }

    private void Profile_Seted(AIProfile profile, Player player)
    {
        //if (playersHealthBars.ContainsKey(player))
        //{
        //    var bar = playersHealthBars[player];
        //    (bar as PlayerHealthBar).nickname.text = profile.name;
        //}
    }

    private void Victory(Team team)
    {
        GameComplete(CompleteStatus.Victory);
    }

    private void Defeat(Team team)
    {
        GameComplete(CompleteStatus.Defeat);
    }

    private void GameComplete(CompleteStatus status)
    {
        if (!MultiplayerManager.IsMaster)
            return;

        this.status = status;

        respawnTimer.gameObject.SetActive(false);
        joystick.gameObject.SetActive(false);
        perkHandler.Clear();

        if (PhotonNetwork.CurrentRoom.Players.Count - 1 == 0)
        {
            LeanTween.delayedCall(1f, () =>
            {
                if (complete)
                {
                    complete.gameObject.SetActive(true);
                    complete.Init(status, perkHandler.AllPerks);
                }
            });
        }
    }

    public void NetworkGameComplete(CompleteStatus status)
    {
        respawnTimer.gameObject.SetActive(false);
        joystick.gameObject.SetActive(false);
        perkHandler.Clear();

        complete.gameObject.SetActive(true);
        complete.Init(status, perkHandler.AllPerks);

    }

    private void Perk_Clicked(PerkID perkID)
    {
        ClosePerkPanel();

        if (playerMine)
        {
            onPerkClick?.Invoke(perkID);
        }
        else
        {
            var mineRespawnData = GameManager.Instance.respawnQueue.Find
            (
                r =>
                !r.isAI &&
                int.Parse($"{r.viewID}".Substring(0, 1)) == PhotonNetwork.LocalPlayer.ActorNumber
            );
            mineRespawnData.usedPerks.Add(perkID);
        }
    }

    

    private void UpgradeProperty_Clicked(PerkID perkID)
    {
        playerMine.UpgradeProperty(perkID);

        if(playerMine.CountAvailableUpgrades <= 0)
        {
            var canvas = panelUpgrades.GetComponent<CanvasGroup>();
            canvas.interactable = false;
            LeanTween.value(gameObject, a => canvas.alpha = a, 1, 0, 0.5f).setEaseInOutQuad();
        }

    }

    private void Player_Anniged(Player player)
    {
        var pair = barsPlayers.Find(p => p.player == player);
        if (pair != null)
        {
            Destroy(pair.bar.gameObject);
            barsPlayers.Remove(pair);
        }

        var entry = playersEntryes.Find(p => p.player == player);
        if (entry != null)
        {
            Destroy(entry.entry.gameObject);
            playersEntryes.Remove(entry);
        }


        if (player.GetComponent<BehaviourPlayer>() && player.IsMine() && !GameManager.Instance.complete)
        {
            //joystick.enabled = false;
            joystick.gameObject.SetActive(false);
            joystick.Reset();

            //respawnTimer.gameObject.SetActive(true);
            //respawnTimer.Init();
        }
    }

    private void PlayerAny_Spawned(Player player)
    {
        var bar = Instantiate(progressBarPrefab, healthbarParent);
        barsPlayers.Add(new() { bar = bar, player = player, canvas = bar.GetComponent<CanvasGroup>() });
        (bar as PlayerHealthBar).nickname.text = player.Nickname;
        
        var entry = Instantiate(entryLiderboardPrefab, liderboard);
        playersEntryes.Add(new() { player = player, entry = entry });
        
    }

    private void PlayerMine_Spawned(Player player)
    {
        player.GetComponent<BehaviourPlayer>().SetUI(this);

        playerMine = player;
        playerMine.onUpgradeAvailable += Upgrade_Availabled;
        playerMine.onNextStageAvailable += NextStage_Availabled;
    }

    private void NextStage_Availabled()
    {
        panelNextStage.SetActive(true);

        foreach (Transform item in panelNextStage.transform)
        {
            Destroy(item.gameObject);
        }

        var types = playerMine.CurrentType.GetAvailableNextTypes();
        foreach (var item in types)
        {
            var stage = Instantiate(stagePrefab, panelNextStage.transform);
            stage.Init(item);
            stage.onStageCliked += Stage_Clicked;
        }
    }

    private void Stage_Clicked(TankType tankType)
    {
        playerMine.InitType(tankType);

        panelNextStage.SetActive(false);
    }

    private void Upgrade_Availabled()
    {
        panelUpgrades.SetActive(true);

        var canvas = panelUpgrades.GetComponent<CanvasGroup>();
        canvas.interactable = true;
        LeanTween.value(gameObject, a => canvas.alpha = a, 0, 1, 0.5f).setEaseInOutQuad();

    }

    private void Rune_Spawned(Rune rune)
    {
        infoRuneSpawned.SetActive(true);
    }

    private void Rune_Taked(Player player)
    {
        infoRuneSpawned.SetActive(false);

        if (player.Nickname == PhotonNetwork.NickName)
        {
            runeEffect.SetActive(true);
        }
    }

    private void Rune_Ended(Player player)
    {
        runeEffect.SetActive(false);
    }

    private void Update()
    {
        UpdateLvlBar();
        UpdateLiderboard();
        UpdateHealthBars();

        //if(joystick.Direction != Vector2.zero)
        //{
        //    isSleep = false;
        //    onJoystick?.Invoke(joystick.Direction);
        //}
        //else if(!isSleep)
        //{
        //    isSleep = true;
        //    onJoystick?.Invoke(joystick.Direction);
        //}

        labelCountUpgrades.text = $"x{playerMine.CountAvailableUpgrades}";

        if (GameManager.Instance.Rune)
        {
            runeTimeLeft.text = $"{RUNE_DURATION - GameManager.Instance.Rune.currentDuration:F0}";
        }
    }

    void UpdateLvlBar()
    {
        labelLvl.text = $"{playerMine.Lvl} Óð.";

        float thresold = 10 + Mathf.Pow(playerMine.Lvl, 1.3f);
        expBar.value = playerMine.Score / thresold;
    }

    void UpdateLiderboard()
    {
        foreach (var item in playersEntryes)
        {
            item.entry.text = $"{item.player.ScoreTotal} Lvl.{item.player.Lvl} {item.player.Nickname}";

            if (item.player.ScoreTotal >= maxScore)
            {
                maxScore = item.player.ScoreTotal;
                labelMaxScore.text = maxScore.ToString();
            }
        }

        playersEntryes = playersEntryes.OrderByDescending(i => i.player.ScoreTotal).ToList();

        for (int i = 0; i < playersEntryes.Count; i++)
        {
            playersEntryes[i].entry.transform.SetSiblingIndex(i);
        }
    }

    public void ShowPerkPanel(List<PerkID> usedPerks)
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.1f);

            var rect = perkProgressBar.GetComponent<RectTransform>();
            LeanTween.value
            (
                gameObject,
                y => 
                { 
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y); 
                },
                0,
                30,
                0.3f
            ).setEaseOutQuad();

            perkHandler.Init(usedPerks);
        }
    }

    public void ClosePerkPanel()
    {
        perkHandler.Clear();
        var rect = perkProgressBar.GetComponent<RectTransform>();
        LeanTween.value
        (
            gameObject,
            y =>
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            },
            30,
            0,
            0.3f
        ).setEaseOutQuad();
    }

    void UpdateHealthBars()
    {
        int yOffset = (int)((float)Screen.height / 11f / transform.lossyScale.x);
        foreach (var item in barsPlayers)
        {
            var screenPos = Camera.main.WorldToScreenPoint(item.player.transform.position);

            if (screenPos.x < 0 || screenPos.x > Screen.width 
                || screenPos.y < 0 || screenPos.y > Screen.height)
            {
                item.canvas.alpha = 0;
                continue;
            }
            else
            {
                item.canvas.alpha = 1;
                var pos = screenPos / transform.lossyScale.x;
                (item.bar.transform as RectTransform).anchoredPosition = pos - new Vector3(0, yOffset, 0);
            }

            var value = (float)item.player.Health.Value / (float)item.player.Health.MaxValue;
            item.bar.value = value;
        }


        foreach (var item in healthBars)
        {
            var screenPos = Camera.main.WorldToScreenPoint(item.health.transform.position);

            if (screenPos.x < 0 || screenPos.x > Screen.width
                || screenPos.y < 0 || screenPos.y > Screen.height)
            {
                item.canvas.alpha = 0;
                continue;
            }
            else
            {
                item.canvas.alpha = 1;
                var pos = screenPos / transform.lossyScale.x;
                (item.bar.transform as RectTransform).anchoredPosition = pos - new Vector3(0, yOffset, 0);
            }

            var value = (float)item.health.Value / (float)item.health.MaxValue;
            item.bar.value = value;
        }

    }

    void UpdatePerkBar()
    {
        if (!playerMine)
            return;

        var curProgress = playerMine.PerkProgress.Evaluate(playerMine.usedPerks.Count);

        perkProgressBar.value = playerMine.Score / curProgress;
    }

    private void Cam_Clicked()
    {
        EventsHolder.onCamClicked?.Invoke();
    }

    class BarPlayerPair
    {
        public ProgressBar bar;
        public Player player;
        public CanvasGroup canvas;
    }

    class BarHealthPair
    {
        public ProgressBar bar;
        public HealthComponent health;
        public CanvasGroup canvas;
    }

    class LiderboardEntry
    {
        public Player player;
        public TMP_Text entry;
    }
}
