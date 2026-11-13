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

    private bool isConnecting;

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

        inputField.text = nickname;
        inputField.onValueChanged.AddListener(SetNick);

        // Only register direct button click if MainMenuUI is not driving the flow
        var mainMenu = Object.FindAnyObjectByType<BatlIO.UI.MainMenuUI>();
        if (mainMenu == null)
        {
            btnConnect.onClick.AddListener(() => JoinRoom());
        }

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

    public void JoinRoom(string targetIp = null)
    {
        if (isConnecting) return;

        btnConnect.interactable = false;
        btnLabel.text = Language.Rus ? "Поиск игры.." : "Game searching..";

        StartCoroutine(StartNetworkFlow(targetIp));
    }

    private IEnumerator StartNetworkFlow(string targetIp)
    {
        isConnecting = true;

        var netManager = NetworkManager.Singleton;

        if (netManager.IsServer || netManager.IsClient)
        {
            netManager.Shutdown();
            yield return null;
        }

        var transport = netManager.GetComponent<UnityTransport>();
        ushort targetPort = 7777;
        string targetAddress = "127.0.0.1";
        bool isExplicitRemoteIp = false;

        if (!string.IsNullOrWhiteSpace(targetIp) && IPAddress.TryParse(targetIp.Trim(), out var parsedFromArg))
        {
            targetAddress = parsedFromArg.ToString();
            if (targetAddress != "127.0.0.1" && targetAddress != "0.0.0.0")
            {
                isExplicitRemoteIp = true;
            }
        }
        else if (IPAddress.TryParse(inputField.text.Trim(), out var parsedFromField))
        {
            targetAddress = parsedFromField.ToString();
            if (targetAddress != "127.0.0.1" && targetAddress != "0.0.0.0")
            {
                isExplicitRemoteIp = true;
            }
        }

        transport.SetConnectionData(targetAddress, targetPort);

        bool portOccupied = !IsPortAvailable(targetPort);

        if (isExplicitRemoteIp || portOccupied)
        {
            Log(Language.Rus ? "Подключение к серверу (" + targetAddress + ")..." : "Connecting to server (" + targetAddress + ")...");
            bool clientStarted = netManager.StartClient();
            if (!clientStarted)
            {
                Log(Language.Rus ? "Ошибка старта клиента!" : "Client start failed!");
                ResetConnectingState();
                yield break;
            }

            float clientTimeout = 6f;
            while (clientTimeout > 0 && netManager.IsClient && !netManager.IsConnectedClient)
            {
                clientTimeout -= Time.deltaTime;
                yield return null;
            }

            if (!netManager.IsConnectedClient)
            {
                Log(Language.Rus ? "Не удалось подключиться к серверу!" : "Failed to connect to server!");
                netManager.Shutdown();
                ResetConnectingState();
                yield break;
            }
        }
        else
        {
            Log(Language.Rus ? "Создание игры (Хост)..." : "Hosting game...");
            bool hostStarted = netManager.StartHost();
            if (!hostStarted || !netManager.IsServer)
            {
                Log(Language.Rus ? "Порт занят, пробуем подключиться как клиент..." : "Port busy, fallback to client...");
                transport.SetConnectionData("127.0.0.1", targetPort);
                bool clientStarted = netManager.StartClient();
                if (!clientStarted)
                {
                    ResetConnectingState();
                    yield break;
                }

                float clientTimeout = 6f;
                while (clientTimeout > 0 && netManager.IsClient && !netManager.IsConnectedClient)
                {
                    clientTimeout -= Time.deltaTime;
                    yield return null;
                }

                if (!netManager.IsConnectedClient)
                {
                    Log(Language.Rus ? "Не удалось подключиться!" : "Connection failed!");
                    netManager.Shutdown();
                    ResetConnectingState();
                }
                yield break;
            }

            float waitTime = 0;
            while (waitTime < 2.5f && netManager.IsServer)
            {
                waitTime += Time.deltaTime;
                btnLabel.text = (waitTime % 1.1f > 0.55f)
                    ? (Language.Rus ? "Поиск игры.." : "Game searching..")
                    : (Language.Rus ? "Поиск игры." : "Game searching.");

                if (netManager.ConnectedClients.Count > 1)
                {
                    break;
                }
                yield return null;
            }

            if (netManager.IsServer && netManager.IsListening)
            {
                netManager.SceneManager.LoadScene(sceneNameToload, LoadSceneMode.Single);
            }
            else
            {
                Log(Language.Rus ? "Ошибка старта хоста!" : "Host start error!");
                ResetConnectingState();
            }
        }
    }

    private void ResetConnectingState()
    {
        isConnecting = false;
        btnConnect.interactable = true;
        btnLabel.text = Language.Rus ? "ИГРАТЬ" : "PLAY";

        var mainMenu = Object.FindAnyObjectByType<BatlIO.UI.MainMenuUI>();
        if (mainMenu != null)
        {
            mainMenu.ResetPlayButton();
        }
    }

    private bool IsPortAvailable(int port)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ExclusiveAddressUse = true;
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
        if (!string.IsNullOrEmpty(msg))
        {
            logText.text += "\n" + msg;
        }

        var mainMenu = Object.FindAnyObjectByType<BatlIO.UI.MainMenuUI>();
        if (mainMenu != null && !string.IsNullOrEmpty(msg))
        {
            mainMenu.ShowStatus(msg);
        }
    }
}
