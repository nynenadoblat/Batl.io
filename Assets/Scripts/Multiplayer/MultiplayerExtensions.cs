using System;
using UnityEngine;
using Unity.Netcode;

public static class MultiplayerExtensions
{
    public static bool IsMine(this MonoBehaviour component)
    {
        if (component == null) return false;

        var networkBehaviour = component as NetworkBehaviour ?? component.GetComponent<NetworkBehaviour>();
        if (networkBehaviour != null)
        {
            return networkBehaviour.IsOwner;
        }

        var networkObject = component.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            return networkObject.IsOwner;
        }

        return false;
    }

    public static NetworkObject GetNetworkObject(this MonoBehaviour component)
    {
        return component == null ? null : component.GetComponent<NetworkObject>();
    }
}