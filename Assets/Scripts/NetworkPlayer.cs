using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    public ulong ViewID => NetworkObjectId;

    Player player;

    private readonly NetworkVariable<FixedString64Bytes> networkNickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkProfileIndex = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkTankType = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkStage = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkLvl = new(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkScoreTotal = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkScore = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkAnniges = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> networkAnniged = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        player = GetComponent<Player>();

        if (GetComponent<BehaviourPlayer>())
        {
            if (IsServer)
            {
                int idx = (int)(OwnerClientId % 2);
                var team = (Team)idx;
                var netHealth = GetComponent<NetworkHealthComponent>();
                if (netHealth != null)
                {
                    netHealth.SetTeam(team);
                }
                else
                {
                    player.Health.Team = team;
                }
            }

            if (IsOwner)
            {
                var team = player.Health.Team;
                EventsHolder.mineTeamPicked?.Invoke(team);

                string nick = LobbyManager.GetNickname();
                player.Nickname = nick;
                SetNicknameServerRpc(nick);
            }
            else
            {
                if (!networkNickname.Value.IsEmpty)
                {
                    player.Nickname = networkNickname.Value.ToString();
                }

                if (networkTankType.Value != 0)
                {
                    player.InitType((TankType)networkTankType.Value, true);
                }

                player.Lvl = Mathf.Max(1, networkLvl.Value);
                player.transform.localScale = Vector3.one * (1f + (player.Lvl - 1) * 0.08f);
                player.ScoreTotal = networkScoreTotal.Value;
                player.Score = networkScore.Value;
                player.CountAnniges = networkAnniges.Value;
                player.CountAnniged = networkAnniged.Value;
            }
        }

        var ai = GetComponent<BehaviourAI>();
        if (ai)
        {
            if (IsServer)
            {
                ai.onProfileSeted += OnAIProfileSetServer;
            }
            else if (networkProfileIndex.Value >= 0 && networkProfileIndex.Value < Settings.aiProfiles.Count)
            {
                var profile = Settings.aiProfiles[networkProfileIndex.Value];
                ai.SetProfile(profile, networkProfileIndex.Value, true);
            }

            if (!IsServer)
            {
                if (networkTankType.Value != 0)
                {
                    player.InitType((TankType)networkTankType.Value, true);
                }
                player.Lvl = Mathf.Max(1, networkLvl.Value);
                player.transform.localScale = Vector3.one * (1f + (player.Lvl - 1) * 0.08f);
                player.ScoreTotal = networkScoreTotal.Value;
                player.Score = networkScore.Value;
                player.CountAnniges = networkAnniges.Value;
                player.CountAnniged = networkAnniged.Value;
            }
        }

        player.onPerkPick += OnPerkPickedLocal;
        player.onPropertyUpgraded += OnPropertyUpgradedLocal;

        networkNickname.OnValueChanged += OnNicknameChanged;
        networkProfileIndex.OnValueChanged += OnProfileIndexChanged;
        networkTankType.OnValueChanged += OnTankTypeChanged;
        networkStage.OnValueChanged += OnStageChanged;
        networkLvl.OnValueChanged += OnLvlChanged;
        networkScoreTotal.OnValueChanged += OnScoreTotalChanged;
        networkScore.OnValueChanged += OnScoreChanged;
        networkAnniges.OnValueChanged += OnAnnigesChanged;
        networkAnniged.OnValueChanged += OnAnnigedChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkNickname.OnValueChanged -= OnNicknameChanged;
        networkProfileIndex.OnValueChanged -= OnProfileIndexChanged;
        networkTankType.OnValueChanged -= OnTankTypeChanged;
        networkStage.OnValueChanged -= OnStageChanged;
        networkLvl.OnValueChanged -= OnLvlChanged;
        networkScoreTotal.OnValueChanged -= OnScoreTotalChanged;
        networkScore.OnValueChanged -= OnScoreChanged;
        networkAnniges.OnValueChanged -= OnAnnigesChanged;
        networkAnniged.OnValueChanged -= OnAnnigedChanged;

        if (player != null)
        {
            player.onPerkPick -= OnPerkPickedLocal;
            player.onPropertyUpgraded -= OnPropertyUpgradedLocal;
        }

        base.OnNetworkDespawn();
    }

    private void OnNicknameChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        if (player != null && !current.IsEmpty)
        {
            player.Nickname = current.ToString();
        }
    }

    private void OnProfileIndexChanged(int previous, int current)
    {
        var ai = GetComponent<BehaviourAI>();
        if (ai != null && current >= 0 && current < Settings.aiProfiles.Count)
        {
            var profile = Settings.aiProfiles[current];
            ai.SetProfile(profile, current, true);
        }
    }

    private void OnTankTypeChanged(int previous, int current)
    {
        if (player != null && !IsOwner)
        {
            player.InitType((TankType)current, true);
        }
    }

    private void OnStageChanged(int previous, int current)
    {
        if (player != null)
        {
            player.Stage = current;
        }
    }

    private void OnLvlChanged(int previous, int current)
    {
        if (player != null)
        {
            player.Lvl = current;
            player.transform.localScale = Vector3.one * (1f + Mathf.Max(0, current - 1) * 0.08f);
        }
    }

    private void OnScoreTotalChanged(int previous, int current)
    {
        if (player != null)
        {
            player.ScoreTotal = current;
        }
    }

    private void OnScoreChanged(int previous, int current)
    {
        if (player != null)
        {
            player.Score = current;
        }
    }

    private void OnAnnigesChanged(int previous, int current)
    {
        if (player != null)
        {
            player.CountAnniges = current;
        }
    }

    private void OnAnnigedChanged(int previous, int current)
    {
        if (player != null)
        {
            player.CountAnniged = current;
        }
    }

    private void OnAIProfileSetServer(int profileIdx)
    {
        if (IsServer)
        {
            networkProfileIndex.Value = profileIdx;
        }
    }

    [ServerRpc]
    private void SetNicknameServerRpc(FixedString64Bytes nick)
    {
        networkNickname.Value = nick;
        if (player != null)
        {
            player.Nickname = nick.ToString();
        }
    }

    [ServerRpc]
    public void SetTankTypeServerRpc(int tankType, int stage)
    {
        networkTankType.Value = tankType;
        networkStage.Value = stage;
        if (player != null && !IsOwner)
        {
            player.InitType((TankType)tankType, true);
        }
    }

    [ServerRpc]
    public void SyncScoreServerRpc(int score, int totalScore)
    {
        networkScore.Value = score;
        networkScoreTotal.Value = totalScore;
        if (player != null)
        {
            player.Score = score;
            player.ScoreTotal = totalScore;
        }
    }

    [ServerRpc]
    public void SyncLvlServerRpc(int lvl, int score, int totalScore)
    {
        networkLvl.Value = lvl;
        networkScore.Value = score;
        networkScoreTotal.Value = totalScore;
        if (player != null)
        {
            player.Lvl = lvl;
            player.Score = score;
            player.ScoreTotal = totalScore;
            player.transform.localScale = Vector3.one * (1f + Mathf.Max(0, lvl - 1) * 0.08f);
        }
    }

    [ServerRpc]
    public void SyncKillsServerRpc(int kills)
    {
        networkAnniges.Value = kills;
        if (player != null)
        {
            player.CountAnniges = kills;
        }
    }

    [ServerRpc]
    public void SyncDeathsServerRpc(int deaths)
    {
        networkAnniged.Value = deaths;
        if (player != null)
        {
            player.CountAnniged = deaths;
        }
    }

    public void SyncScoreOnRespawnServer(int score, int kills, int deaths)
    {
        if (IsServer)
        {
            networkScore.Value = score;
            networkScoreTotal.Value = score;
            networkAnniges.Value = kills;
            networkAnniged.Value = deaths;
        }
    }

    private void OnPerkPickedLocal(PerkID perk)
    {
        if (IsOwner)
        {
            PickPerkServerRpc((int)perk);
        }
    }

    [ServerRpc]
    private void PickPerkServerRpc(int perkInt)
    {
        PerkID perk = (PerkID)perkInt;
        if (!IsOwner)
        {
            player.PickPerk(perk, true);
        }
        PickPerkClientRpc(perkInt);
    }

    [ClientRpc]
    private void PickPerkClientRpc(int perkInt)
    {
        if (!IsOwner && !IsServer)
        {
            player.PickPerk((PerkID)perkInt, true);
        }
    }

    private void OnPropertyUpgradedLocal(PerkID perk)
    {
        if (IsOwner)
        {
            UpgradePropertyServerRpc((int)perk);
        }
    }

    [ServerRpc]
    private void UpgradePropertyServerRpc(int perkInt)
    {
        PerkID perk = (PerkID)perkInt;
        if (!IsOwner)
        {
            player.UpgradeProperty(perk, true);
        }
        UpgradePropertyClientRpc(perkInt);
    }

    [ClientRpc]
    private void UpgradePropertyClientRpc(int perkInt)
    {
        if (!IsOwner && !IsServer)
        {
            player.UpgradeProperty((PerkID)perkInt, true);
        }
    }

    public void BroadcastFire(Vector3[] positions, Vector3[] directions, int speed, int damage, int critChance)
    {
        if (IsServer)
        {
            FireClientRpc(positions, directions, speed, damage, critChance);
        }
        else if (IsOwner)
        {
            FireServerRpc(positions, directions, speed, damage, critChance);
        }
    }

    [ServerRpc]
    private void FireServerRpc(Vector3[] positions, Vector3[] directions, int speed, int damage, int critChance)
    {
        FireClientRpc(positions, directions, speed, damage, critChance);
    }

    [ClientRpc]
    private void FireClientRpc(Vector3[] positions, Vector3[] directions, int speed, int damage, int critChance)
    {
        if (!IsOwner && player != null)
        {
            player.SpawnRemoteProjectiles(positions, directions, speed, damage, critChance);
        }
    }
}
