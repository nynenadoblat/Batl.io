using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static Settings;

public class Rune : MonoBehaviour
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
        if (!taked && MultiplayerManager.IsMaster)
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

                    break;
                }
            }
        }

        if (taked)
        {
            currentDuration += Time.deltaTime;

            if(currentDuration > RUNE_DURATION && MultiplayerManager.IsMaster)
            {
                PhotonNetwork.Destroy(gameObject);
                owner.RuneEnded();
                EventsHolder.runeEnded?.Invoke(owner);
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
