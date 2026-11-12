using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class PerkHandlerUI : MonoBehaviour
{
    [SerializeField] List<Perk> allPerks;
    [SerializeField] PerkUI perkPrefab;
    [SerializeField] Transform parent;
    [SerializeField] HorizontalLayoutGroup group;

    public Action<PerkID> onPerkClick;

    public List<Perk> AllPerks => allPerks;

    public void Init(List<PerkID> usedPerks)
    {
        Clear();

        List<Perk> viewable = new();

        for (int i = 0; i < 3; i++)
        {
            bool perkFound = false;
            var perk = allPerks[Random.Range(0, allPerks.Count)];
            
            while (!perkFound)
            {
                var countSamePerk = usedPerks.FindAll(p => p == perk.id).Count;
                if (countSamePerk > perk.countStack)
                {
                    perk = allPerks[Random.Range(0, allPerks.Count)];
                }
                else
                {
                    perkFound = true;
                }
            }

            viewable.Add(perk);
        }

        ShowPerks(viewable);
    }

    void ShowPerks(List<Perk> perks)
    {
        foreach (var item in perks)
        {
            var p = Instantiate(perkPrefab, parent);
            var description = Language.Rus ? item.descriptionRus : item.descriptionEng;
            p.Init(item, description);
            p.onClick += Perk_Clicked;
        }

        FixUI();
    }

    private void Perk_Clicked(PerkID perkID)
    {
        onPerkClick?.Invoke(perkID);
    }

    void FixUI()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return null;

            group.childForceExpandWidth = false;

            yield return null;

            group.childForceExpandWidth = true;
        }
    }

    public void Clear()
    {
        foreach (Transform item in parent)
        {
            Destroy(item.gameObject);
        }
    }
}
