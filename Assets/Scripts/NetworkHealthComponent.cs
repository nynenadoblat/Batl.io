using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkHealthComponent : MonoBehaviourPunCallbacks
{
    HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.onDamage += Damaged;
        health.onIncreaseHealth += Health_Increased;
        health.onAnniged += Anniged;
    }

    private void Start()
    {
        photonView.RegisterMethod<IntIntIntOwnerNetworkData>(EventCode.Damage, Network_Damaged, true);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.IncreaseHealth, NetworkIncreaseHealth_Received, true);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.TeamIdx, NetworkTeamIdx_Received);
        photonView.RegisterMethod<IntIntOwnerNetworkData>(EventCode.Anniged, NetworkAnnigedData_Received);

        RaiseTeamIdx();
    }

    private void NetworkIncreaseHealth_Received(IntOwnerNetworkData data)
    {
        if(data.viewID == photonView.ViewID)
        {
            health.IncreaseMaxHealth(data.value, true);
        }
    }

    private void NetworkTeamIdx_Received(IntOwnerNetworkData data)
    {
        if(photonView.ViewID == data.viewID)
        {
            health.Team = (Team)data.value;
        }
    }

    private void Health_Increased(int value)
    {
        var data = new IntOwnerNetworkData
        {
            value = value,
            viewID = photonView.ViewID,
        };
        photonView.RaiseEvent(EventCode.IncreaseHealth, data);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        RaiseTeamIdx();
    }

    void RaiseTeamIdx()
    {
        if (!health.IsAlive)
            return;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.7f);

            if (gameObject)
            {
                var data = new IntOwnerNetworkData
                {
                    value = (int)health.Team,
                    viewID = photonView.ViewID
                };
                photonView.RaiseEvent(EventCode.TeamIdx, data);
            }
        }
    }

    private void Network_Damaged(IntIntIntOwnerNetworkData data)
    {
        if(data.viewID == photonView.ViewID)
        {
            health.DamageNetworkReceived(data.value_1, data.value_2, data.value_3);
        }
    }

    private void Damaged(int value, int ownerID)
    {
        var data = new IntIntIntOwnerNetworkData
        {
            value_1 = value,
            value_2 = ownerID,
            //value_3 = потом надо добавить время дамага
            viewID = photonView.ViewID 
        };

        photonView.RaiseEventAll(EventCode.Damage, data);
    }

    private void NetworkAnnigedData_Received(IntIntOwnerNetworkData data)
    {
        if(data.viewID == photonView.ViewID)
        {
            if(health.AnnigedTime > data.value_2)
            {
                health.LastDamagedOwnerID = data.value_1;
            }
        }
    }

    private void Anniged(int obj)
    {
        var data = new IntIntOwnerNetworkData
        {
            value_1 = health.LastDamagedOwnerID,
            value_2 = PhotonNetwork.ServerTimestamp,
            viewID = photonView.ViewID,
        };
        photonView.RaiseEventAll(EventCode.Anniged, data);
    }
}
