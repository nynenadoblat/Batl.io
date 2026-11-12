using UnityEngine;
using Unity.Netcode;

public class NetworkCreep : NetworkBehaviour
{
    Creep creep;

    private void Awake()
    {
        creep = GetComponent<Creep>();
        creep.Health.onAnniged += Anniged;
    }

    private void Anniged(ulong annigilatorID)
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}