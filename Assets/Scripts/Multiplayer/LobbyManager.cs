using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] string sceneNameToload = "Game";

    [Space(3)]
    [SerializeField] byte maxPlayers = 10;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI logText;
    [SerializeField] Button btnConnect;
    [SerializeField] TMP_InputField inputField;

    [Space]

    [SerializeField] TMP_Text btnLabel;

    public System.Action<string> onConnection;

    public static string disconectMsg;
    public static string nickname = string.Empty;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("nick"))
        {
            nickname = PlayerPrefs.GetString("nick");
        }

        if (string.IsNullOrEmpty(nickname))
        {
            if (Language.Rus)
                nickname = "Игрок " + Random.Range(100, 999);
            else
                nickname = "Player " + Random.Range(100, 999);
            PlayerPrefs.SetString("nick", nickname);
        }

        if (inputField != null)
        {
            inputField.text = nickname;
            inputField.onValueChanged.AddListener(SetNick);
        }

        btnConnect.onClick.AddListener(JoinRoom);

        if (!string.IsNullOrEmpty(disconectMsg))
        {
            Log(disconectMsg);
        }
    }

    public static string GetNickname()
    {
        if (!string.IsNullOrEmpty(nickname))
            return nickname;

        if (PlayerPrefs.HasKey("nick"))
        {
            nickname = PlayerPrefs.GetString("nick");
            return nickname;
        }

        nickname = (Language.Rus ? "Игрок " : "Player ") + Random.Range(100, 999);
        PlayerPrefs.SetString("nick", nickname);
        return nickname;
    }

    public static void SetNick(string value)
    {
        nickname = value;
        PlayerPrefs.SetString("nick", value);
    }

    public void JoinRoom()
    {
        btnConnect.interactable = false;
        btnLabel.text = Language.Rus ? "Поиск игры.." : "Game searching..";

        StartCoroutine(StartNetworkFlow());
    }

    private IEnumerator StartNetworkFlow()
    {
        var netManager = NetworkManager.Singleton;
        if (netManager == null)
        {
            Log("NetworkManager not found!");
            btnConnect.interactable = true;
            yield break;
        }

        var transport = netManager.GetComponent<UnityTransport>();

        string targetAddress = "127.0.0.1";
        ushort targetPort = 7777;

        bool isExplicitRemoteIp = false;
        if (inputField != null && IPAddress.TryParse(inputField.text.Trim(), out var parsedIp))
        {
            targetAddress = parsedIp.ToString();
            isExplicitRemoteIp = true;
        }

        if (transport != null)
        {
            transport.SetConnectionData(targetAddress, targetPort);
        }

        if (isExplicitRemoteIp || !IsPortAvailable(targetPort))
        {
            Log(Language.Rus ? "Подключение к игре..." : "Connecting to host...");
            netManager.StartClient();
        }
        else
        {
            Log(Language.Rus ? "Создание игры (Хост)..." : "Hosting game...");
            netManager.StartHost();

            float waitTime = 0;
            while (waitTime < 2.5f)
            {
                waitTime += Time.deltaTime;
                if (waitTime % 1.1f > 0.55f)
                    btnLabel.text = Language.Rus ? "Поиск игры.." : "Game searching..";
                else
                    btnLabel.text = Language.Rus ? "Поиск игры." : "Game searching.";

                if (netManager.ConnectedClients.Count > 1)
                {
                    break;
                }
                yield return null;
            }

            netManager.SceneManager.LoadScene(sceneNameToload, LoadSceneMode.Single);
        }
    }

    private bool IsPortAvailable(int port)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    void Log(string msg)
    {
        if (logText != null && !string.IsNullOrEmpty(msg))
        {
            logText.text += "\n" + msg;
        }
    }
}