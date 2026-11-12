using UnityEngine;
using Unity.Netcode;

public class Citadel : NetworkBehaviour
{
    [SerializeField] HealthComponent health;

    public HealthComponent Health => health;

    private void Start()
    {
        health.onAnniged += Anniged;
    }

    private void Anniged(ulong annigerID)
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}