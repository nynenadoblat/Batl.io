using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerComplateEntry : MonoBehaviour
{
    [SerializeField] Image outline;
    [SerializeField] TMP_Text labelNickname;
    [SerializeField] TMP_Text labelAnniges;
    [SerializeField] TMP_Text labelAnniged;
    [SerializeField] Transform perkParent;
    [SerializeField] Image usedPerkPrefab;

    [Space]

    [SerializeField] Color friendly;
    [SerializeField] Color enemy;

    public void Init(CompleteUI.PlayerEntry playerEntry, List<Perk> allPerks)
    {
        labelNickname.text = playerEntry.name;
        labelAnniges.text = $"Ó\n{playerEntry.countAnniges}";
        labelAnniged.text = $"Ñ\n{playerEntry.countAnniged}";

        foreach (var perk in playerEntry.perks)
        {
            var p = Instantiate(usedPerkPrefab, perkParent);
            p.transform.GetChild(0).GetComponent<Image>().sprite = allPerks.Find(i => i.id == perk).icon;
        }

        if (GameManager.Instance.MineTeam == playerEntry.team)
        {
            outline.color = friendly;
        }
        else
        {
            outline.color = enemy;
        }
    }
}
