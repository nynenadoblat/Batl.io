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
        }

        player.onPerkPick += OnPerkPickedLocal;

        networkNickname.OnValueChanged += OnNicknameChanged;
        networkProfileIndex.OnValueChanged += OnProfileIndexChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkNickname.OnValueChanged -= OnNicknameChanged;
        networkProfileIndex.OnValueChanged -= OnProfileIndexChanged;
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
}