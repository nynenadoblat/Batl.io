using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class PerkUI : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text description;

    public Action<PerkID> onClick;

    public PerkID ID { get; set; }

    public void Init(Perk perk, string descr)
    {
        ID = perk.id;

        description.text = descr;
        icon.sprite = perk.icon;
        button.onClick.AddListener(Perk_Clicked);
    }

    private void Perk_Clicked()
    {
        onClick?.Invoke(ID);
    }
}
