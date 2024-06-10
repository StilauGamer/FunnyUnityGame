using System.Collections;
using System.Collections.Generic;
using Game;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace PlayerScripts
{
    public class PlayerUI : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private GameObject deathScreenPrefab;
        private GameObject _deathScreen;
        
        [SerializeField]
        private GameObject emergencyScreenPrefab;
        private GameObject _emergencyScreen;
        
        [SerializeField]
        private GameObject reportButtonPrefab;
        private GameObject _reportButton;

        [SerializeField]
        private GameObject bodyReportedPrefab;
        private GameObject _bodyReportedScreen;

        [Header("Lobby UI")]
        [SerializeField]
        private GameObject lobbyUiPrefab;
        private GameObject _lobbyUi;
        private Image _lobbyStartButtonImage;

        [SerializeField]
        private Sprite readyButtonSprite;
        [SerializeField]
        private Sprite unreadyButtonSprite;
        [SerializeField]
        private Sprite startButtonSprite;
        [SerializeField]
        private Sprite waitingButtonSprite;
        
        
        [SyncVar]
        private bool _isReady;

        
        private Player _player;
        private NetworkIdentity _networkIdentity;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        
        public override void OnStartLocalPlayer()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            _player = GetComponent<Player>();
            
            Debug.Log("PlayerUI - OnStartLocalPlayer - Player: " + _player.netId);
            StartCoroutine(InitializeUI());
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            Debug.Log("PlayerUI - OnSceneLoaded - Player: " + _player.netId + " - Scene: " + scene.name);
            StartCoroutine(OnPlayerSceneReady(scene.name));
        }

        private IEnumerator OnPlayerSceneReady(string scene)
        {
            yield return new WaitForSeconds(0.1f);

            if (scene != "LobbyScene")
            {
                _lobbyUi.SetActive(false);
            }
            
            switch (scene)
            {
                case "GameScene":
                    Debug.Log("PlayerUI - OnPlayerReady - GameScene");
                    _player.playerCam.ToggleInput(false);
                    
                    _emergencyScreen.SetActive(false);
                    
                    _player.ReadyForMeeting(false);
                    break;
                case "EmergencyScene":
                    Debug.Log("PlayerUI - OnPlayerReady - LobbyScene");
                    _player.playerCam.ToggleInput(true);
                    
                    _deathScreen.SetActive(false);
                    _bodyReportedScreen.SetActive(false);
                    _emergencyScreen.SetActive(true);
                    
                    _player.ReadyForMeeting(true);
                    break;
            }
        }
        
        private void OnLobbyNameChanged(string displayName)
        {
            _player.CmdChangeDisplayName(displayName);
        }

        
        
        [TargetRpc]
        internal void ToggleBodyReportedScreen(bool isReported)
        {
            if (!_bodyReportedScreen)
            {
                Debug.LogWarning("BodyReportedScreen not found");
                return;
            }
            
            Debug.Log("Toggling body reported screen: " + isReported);
            _bodyReportedScreen.SetActive(isReported);
        }

        internal void ToggleDeathScreen(bool isDead)
        {
            if (!_deathScreen)
            {
                return;
            }
            
            _deathScreen.SetActive(isDead);
        }
        
        

        private IEnumerator InitializeUI()
        {
            yield return new WaitForSeconds(.1f);

            if (!_deathScreen)
            {
                _deathScreen = Instantiate(deathScreenPrefab, GameManager.Instance.canvas.transform);
                _deathScreen.SetActive(false);
            }
            
            if (!_reportButton)
            {
                _reportButton = Instantiate(reportButtonPrefab, GameManager.Instance.canvas.transform);
                _reportButton.SetActive(false);
            }
            
            if (!_bodyReportedScreen)
            {
                _bodyReportedScreen = Instantiate(bodyReportedPrefab, GameManager.Instance.canvas.transform);
                _bodyReportedScreen.SetActive(false);
            }
            
            if (!_emergencyScreen)
            {
                _emergencyScreen = Instantiate(emergencyScreenPrefab, GameManager.Instance.canvas.transform);
                _emergencyScreen.SetActive(false);
            }
            
            if (!_lobbyUi)
            {
                _lobbyUi = Instantiate(lobbyUiPrefab, GameManager.Instance.canvas.transform);
                
                
                var lobbyStartButton = ModelUtils.GetModel(_lobbyUi, "StartButton");
                lobbyStartButton.GetComponent<Button>().onClick.AddListener(OnReadyButtonClicked);
                _lobbyStartButtonImage = lobbyStartButton.GetComponent<Image>();
                
                
                var lobbyNameField = ModelUtils.GetModel(_lobbyUi, "LobbyName").GetComponent<TMP_InputField>();
                lobbyNameField.onValueChanged.AddListener(OnLobbyNameChanged);
            }
        }

        private void OnReadyButtonClicked()
        {
            _isReady = !_isReady;
            CmdReadyForGame(_isReady);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdReadyForGame(bool isReady)
        {
            if (!LobbyManager.Instance.Host)
            {
                Debug.LogWarning("Host not found");
                return;
            }
            
            
            var isHost = LobbyManager.Instance.Host.netId == connectionToClient.identity.netId;
            LobbyManager.Instance.ReadyUp(isHost, isReady);
        }
        
        
        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            if (!_reportButton)
            {
                return;
            }
            
            var closestPlayer = _player.FindClosestPlayer(_player.gameObject, true);
            if (!closestPlayer)
            {
                _reportButton.SetActive(false);
                return;
            }

            _reportButton.SetActive(true);
        }

        internal void UpdateReadyButton(bool canStart)
        {
            if (!_lobbyStartButtonImage || !LobbyManager.Instance.Host)
            {
                return;
            }
            
            
            if (LobbyManager.Instance.Host.netId == _player.netId)
            {
                _lobbyStartButtonImage.sprite = canStart ? startButtonSprite : waitingButtonSprite;
                return;
            }
            
            
            _lobbyStartButtonImage.sprite = _isReady ? unreadyButtonSprite : readyButtonSprite;
        }
        
        
        internal void StartEmergencyUI(List<Player> allPlayers)
        {
            Debug.Log("Starting emergency UI for " + _player.netId);
            if (!_emergencyScreen)
            {
                return;
            }
            
            _emergencyScreen.SetActive(true);

            var currentCount = 0;
            
            var playerList = ModelUtils.GetModel(_emergencyScreen, "PlayerList");
            if (!playerList)
            {
                Debug.Log("PlayerList not found");
                return;
            }
            
            foreach (var otherPlayer in allPlayers)
            {
                Debug.Log("Updating UI - Client - Player: " + otherPlayer.netId);
                var playerBox = ModelUtils.GetModel(playerList, "PlayerBox" + currentCount);
                if (!playerBox)
                {
                    Debug.Log("PlayerBox not found");
                    continue;
                }
                
                playerBox.SetActive(true);
                
                
                var playerAvatarBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Avatar");
                if (playerAvatarBox)
                {
                    Debug.Log("Updating UI - Client - Player Avatar: " + otherPlayer.netId);
                    var newColor = otherPlayer.BodyColor;
                    newColor.a = 1f;
                    
                    playerAvatarBox.GetComponent<RawImage>().color = newColor;
                }
                
                var playerNameBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Name");
                if (playerNameBox)
                {
                    Debug.Log("Updating UI - Client - Player Name: " + otherPlayer.DisplayName);
                    playerNameBox.GetComponent<TextMeshProUGUI>().text = otherPlayer.DisplayName + " - " + otherPlayer.netId;
                }
                
                var playerDeadBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Dead");
                if (playerDeadBox)
                {
                    Debug.Log("Updating UI - Client - Player Dead: " + otherPlayer.IsDead);
                    playerDeadBox.SetActive(otherPlayer.IsDead);
                }
                
                currentCount++;
            }
        }
    }
}