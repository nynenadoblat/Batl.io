using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class StageUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Button btn;
    [SerializeField] TMP_Text labelName;

    [SerializeField] List<StageSetUI> sets = new();

    public Action<TankType> onStageCliked;

    TankType stage;

    public void Init(TankType tankType)
    {
        stage = tankType;

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Stage_Clicked);
        }

        if (labelName != null)
        {
            labelName.text = GetTankDisplayName(tankType);
        }

        var set = sets?.Find(s => s.tankType == tankType);
        if (set != null && set.icon != null)
        {
            icon.sprite = set.icon;
            icon.enabled = true;
        }
        else
        {
            var loadedSprite = Resources.Load<Sprite>($"Tanks/Tank_{tankType}");
            if (loadedSprite != null)
            {
                icon.sprite = loadedSprite;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }
    }

    public static string GetTankDisplayName(TankType type)
    {
        return type switch
        {
            TankType.Simple => "Simple",
            TankType.Twin => "Twin",
            TankType.Trooper => "Trooper",
            TankType.Guard => "Guard",
            TankType.TripleShot => "Triple Shot",
            TankType.DualTrooper => "Dual Trooper",
            TankType.QuadGuard => "Quad Guard",
            TankType.Octo => "Octo",
            TankType.StormTrooper => "Storm",
            TankType.Triplet => "Triplet",
            _ => type.ToString()
        };
    }

    private void Stage_Clicked()
    {
        onStageCliked?.Invoke(stage);
    }
}


[Serializable]
public class StageSetUI
{
    public TankType tankType;
    public Sprite icon;
}
