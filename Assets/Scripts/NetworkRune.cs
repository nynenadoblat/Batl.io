using UnityEngine;
using Unity.Netcode;

public class NetworkRune : NetworkBehaviour
{
    public void RuneTakedOnServer(Player player)
    {
        RuneTakedClientRpc(player.NetworkObjectId);
    }

    public void RuneEndedOnServer(Player player)
    {
        RuneEndedClientRpc(player.NetworkObjectId);
    }

    [ClientRpc]
    private void RuneTakedClientRpc(ulong takedViewID)
    {
        if (IsServer) return;

        bool playerFound = false;
        foreach (var player in GameManager.Instance.allPlayers)
        {
            if (player && player.NetworkObjectId == takedViewID)
            {
                player.TakeRune();
                GetComponent<Rune>().Take(player);
                playerFound = true;
                break;
            }
        }

        if (!playerFound && GameManager.Instance.allPlayers.Count > 0)
        {
            var first = GameManager.Instance.allPlayers.Find(p => p != null);
            if (first != null)
            {
                GetComponent<Rune>().Take(first);
            }
        }
    }

    [ClientRpc]
    private void RuneEndedClientRpc(ulong takedViewID)
    {
        if (IsServer) return;

        foreach (var player in GameManager.Instance.allPlayers)
        {
            if (player && player.NetworkObjectId == takedViewID)
            {
                player.RuneEnded();
                EventsHolder.runeEnded?.Invoke(player);
                break;
            }
        }
    }
}