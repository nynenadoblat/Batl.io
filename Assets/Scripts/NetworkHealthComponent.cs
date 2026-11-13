using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class NetworkHealthComponent : NetworkBehaviour
{
    private HealthComponent health;

    private readonly NetworkVariable<int> netHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> netMaxHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<Team> netTeam = new(
        Team.One,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.onDamage += OnLocalDamage;
        health.onIncreaseHealth += OnLocalHealthIncreased;
        health.onAnniged += OnLocalAnniged;
        health.onHeal += OnLocalHeal;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        health.OwnerID = NetworkObjectId;

        if (IsServer)
        {
            netHealth.Value = health.Value;
            netMaxHealth.Value = health.MaxValue;
            netTeam.Value = health.Team;
        }
        else
        {
            if (netMaxHealth.Value != health.MaxValue)
            {
                health.IncreaseMaxHealth(netMaxHealth.Value - health.MaxValue, true);
            }
            health.Team = netTeam.Value;
        }

        netHealth.OnValueChanged += OnHealthChanged;
        netMaxHealth.OnValueChanged += OnMaxHealthChanged;
        netTeam.OnValueChanged += OnTeamChanged;
    }

    public override void OnNetworkDespawn()
    {
        netHealth.OnValueChanged -= OnHealthChanged;
        netMaxHealth.OnValueChanged -= OnMaxHealthChanged;
        netTeam.OnValueChanged -= OnTeamChanged;
        base.OnNetworkDespawn();
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (!IsServer)
        {
            health.SetHealthNetwork(newValue, health.LastDamagedOwnerID);
        }
    }

    private void OnLocalHeal(int healAmount)
    {
        if (IsServer)
        {
            netHealth.Value = health.Value;
        }
    }

    private void OnMaxHealthChanged(int previousValue, int newValue)
    {
        if (!IsServer)
        {
            int delta = newValue - health.MaxValue;
            if (delta != 0)
            {
                health.IncreaseMaxHealth(delta, true);
            }
        }
    }

    private void OnTeamChanged(Team previousValue, Team newValue)
    {
        health.Team = newValue;
    }

    public void SetTeam(Team team)
    {
        if (IsServer)
        {
            netTeam.Value = team;
            health.Team = team;
        }
    }

    private void OnLocalDamage(int value, ulong ownerID)
    {
        if (IsServer)
        {
            netHealth.Value = health.Value;
            DamageReceivedClientRpc(value, ownerID);
        }
        else
        {
            RequestDamageServerRpc(value, ownerID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(int value, ulong ownerID)
    {
        health.Damage(value, ownerID);
        netHealth.Value = health.Value;
        DamageReceivedClientRpc(value, ownerID);
    }

    [ClientRpc]
    private void DamageReceivedClientRpc(int value, ulong ownerID)
    {
        if (!IsServer)
        {
            health.LastDamagedOwnerID = ownerID;
            health.TimeLastDamage = Time.time;
        }
    }

    private void OnLocalHealthIncreased(int value)
    {
        if (IsServer)
        {
            netMaxHealth.Value = health.MaxValue;
            netHealth.Value = health.Value;
        }
        else
        {
            RequestIncreaseHealthServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestIncreaseHealthServerRpc(int value)
    {
        health.IncreaseMaxHealth(value, true);
        netMaxHealth.Value = health.MaxValue;
        netHealth.Value = health.Value;
    }

    private void OnLocalAnniged(ulong lastDamagedOwnerID)
    {
        if (IsServer)
        {
            AnnigedClientRpc(lastDamagedOwnerID);
        }
    }

    [ClientRpc]
    private void AnnigedClientRpc(ulong lastDamagedOwnerID)
    {
        if (!IsServer)
        {
            health.LastDamagedOwnerID = lastDamagedOwnerID;
            if (health.IsAlive)
            {
                health.Annigilaion(true);
            }
        }
    }
}