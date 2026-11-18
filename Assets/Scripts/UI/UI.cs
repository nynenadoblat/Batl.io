using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using TMPro;
using static Settings;

public class UI : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] Joystick joystickAttack;
    [SerializeField] Button btnCam;

    public Vector2 MoveDirection => joystick != null ? joystick.Direction : Vector2.zero;
    public Vector2 AttackDirection => joystickAttack != null ? joystickAttack.Direction : Vector2.zero;
    public bool IsAttackingWithJoystick => joystickAttack != null && joystickAttack.Direction.sqrMagnitude > 0.04f;
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
    int maxScore = -1;

    Player playerMine;
    // HOT FIX
    CompleteStatus status;

    private Camera mainCamera;
    private Camera MainCamera
    {
        get
        {
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera;
        }
    }

    private float leaderboardTimer;
    private const float LEADERBOARD_INTERVAL = 0.2f;

    private int lastLvl = -1;
    private int lastUpgrades = -1;
    private float lastScore = -1f;
    private int lastRuneSeconds = -1;
    private float lastLossyScaleX;
    private int cachedYOffset;

    private static readonly Comparison<LiderboardEntry> s_LeaderboardComparer = (a, b) =>
    {
        int scoreA = a?.player != null ? a.player.ScoreTotal : 0;
        int scoreB = b?.player != null ? b.player.ScoreTotal : 0;
        return scoreB.CompareTo(scoreA);
    };

    private void Awake()
    {
        if (joystick == null || joystickAttack == null)
        {
            var foundJoysticks = GetComponentsInChildren<Joystick>(true);
            foreach (var j in foundJoysticks)
            {
                if (j.gameObject.name.Contains("Move") && joystick == null)
                {
                    joystick = j;
                }
                else if (j.gameObject.name.Contains("Attack") && joystickAttack == null)
                {
                    joystickAttack = j;
                }
            }
        }

        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
        EventsHolder.playerSpawnedAny.AddListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.AddListener(Player_Anniged);
        EventsHolder.onCrepochekDamage.AddListener(Crepochek_Damaged);
        EventsHolder.profileSeted.AddListener(Profile_Seted);
        EventsHolder.creepAnniged.AddListener(Creepochek_Anniged);
        EventsHolder.runeSpawned.AddListener(Rune_Spawned);
        EventsHolder.runeTaked.AddListener(Rune_Taked);
        EventsHolder.runeEnded.AddListener(Rune_Ended);

        EventsHolder.onVictory.AddListener(Victory);
        EventsHolder.onDefeat.AddListener(Defeat);
        EventsHolder.clientGameCompleted.AddListener(OnClientGameCompletedDefeat);

        if (respawnTimer != null)
        {
            respawnTimer.onTimerNotify += RespawnTimer;
        }

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

        if (labelMaxScore != null)
        {
            labelMaxScore.text = "0";
        }
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
        if (joystick != null)
        {
            joystick.gameObject.SetActive(true);
            joystick.Reset();
        }
        if (joystickAttack != null)
        {
            joystickAttack.gameObject.SetActive(true);
            joystickAttack.Reset();
        }

        MultiplayerManager.RespawnPlayer();
    }

    private void Profile_Seted(AIProfile profile, Player player)
    {
        var pair = barsPlayers.Find(p => p.player == player);
        if (pair != null)
        {
            (pair.bar as PlayerHealthBar).nickname.text = profile.name;
        }
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
        this.status = status;

        if (respawnTimer != null) respawnTimer.gameObject.SetActive(false);
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (joystickAttack != null) joystickAttack.gameObject.SetActive(false);
        if (perkHandler != null) perkHandler.Clear();

        LeanTween.delayedCall(1f, () =>
        {
            if (complete)
            {
                complete.gameObject.SetActive(true);
                complete.Init(status, perkHandler != null ? perkHandler.AllPerks : new());
            }
        });
    }

    public void NetworkGameComplete(CompleteStatus status)
    {
        respawnTimer.gameObject.SetActive(false);
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (joystickAttack != null) joystickAttack.gameObject.SetActive(false);
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
                r.name == LobbyManager.GetNickname()
            );
            mineRespawnData.usedPerks.Add(perkID);
        }
    }

    

    private void UpgradeProperty_Clicked(PerkID perkID)
    {
        if (playerMine == null) return;

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
            if (pair.bar != null) Destroy(pair.bar.gameObject);
            barsPlayers.Remove(pair);
        }

        var entry = playersEntryes.Find(p => p.player == player);
        if (entry != null)
        {
            if (entry.entry != null) Destroy(entry.entry.gameObject);
            playersEntryes.Remove(entry);
        }

        if (player.GetComponent<BehaviourPlayer>() && player.IsMine() && !GameManager.Instance.complete)
        {
            if (playerMine == player)
            {
                playerMine = null;
            }
            if (joystick != null)
            {
                joystick.gameObject.SetActive(false);
                joystick.Reset();
            }
            if (joystickAttack != null)
            {
                joystickAttack.gameObject.SetActive(false);
                joystickAttack.Reset();
            }
            if (respawnTimer != null)
            {
                respawnTimer.gameObject.SetActive(true);
                respawnTimer.Init();
            }
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

        if (joystick != null)
        {
            joystick.gameObject.SetActive(true);
            joystick.Reset();
        }
        if (joystickAttack != null)
        {
            joystickAttack.gameObject.SetActive(true);
            joystickAttack.Reset();
        }
        if (respawnTimer != null)
        {
            respawnTimer.gameObject.SetActive(false);
        }
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

        if (player.Nickname == LobbyManager.GetNickname())
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
        if (playerMine != null)
        {
            if (playerMine.Lvl != lastLvl)
            {
                lastLvl = playerMine.Lvl;
                labelLvl.text = Language.Rus ? $"{lastLvl} ур." : $"{lastLvl} lvl.";
            }

            if (playerMine.CountAvailableUpgrades != lastUpgrades)
            {
                lastUpgrades = playerMine.CountAvailableUpgrades;
                labelCountUpgrades.text = $"x{lastUpgrades}";
            }

            if (Mathf.Abs(playerMine.Score - lastScore) > 0.01f)
            {
                lastScore = playerMine.Score;
                float thresold = 10 + Mathf.Pow(playerMine.Lvl, 1.3f);
                expBar.value = playerMine.Score / thresold;
            }
        }

        UpdateLiderboard();
        UpdateHealthBars();

        if (GameManager.Instance != null && GameManager.Instance.Rune != null)
        {
            int remainingSeconds = Mathf.CeilToInt(RUNE_DURATION - GameManager.Instance.Rune.currentDuration);
            if (remainingSeconds != lastRuneSeconds)
            {
                lastRuneSeconds = remainingSeconds;
                runeTimeLeft.text = remainingSeconds.ToString();
            }
        }
    }

    void UpdateLvlBar()
    {
        if (playerMine == null) return;
        labelLvl.text = Language.Rus ? $"{playerMine.Lvl} ур." : $"{playerMine.Lvl} lvl.";

        float thresold = 10 + Mathf.Pow(playerMine.Lvl, 1.3f);
        expBar.value = playerMine.Score / thresold;
    }

    void UpdateLiderboard()
    {
        leaderboardTimer += Time.deltaTime;
        if (leaderboardTimer < LEADERBOARD_INTERVAL)
            return;
        leaderboardTimer = 0f;

        int currentMax = 0;
        for (int i = playersEntryes.Count - 1; i >= 0; i--)
        {
            var item = playersEntryes[i];
            if (item == null || item.player == null)
            {
                if (item?.entry != null) Destroy(item.entry.gameObject);
                playersEntryes.RemoveAt(i);
                continue;
            }

            int score = item.player.ScoreTotal;
            int lvl = item.player.Lvl;

            if (score != item.lastScore || lvl != item.lastLvl)
            {
                item.lastScore = score;
                item.lastLvl = lvl;
                item.entry.text = $"{score} Lvl.{lvl} {item.player.Nickname}";
            }

            if (score > currentMax)
            {
                currentMax = score;
            }
        }

        if (currentMax != maxScore)
        {
            maxScore = currentMax;
            labelMaxScore.text = maxScore.ToString();
        }

        playersEntryes.Sort(s_LeaderboardComparer);

        for (int i = 0; i < playersEntryes.Count; i++)
        {
            var entry = playersEntryes[i]?.entry;
            if (entry != null && entry.transform.GetSiblingIndex() != i)
            {
                entry.transform.SetSiblingIndex(i);
            }
        }
    }

    public void ShowPerkPanel(List<PerkID> usedPerks)
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.1f);

            if (perkProgressBar != null)
            {
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
            }

            if (perkHandler != null)
            {
                perkHandler.Init(usedPerks);
            }
        }
    }

    public void ClosePerkPanel()
    {
        if (perkHandler != null) perkHandler.Clear();
        if (perkProgressBar != null)
        {
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
    }

    void UpdateHealthBars()
    {
        var cam = MainCamera;
        if (cam == null) return;

        float scaleX = transform.lossyScale.x;
        if (Mathf.Abs(scaleX - lastLossyScaleX) > 0.001f || cachedYOffset == 0)
        {
            lastLossyScaleX = scaleX;
            cachedYOffset = scaleX > 0.0001f ? (int)(Screen.height / 11f / scaleX) : 0;
        }

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        for (int i = barsPlayers.Count - 1; i >= 0; i--)
        {
            var item = barsPlayers[i];
            if (item == null || item.player == null)
            {
                if (item?.bar != null) Destroy(item.bar.gameObject);
                barsPlayers.RemoveAt(i);
                continue;
            }

            var screenPos = cam.WorldToScreenPoint(item.player.transform.position);

            if (screenPos.z <= 0 || screenPos.x < 0 || screenPos.x > screenWidth 
                || screenPos.y < 0 || screenPos.y > screenHeight)
            {
                item.canvas.alpha = 0;
                continue;
            }
            else
            {
                item.canvas.alpha = 1;
                var pos = screenPos / scaleX;
                (item.bar.transform as RectTransform).anchoredPosition = pos - new Vector3(0, cachedYOffset, 0);
            }

            if (item.player.Health != null && item.player.Health.MaxValue > 0)
            {
                var value = (float)item.player.Health.Value / (float)item.player.Health.MaxValue;
                if (Mathf.Abs(item.bar.value - value) > 0.001f)
                {
                    item.bar.value = value;
                }
            }
        }

        for (int i = healthBars.Count - 1; i >= 0; i--)
        {
            var item = healthBars[i];
            if (item == null || item.health == null)
            {
                if (item?.bar != null) Destroy(item.bar.gameObject);
                healthBars.RemoveAt(i);
                continue;
            }

            var screenPos = cam.WorldToScreenPoint(item.health.transform.position);

            if (screenPos.z <= 0 || screenPos.x < 0 || screenPos.x > screenWidth
                || screenPos.y < 0 || screenPos.y > screenHeight)
            {
                item.canvas.alpha = 0;
                continue;
            }
            else
            {
                item.canvas.alpha = 1;
                var pos = screenPos / scaleX;
                (item.bar.transform as RectTransform).anchoredPosition = pos - new Vector3(0, cachedYOffset, 0);
            }

            if (item.health.MaxValue > 0)
            {
                var value = (float)item.health.Value / (float)item.health.MaxValue;
                if (Mathf.Abs(item.bar.value - value) > 0.001f)
                {
                    item.bar.value = value;
                }
            }
        }
    }

    void UpdatePerkBar()
    {
        if (!playerMine || perkProgressBar == null)
            return;

        var curProgress = playerMine.PerkProgress.Evaluate(playerMine.usedPerks.Count);

        perkProgressBar.value = playerMine.Score / curProgress;
    }

    private void Cam_Clicked()
    {
        EventsHolder.onCamClicked?.Invoke();
    }

    private void OnDestroy()
    {
        EventsHolder.playerSpawnedMine.RemoveListener(PlayerMine_Spawned);
        EventsHolder.playerSpawnedAny.RemoveListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.RemoveListener(Player_Anniged);
        EventsHolder.onCrepochekDamage.RemoveListener(Crepochek_Damaged);
        EventsHolder.profileSeted.RemoveListener(Profile_Seted);
        EventsHolder.creepAnniged.RemoveListener(Creepochek_Anniged);
        EventsHolder.runeSpawned.RemoveListener(Rune_Spawned);
        EventsHolder.runeTaked.RemoveListener(Rune_Taked);
        EventsHolder.runeEnded.RemoveListener(Rune_Ended);
        EventsHolder.onVictory.RemoveListener(Victory);
        EventsHolder.onDefeat.RemoveListener(Defeat);
        EventsHolder.clientGameCompleted.RemoveListener(OnClientGameCompletedDefeat);
        EventsHolder.clientCompleteDataReceived.RemoveListener(ClientData_Received);

        if (respawnTimer != null)
        {
            respawnTimer.onTimerNotify -= RespawnTimer;
        }

        if (playerMine != null)
        {
            playerMine.onUpgradeAvailable -= Upgrade_Availabled;
            playerMine.onNextStageAvailable -= NextStage_Availabled;
        }
    }

    private void OnClientGameCompletedDefeat()
    {
        GameComplete(CompleteStatus.Defeat);
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
        public int lastScore = -1;
        public int lastLvl = -1;
    }
}
