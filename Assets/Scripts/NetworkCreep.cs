using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkCreep : MonoBehaviourPun
{
    Creep creep;

    private void Awake()
    {
        creep = GetComponent<Creep>();
        creep.Health.onAnniged += Anniged;
    }

    private void Anniged(int annigilatorID)
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(photonView);
    }
}
