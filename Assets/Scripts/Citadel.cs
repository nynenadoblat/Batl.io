using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citadel : MonoBehaviourPun
{
    
    [SerializeField] HealthComponent health;

    public HealthComponent Health => health;

    private void Start()
    {
        health.onAnniged += Anniged;
    }

    private void Anniged(int annigerID)
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(photonView);
    }
}
