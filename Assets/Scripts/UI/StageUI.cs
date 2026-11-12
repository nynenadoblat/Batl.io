using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class StageUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Button btn;

    [SerializeField] List<StageSetUI> sets;

    public Action<TankType> onStageCliked;

    TankType stage;

    public void Init(TankType tankType)
    {
        stage = tankType;

        btn.onClick.AddListener(Stage_Clicked);

        var set = sets.Find(s => s.tankType == tankType);
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
