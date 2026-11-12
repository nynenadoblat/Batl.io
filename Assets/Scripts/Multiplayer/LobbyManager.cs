using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string sceneNameToload;

    [Space(3)]
    [SerializeField] byte maxPlayers = 10;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI logText;
    [SerializeField] Button btnConnect;
    [SerializeField] TMP_InputField inputField;

    [Space]

    [SerializeField] TMP_Text btnLabel;

    ExitGames.Client.Photon.Hashtable roomProps;

    public System.Action<string> onPhotonConnection;

    public static string disconectMsg;

    void Awake()
    {
        if (PlayerPrefs.HasKey("nick"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("nick");
        }

        // Первоначальные настройки клиента, тупо спизжено из тутора
        if (PhotonNetwork.NickName == string.Empty)
        {
            if (Language.Rus)
                PhotonNetwork.NickName = "Игрок " + Random.Range(100, 999);
            else
                PhotonNetwork.NickName = "Player " + Random.Range(100, 999);
        }

        btnConnect.onClick.AddListener(JoinRoom);

        //PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.NickName);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "1";

            PhotonNetwork.SerializationRate = 30;
            PhotonNetwork.SendRate = 60;

            PhotonNetwork.ConnectUsingSettings();
            // ________________________________________________________
            roomProps = new ExitGames.Client.Photon.Hashtable();

            roomProps["idi_naxyi"] = PhotonNetwork.GameVersion;

            
            btnConnect.gameObject.SetActive(false);
        }
        else
        {
            //OnApplicationPause(false);
            btnConnect.gameObject.SetActive(true);
        }

        var ebat = FindObjectOfType<Advertising>();
        ebat.onVideoClosed += () => PhotonNetwork.ConnectUsingSettings();

        PhotonNetwork.KeepAliveInBackground = 180;

        Log(disconectMsg);
    }

    public static string GetNickname()
    {
        return PhotonNetwork.NickName;
    }

    public static void SetNick(string value)
    {
        PhotonNetwork.NickName = value;
    }

    public override void OnConnectedToMaster()
    {
        Log("Некий хуежуй: " + PhotonNetwork.NickName + " присоеденился к пиздатой игруле");
        onPhotonConnection?.Invoke(PhotonNetwork.NickName);
        btnConnect.gameObject.SetActive(true);
    }

    public override void OnConnected()
    {
        Log("шо блять?");
    }

    void CreateRoom()
    {  
        string[] props = new string[1];
        props[0] = "idi_naxyi";

        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            MaxPlayers = maxPlayers,
            CleanupCacheOnLeave = false,
            IsOpen = true,
            IsVisible = true,
            //CustomRoomProperties = roomProps,
            //CustomRoomPropertiesForLobby = props
        });
    }

    public void JoinRoom()
    {
        btnLabel.text = Language.Rus ? "Поиск игры.." : "Game searching..";

        print(PhotonNetwork.CountOfRooms);
        print(PhotonNetwork.CurrentRoom);
        
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinRandomRoom(roomProps, maxPlayers);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
        print("Не получилось заджойнитья");
    }

    public override void OnJoinedRoom()
    {
        print("Припиздяшил, Уебок " + PhotonNetwork.NickName);

        float waitTime = 0;

        StartCoroutine(PlayersWaiting());

        IEnumerator PlayersWaiting()
        {
            while(waitTime < 3)
            {
                yield return null;

                waitTime += Time.deltaTime;

                if (waitTime % 1.1f > 0.55f)
                    btnLabel.text = Language.Rus ? "Поиск игры.." : "Game searching..";
                else
                    btnLabel.text = Language.Rus ? "Поиск игры." : "Game searching.";


                if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    break;
                    //PhotonNetwork.LoadLevel(sceneNameToload);
                }
            }

            
            PhotonNetwork.LoadLevel(sceneNameToload);
        }
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        Log(cause.ToString());
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            StartCoroutine(Connect());
        }

        IEnumerator Connect()
        {
            yield return new WaitForSeconds(0.3f);

            PhotonNetwork.ConnectUsingSettings();
        }
    }


    private void Update()
    {
//#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
        }
//#endif
    }
    void Log(string msg)
    {
        logText.text += "\n";
        logText.text += msg;
    }

    
}
