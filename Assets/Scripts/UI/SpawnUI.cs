using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using Photon.Pun;

public class SpawnUI : MonoBehaviour
{
    [SerializeField] Button btnPlay;
    [SerializeField] TMP_InputField input;

    public void Init()
    {
        input.onSubmit.AddListener(Submited);
        btnPlay.onClick.AddListener(Play_Clicked);
    }

    private void Play_Clicked()
    {
        if (!string.IsNullOrEmpty(input.text))
        {
            PhotonNetwork.NickName = input.text;
        }

    }

    private void Submited(string text)
    {
        Play_Clicked();
    }


}
