using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public int ViewID => photonView.ViewID;

    Player player;
    Coroutine dataSender;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (GetComponent<BehaviourPlayer>())
        {
            int idx = photonView.OwnerActorNr % 2;
            player.Health.Team = (Team)idx;

            if (photonView.IsMine)
            {
                EventsHolder.mineTeamPicked?.Invoke((Team)idx);

                player.Nickname = PhotonNetwork.NickName;
            }
            else
            {
                var p = PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr];
                player.Nickname = p.NickName;
            }

            //EventsHolder.teamPicked?.Invoke(player.Health);
            //player.Team_Seted((Team)idx);
        }

        var ai = GetComponent<BehaviourAI>();
        if (ai)
        {
            ai.onProfileSeted += RaiseProfile;
            photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.Profile, NetworkProfile_Seted);
        }

        player.onPerkPick += Perk_Picked;

        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.PerkPick, Network_PerkPick);
        
        //EventsHolder.onVictory.AddListener(Game_Completed);
    }

    

    //private void Game_Completed(Team team)
    //{
    //    var data = new IntOwnerNetworkData
    //    {
    //        value = player.CountAnniges,
    //        viewID = photonView.ViewID,
    //    };
    //    photonView.RaiseEventAll(EventCode.AllAnniges, data);
    //}

    private void NetworkProfile_Seted(IntOwnerNetworkData data)
    {
        if(data.viewID == photonView.ViewID)
        {
            var profile = Settings.aiProfiles[data.value];
            player.GetComponent<BehaviourAI>().SetProfile(profile, data.value, true);
        }
    }

    void RaiseProfile(int idx)
    {
        var data = new IntOwnerNetworkData
        {
            value = idx,
            viewID = photonView.ViewID
        };
        photonView.RaiseEvent(EventCode.Profile, data);
    }

    private void Network_PerkPick(IntOwnerNetworkData data)
    {
        if(ViewID == data.viewID)
        {
            player.PickPerk((PerkID)data.value, true);
            //print("Перк выбран где-то там");
        }
    }

    private void Perk_Picked(PerkID perk)
    {
        IntOwnerNetworkData data = new()
        {
            value = (int)perk,
            viewID = ViewID
        };
        photonView.RaiseEventAll(EventCode.PerkPick, data);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (photonView.IsMine && player.Health.IsAlive)
        {
            dataSender = StartCoroutine(DataSender());

            // Костыль
            player.Move(transform.right);
        }

        IEnumerator DataSender()
        {
            // Перки
            foreach (var perk in player.usedPerks)
            {
                yield return new WaitForSeconds(0.37f);

                Perk_Picked(perk);
            }

            yield return new WaitForSeconds(0.37f);

            var ai = GetComponent<BehaviourAI>();
            if (ai)
            {
                RaiseProfile(ai.idxProfile);
            }

            dataSender = null;
        }
    }

    private void Network_Anniged(int obj)
    {
        
    }

    private void Anniged()
    {
        //photonView.RaiseEventAll(EventCode.Anniged, photonView.ViewID);
    }

    private void OnDestroy()
    {
        if (dataSender != null)
            StopCoroutine(dataSender);
    }
}
