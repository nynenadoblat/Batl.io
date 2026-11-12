using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkRune : MonoBehaviourPun
{
    private void Awake()
    {
        EventsHolder.runeTaked.AddListener(Rune_Taked);
        EventsHolder.runeEnded.AddListener(Rune_Ended);
    }

    private void Start()
    {
        photonView.RegisterMethod<int>(EventCode.RuneTaked, NetworkRune_Taked);
        photonView.RegisterMethod<int>(EventCode.RuneEnded, NetworkRune_Ended);
    }

    private void NetworkRune_Ended(int runeOwnerID)
    {
        foreach (var player in GameManager.Instance.allPlayers)
        {
            if (!player)
                continue;

            if (player.photonView.ViewID == runeOwnerID)
            {
                player.RuneEnded();
                EventsHolder.runeEnded?.Invoke(player);
            }
        }
    }

    private void NetworkRune_Taked(int takedViewID)
    {
        var playerFound = false;

        foreach (var player in GameManager.Instance.allPlayers)
        {
            if (!player)
                continue;

            if (player.photonView.ViewID == takedViewID)
            {
                player.TakeRune();
                GetComponent<Rune>().Take(player);
                playerFound = true;
                break;
            }
        }

        // MEGA просто пиздец какой-то фикс,
        // в общем иногда мастер плеер не спавнится на клиенте
        // если резко одновременно зайти в комнату
        // и нужен хоть какой-то игрок чтобы руна пропала
        if (!playerFound)
        {
            var randomPlayer = GameManager.Instance.allPlayers.Find(p => p.Nickname != PhotonNetwork.NickName);
            GetComponent<Rune>().Take(randomPlayer);
        }
    }

    private void Rune_Taked(Player player)
    {
        photonView.RaiseEvent(EventCode.RuneTaked, player.photonView.ViewID);
    }

    private void Rune_Ended(Player player)
    {
        photonView.RaiseEvent(EventCode.RuneEnded, player.photonView.ViewID);
    }
}
