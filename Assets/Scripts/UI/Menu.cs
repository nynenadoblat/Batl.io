using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class Menu : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameInput;
    [SerializeField] Button btnSubmitNikname;

    [SerializeField] HorizontalOrVerticalLayoutGroup[] layoutGroups;
    [SerializeField] TMP_InputField nicknameHolder;

    [Space]

    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject nicknamePanel;

    [Space]

    [SerializeField] GameObject[] infoControlMobile;
    [SerializeField] GameObject[] infoControlPC;

    private void Start()
    {
        foreach (var item in infoControlMobile) item.SetActive(Application.isMobilePlatform);
        foreach (var item in infoControlPC) item.SetActive(!Application.isMobilePlatform);

        nicknameHolder.text = LobbyManager.GetNickname();
        nicknameHolder.onSubmit.AddListener(Nickname_Changed);
        nicknameHolder.onValueChanged.AddListener(Nickname_Changed);        

        if (PlayerPrefs.HasKey("nick"))
        {
            InitMainPanel();
        }
        else
        {
            mainPanel.SetActive(false);
            nicknamePanel.SetActive(true);

            nicknameInput.text = LobbyManager.GetNickname();
            nicknameInput.onSubmit.AddListener(Submited);
            btnSubmitNikname.onClick.AddListener(Submited);
        }
    }

    private void Nickname_Changed(string nick)
    {
        UpdateNicknameData(nick);
    }

    private void Submited()
    {
        Submited(nicknameInput.text);
    }

    private void Submited(string nick)
    {
        UpdateNicknameData(nick);

        InitMainPanel();
    }

    void InitMainPanel()
    {
        mainPanel.SetActive(true);
        nicknamePanel.SetActive(false);

        nicknameHolder.text = LobbyManager.GetNickname();

        FixLayoutGroups();
    }

    void UpdateNicknameData(string nick)
    {
        LobbyManager.SetNick(nick);

        PlayerPrefs.SetString("nick", nick);
        PlayerPrefs.Save();
    }

    void FixLayoutGroups()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.3f);

            foreach (var item in layoutGroups)
            {
                item.childForceExpandWidth = false;
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var item in layoutGroups)
            {
                item.childForceExpandWidth = true;
            }
        }
    }
}
