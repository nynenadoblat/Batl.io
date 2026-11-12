using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Settings;

public class Rune : NetworkBehaviour
{
    [SerializeField] GameObject effectTakedPrefab;

    Player owner;

    public bool taked;

    public float currentDuration;

    private void Start()
    {
        EventsHolder.runeSpawned?.Invoke(this);
    }

    private void Update()
    {
        if (!taked && IsServer)
        {
            var players = GameManager.Instance.allPlayers;

            foreach (var player in players)
            {
                if (!player)
                    continue;

                var dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < 1.37f)
                {
                    player.TakeRune();
                    Take(player);
                    var netRune = GetComponent<NetworkRune>();
                    if (netRune != null)
                    {
                        netRune.RuneTakedOnServer(player);
                    }
                    break;
                }
            }
        }

        if (taked)
        {
            currentDuration += Time.deltaTime;

            if (currentDuration > RUNE_DURATION && IsServer)
            {
                if (owner)
                {
                    owner.RuneEnded();
                    EventsHolder.runeEnded?.Invoke(owner);
                    var netRune = GetComponent<NetworkRune>();
                    if (netRune != null)
                    {
                        netRune.RuneEndedOnServer(owner);
                    }
                }
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    public void Take(Player player)
    {
        owner = player;

        taked = true;

        foreach (var item in GetComponentsInChildren<ParticleSystem>())
        {
            item.Stop();
        }

        EventsHolder.runeTaked?.Invoke(player);

        Instantiate(effectTakedPrefab, transform.position, Quaternion.identity);
    }
}