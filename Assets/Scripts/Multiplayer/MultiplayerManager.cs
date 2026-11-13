using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using static Settings;
using Unity.Netcode;

public class MultiplayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject aiPrefab;
    [SerializeField] bool useAI = true;

    [Header("*DEV*")]
    [SerializeField] TMPro.TMP_Text networkRole;

    bool applicationIsQuit;

    public static MultiplayerManager Instance { get; set; }

    public static bool IsMaster => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    public static int ServerTime => NetworkManager.Singleton != null ? (int)(NetworkManager.Singleton.ServerTime.Time * 1000) : Environment.TickCount;
    public static List<Player> Players { get; } = new List<Player>();

    public Action<GameObject> onPlayerSpawned;

    float currentGameTime = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient))
        {
            SceneManager.LoadScene(0);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogError("Больше 1-го MultiplayerManager ");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (networkRole != null)
        {
            networkRole.text = IsServer ? (IsHost ? "Host" : "Server") : "Client";
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);

            SpawnAI();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        currentGameTime += Time.deltaTime;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            var disconnectedPlayer = GameManager.Instance.allPlayers.Find(p => p != null && p.OwnerClientId == clientId);
            if (disconnectedPlayer != null)
            {
                GameManager.Instance.allPlayers.Remove(disconnectedPlayer);
            }
        }
        else
        {
            if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
            {
                Leave();
            }
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        var pos = GameManager.Instance.GetSpawnPos();
        var playerObj = Instantiate(playerPrefab, pos, Quaternion.identity);
        var netObj = playerObj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);

        onPlayerSpawned?.Invoke(playerObj);
    }

    public static void SpawnPlayer()
    {
        if (Instance != null && NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Instance.SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                Instance.RequestSpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerServerRpc(ulong clientId)
    {
        SpawnPlayerForClient(clientId);
    }

    public static void RespawnPlayer()
    {
        if (Instance != null && NetworkManager.Singleton != null)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            string nick = LobbyManager.GetNickname();
            if (NetworkManager.Singleton.IsServer)
            {
                Instance.RespawnPlayerOnServer(localClientId, nick);
            }
            else
            {
                Instance.RequestRespawnPlayerServerRpc(localClientId, nick);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRespawnPlayerServerRpc(ulong clientId, string nickname)
    {
        RespawnPlayerOnServer(clientId, nickname);
    }

    private void RespawnPlayerOnServer(ulong clientId, string nickname)
    {
        var respawnData = GameManager.Instance.respawnQueue.Find(r => !r.isAI && (r.name == nickname || r.viewID == clientId));
        if (respawnData == null && GameManager.Instance.respawnQueue.Count > 0)
        {
            respawnData = GameManager.Instance.respawnQueue.Find(r => !r.isAI);
        }

        if (respawnData == null)
            return;

        GameManager.Instance.respawnQueue.Remove(respawnData);

        var pos = GameManager.Instance.GetSpawnPos();
        var playerObj = Instantiate(playerPrefab, pos, Quaternion.identity);
        var netObj = playerObj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);

        var player = playerObj.GetComponent<Player>();
        onPlayerSpawned?.Invoke(playerObj);

        player.CountAnniged = respawnData.countAnniged;
        player.CountAnniges = respawnData.countAnniges;
        player.Score = respawnData.score;
        player.Nickname = respawnData.name;

        var netPlayer = playerObj.GetComponent<NetworkPlayer>();
        if (netPlayer != null)
        {
            netPlayer.SyncScoreOnRespawnServer(respawnData.score, respawnData.countAnniges, respawnData.countAnniged);
        }

        StartCoroutine(PerkSender());

        IEnumerator PerkSender()
        {
            foreach (var perk in respawnData.usedPerks)
            {
                player.PickPerk(perk);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void NotifyGameComplete(Team winningTeam)
    {
        if (IsServer)
        {
            GameCompleteClientRpc((int)winningTeam);
        }
    }

    [ClientRpc]
    private void GameCompleteClientRpc(int winningTeamInt)
    {
        Team winningTeam = (Team)winningTeamInt;
        GameManager.Instance.complete = true;
        if (GameManager.Instance.MineTeam == winningTeam)
        {
            EventsHolder.onVictory?.Invoke(GameManager.Instance.MineTeam);
        }
        else
        {
            EventsHolder.onDefeat?.Invoke(GameManager.Instance.MineTeam);
        }
    }

    void SpawnAI()
    {
        if (!IsServer || !useAI)
            return;

        List<int> usedProfiles = new();

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1f);

            int currentConnected = NetworkManager.Singleton.ConnectedClients.Count;
            int countAI = 0;
            while (currentConnected + countAI < MAX_PLAYERS)
            {
                var pos = GameManager.Instance.GetSpawnPos();
                var p = Instantiate(aiPrefab, pos, Quaternion.identity);
                var netObj = p.GetComponent<NetworkObject>();
                netObj.Spawn();

                var player = p.GetComponent<Player>();

                int profileIdx = 0;
                bool profileFound = false;

                while (!profileFound)
                {
                    profileIdx = Random.Range(0, aiProfiles.Count);
                    if (!usedProfiles.Contains(profileIdx))
                    {
                        usedProfiles.Add(profileIdx);
                        profileFound = true;
                    }
                }

                var profile = aiProfiles[profileIdx];
                player.GetComponent<BehaviourAI>().SetProfile(profile, profileIdx);

                countAI++;

                yield return new WaitForSeconds(0.178f);
            }
        }
    }

    public void SpawnAI(RespawnData data)
    {
        if (!IsServer || !useAI)
            return;

        var pos = GameManager.Instance.GetSpawnPos();
        var p = Instantiate(aiPrefab, pos, Quaternion.identity);
        var netObj = p.GetComponent<NetworkObject>();
        netObj.Spawn();

        var player = p.GetComponent<Player>();

        player.Health.Team = data.team;
        player.CountAnniges = data.countAnniges;
        player.CountAnniged = data.countAnniged;
        player.Score = data.score;
        player.Team_Seted(data.team);
        var profile = aiProfiles.Find(pr => pr.name == data.name);
        var idxProfile = aiProfiles.IndexOf(profile);
        player.GetComponent<BehaviourAI>().SetProfile(profile, idxProfile);

        StartCoroutine(PerkSender());

        IEnumerator PerkSender()
        {
            foreach (var perk in data.usedPerks)
            {
                player.PickPerk(perk);
                yield return new WaitForSeconds(0.07f);
            }
        }
    }

    public static void AddPlayer(Player player) => Players.Add(player);

    public static T Spawn<T>(T prefab, Vector3 position) where T : Component
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return null;

        var spawned = Instantiate(prefab, position, Quaternion.identity);
        var netObj = spawned.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn();
        }

        return spawned;
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return null;

        var spawned = Instantiate(prefab, position, Quaternion.identity);
        var netObj = spawned.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn();
        }

        return spawned;
    }

    public static void DestroyByPhoton(GameObject target)
    {
        if (target == null) return;

        var netObj = target.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            netObj.Despawn(true);
        }
        else
        {
            Destroy(target);
        }
    }

    public void Leave()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (!applicationIsQuit)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuit = true;
    }
}