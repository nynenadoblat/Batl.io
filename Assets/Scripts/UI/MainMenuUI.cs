using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BatlIO.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Networking & Lobby")]
        [SerializeField] private LobbyManager lobbyManager;
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_InputField serverIpInput;

        [Header("Play Button")]
        [SerializeField] private Button btnPlay;
        [SerializeField] private TMP_Text btnPlayTitle;
        [SerializeField] private TMP_Text btnPlaySubtitle;
        [SerializeField] private GameObject searchingIndicator;

        [Header("Navigation Buttons")]
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnControls;

        [Header("Settings Modal")]
        [SerializeField] private GameObject settingsModal;
        [SerializeField] private Button btnSettingsClose;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeLabel;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Controls Modal")]
        [SerializeField] private GameObject controlsModal;
        [SerializeField] private Button btnControlsClose;

        [Header("Status & Notifications")]
        [SerializeField] private GameObject statusBanner;
        [SerializeField] private TMP_Text statusText;

        private Coroutine searchingCoroutine;

        private void Awake()
        {
            if (lobbyManager == null)
            {
                lobbyManager = Object.FindAnyObjectByType<LobbyManager>();
            }

            // Nickname
            if (nicknameInput != null)
            {
                string nick = LobbyManager.GetNickname();
                nicknameInput.text = nick;
                nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
                nicknameInput.onEndEdit.AddListener(OnNicknameChanged);
            }

            // Server IP
            if (serverIpInput != null)
            {
                serverIpInput.text = "127.0.0.1";
            }

            // Play button
            if (btnPlay != null)
            {
                btnPlay.onClick.AddListener(OnPlayClicked);
            }

            // Modals
            if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
            if (btnSettingsClose != null) btnSettingsClose.onClick.AddListener(CloseSettings);
            if (btnControls != null) btnControls.onClick.AddListener(OpenControls);
            if (btnControlsClose != null) btnControlsClose.onClick.AddListener(CloseControls);

            // Settings values
            if (volumeSlider != null)
            {
                float savedVolume = PlayerPrefs.GetFloat("master_volume", 1f);
                volumeSlider.value = savedVolume;
                AudioListener.volume = savedVolume;
                UpdateVolumeLabel(savedVolume);
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            // Hide modals initially
            if (settingsModal != null) settingsModal.SetActive(false);
            if (controlsModal != null) controlsModal.SetActive(false);
            if (searchingIndicator != null) searchingIndicator.SetActive(false);

            // Disconnect message
            if (!string.IsNullOrEmpty(LobbyManager.disconectMsg))
            {
                ShowStatus(LobbyManager.disconectMsg);
            }
        }

        private void OnNicknameChanged(string newNick)
        {
            if (string.IsNullOrWhiteSpace(newNick))
            {
                newNick = (Language.Rus ? "Игрок " : "Player ") + Random.Range(100, 999);
            }

            LobbyManager.SetNick(newNick);
        }

        private void OnPlayClicked()
        {
            if (lobbyManager == null) return;

            string targetIp = serverIpInput != null ? serverIpInput.text.Trim() : "127.0.0.1";

            btnPlay.interactable = false;
            if (searchingIndicator != null) searchingIndicator.SetActive(true);

            if (searchingCoroutine != null) StopCoroutine(searchingCoroutine);
            searchingCoroutine = StartCoroutine(AnimateSearching());

            lobbyManager.JoinRoom(targetIp);
        }

        public void ResetPlayButton()
        {
            if (searchingCoroutine != null)
            {
                StopCoroutine(searchingCoroutine);
                searchingCoroutine = null;
            }
            if (searchingIndicator != null) searchingIndicator.SetActive(false);
            if (btnPlay != null) btnPlay.interactable = true;
            if (btnPlayTitle != null) btnPlayTitle.text = Language.Rus ? "ИГРАТЬ" : "PLAY";
            if (btnPlaySubtitle != null) btnPlaySubtitle.text = Language.Rus ? "СЛУЧАЙНЫЙ БОЙ НА АРЕНЕ" : "RANDOM ARENA BATTLE";
        }

        private IEnumerator AnimateSearching()
        {
            string baseTitle = Language.Rus ? "ПОИСК ИГРЫ" : "SEARCHING";
            if (btnPlaySubtitle != null)
            {
                btnPlaySubtitle.text = Language.Rus ? "ПОДКЛЮЧЕНИЕ К АРЕНЕ..." : "CONNECTING TO ARENA...";
            }

            int dots = 0;
            while (true)
            {
                dots = (dots + 1) % 4;
                if (btnPlayTitle != null)
                {
                    btnPlayTitle.text = baseTitle + new string('.', dots);
                }
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }

        public void OpenSettings()
        {
            if (settingsModal != null) settingsModal.SetActive(true);
            if (controlsModal != null) controlsModal.SetActive(false);
        }

        public void CloseSettings()
        {
            if (settingsModal != null) settingsModal.SetActive(false);
        }

        public void OpenControls()
        {
            if (controlsModal != null) controlsModal.SetActive(true);
            if (settingsModal != null) settingsModal.SetActive(false);
        }

        public void CloseControls()
        {
            if (controlsModal != null) controlsModal.SetActive(false);
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("master_volume", value);
            PlayerPrefs.Save();
            UpdateVolumeLabel(value);
        }

        private void UpdateVolumeLabel(float value)
        {
            if (volumeLabel != null)
            {
                volumeLabel.text = Mathf.RoundToInt(value * 100f) + "%";
            }
        }

        private void OnFullscreenChanged(bool isFull)
        {
            Screen.fullScreen = isFull;
        }

        public void ShowStatus(string msg)
        {
            if (statusBanner != null && statusText != null)
            {
                statusText.text = msg;
                statusBanner.SetActive(true);
                StopCoroutine(nameof(HideStatusRoutine));
                StartCoroutine(nameof(HideStatusRoutine));
            }
        }

        private IEnumerator HideStatusRoutine()
        {
            yield return new WaitForSecondsRealtime(4f);
            if (statusBanner != null)
            {
                statusBanner.SetActive(false);
            }
        }
    }
}
