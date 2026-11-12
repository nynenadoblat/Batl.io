using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using static Settings;

public class UpgradePropertyUI : MonoBehaviour
{
    [SerializeField] RectTransform separatorPrefab;
    [SerializeField] Transform parent;
    [SerializeField] Button btn;
    [SerializeField] TMP_Text labelName;
    [SerializeField] Image filler;

    public Action<PerkID> onPropertyCliked;

    PerkID perkID;

    float fillStep;

    public void Init(Perk perk)
    {
        perkID = perk.id;
        fillStep = 1f / (MAX_PERKS + 1);

        filler.color = perk.color;

        btn.onClick.AddListener(Property_Clicked);

        var angle = 360 / (MAX_PERKS + 1);
        for (int i = 0; i < MAX_PERKS + 1; i++)
        {
            var sep = Instantiate(separatorPrefab, parent);
            sep.localEulerAngles = new(0, 0, angle * i);
        }

        filler.fillAmount = fillStep;

        labelName.text = Language.Rus ? perk.descriptionRus : perk.descriptionEng;
    }

    private void Property_Clicked()
    {
        filler.fillAmount += fillStep;

        if(Mathf.Approximately(filler.fillAmount, 1))
        {
            btn.interactable = false;
        }

        onPropertyCliked?.Invoke(perkID);
    }
}
