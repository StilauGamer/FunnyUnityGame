using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Models;
using Mirror;
using PlayerScripts.Models;
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
        
        [SerializeField]
        private GameObject imposterScreenPrefab;
        private GameObject _imposterScreen;

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


        private readonly Dictionary<uint, int> _playerMeetingLocations = new();
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

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (!_player || _player.IsDead || GameManager.Instance.IsMeetingActive())
            {
                return;
            }

            if (_reportButton)
            {
                var closestPlayer = _player.FindClosestPlayer(_player.gameObject, true);
                if (!closestPlayer)
                {
                    _reportButton.SetActive(false);
                    return;
                }

                _reportButton.SetActive(true);
            }
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
                case "LobbyScene":
                    Debug.Log("PlayerUI - OnPlayerReady - LobbyScene");
                    _player.playerCam.ToggleInput(true);
                    _player.playerCam.CmdSetCanTurn(false);
                    
                    _lobbyUi.SetActive(true);
                    _deathScreen.SetActive(false);
                    _bodyReportedScreen.SetActive(false);
                    _emergencyScreen.SetActive(false);
                    _imposterScreen.SetActive(false);

                    _player.playerGame.CmdReadyForMeeting(false);
                    break;
                
                case "GameScene":
                    Debug.Log("PlayerUI - OnPlayerReady - GameScene");
                    if (_player.IsImposter)
                    {
                        _imposterScreen.SetActive(true);
                    }
                    
                    _player.playerCam.ToggleInput(false);
                    _player.playerCam.CmdSetCanTurn(true);

                    _emergencyScreen.SetActive(false);

                    _player.playerGame.CmdReadyForMeeting(false);
                    break;
                case "EmergencyScene":
                    Debug.Log("PlayerUI - OnPlayerReady - LobbyScene");
                    _player.playerCam.ToggleInput(true);
                    _player.playerCam.CmdSetCanTurn(false);

                    _deathScreen.SetActive(false);
                    _bodyReportedScreen.SetActive(false);
                    _imposterScreen.SetActive(false);
                    _emergencyScreen.SetActive(true);

                    _player.playerGame.CmdReadyForMeeting(true);
                    break;
            }
        }



        [TargetRpc]
        internal void RpcToggleBodyReportedScreen(bool isReported)
        {
            if (!_bodyReportedScreen)
            {
                return;
            }

            _bodyReportedScreen.SetActive(isReported);
        }

        internal IEnumerator ToggleDeathScreen(bool isDead)
        {
            if (!_deathScreen)
            {
                yield break;
            }
            

            _deathScreen.SetActive(isDead);
            if (!isDead)
            {
                yield break;
            }

            yield return new WaitForSeconds(2);
            _deathScreen.SetActive(false);
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
            
            if (!_imposterScreen)
            {
                _imposterScreen = Instantiate(imposterScreenPrefab, GameManager.Instance.canvas.transform);
                _imposterScreen.SetActive(_player.IsImposter);
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
            _player.playerGame.CmdReadyForGame(_isReady);
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



        [TargetRpc]
        internal void RpcShowVoteResults(List<VotePlayer> allPlayers)
        {
            foreach (var otherPlayer in allPlayers)
            {
                var boxLocation = _playerMeetingLocations[otherPlayer.NetId];
                var playerBox = ModelUtils.GetModel(_emergencyScreen, "PlayerBox" + boxLocation);

                var playerVoteButton = ModelUtils.GetModel(playerBox, $"PlayerBox{boxLocation}_VoteButton");
                if (playerVoteButton)
                {
                    playerVoteButton.SetActive(false);
                }

                var playerReporterBox = ModelUtils.GetModel(playerBox, $"PlayerBox{boxLocation}_Reporter");
                if (playerReporterBox)
                {
                    playerReporterBox.SetActive(false);
                }

                var playerVoteCount = ModelUtils.GetModel(playerBox, $"PlayerBox{boxLocation}_VoteCount");
                if (!playerVoteCount)
                {
                    continue;
                }
                


                var playerVoteCountText = playerVoteCount.GetComponent<TextMeshProUGUI>();
                var voteCount = allPlayers.Sum(p => p.TargetNetId == otherPlayer.NetId ? 1 : 0);

                
                playerVoteCount.SetActive(true);
                playerVoteCountText.text = voteCount.ToString();
            }
        }

        [TargetRpc]
        internal void RpcSetPlayerVoted(uint playerVoted)
        {
            Debug.Log("Player just voted: " + playerVoted);
            if (!_playerMeetingLocations.TryGetValue(playerVoted, out var location))
            {
                Debug.LogWarning("Player not found with netId: " + playerVoted + " - Location: " + location);
                return;
            }

            var playerBox = ModelUtils.GetModel(_emergencyScreen, "PlayerBox" + location);
            if (!playerBox)
            {
                return;
            }

            var playerVotedBox = ModelUtils.GetModel(playerBox, $"PlayerBox{location}_Voted");
            if (playerVotedBox)
            {
                playerVotedBox.SetActive(true);
            }
        }

        internal void HideVoteButtons()
        {
            var allLocations = _playerMeetingLocations.Values;
            var skipButton = ModelUtils.GetModel(_emergencyScreen, "SkipVoteButton");
            if (skipButton)
            {
                skipButton.SetActive(false);
            }
            
            foreach (var location in allLocations)
            {
                var playerBox = ModelUtils.GetModel(_emergencyScreen, "PlayerBox" + location);
                if (!playerBox)
                {
                    continue;
                }

                var playerVoteButton = ModelUtils.GetModel(playerBox, $"PlayerBox{location}_VoteButton");
                if (playerVoteButton)
                {
                    playerVoteButton.SetActive(false);
                }
            }
        }

        internal void StartEmergencyUI(List<VotePlayer> allPlayers)
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
                Debug.LogError("PlayerList not found");
                return;
            }

            var skipVoteButton = ModelUtils.GetModel(_emergencyScreen, "SkipVoteButton");
            skipVoteButton.SetActive(!_player.IsDead);
            skipVoteButton.GetComponent<Button>().onClick.AddListener(() => OnVoteButtonClicked(0, true));


            foreach (var otherPlayer in allPlayers)
            {
                _playerMeetingLocations[otherPlayer.NetId] = currentCount;
                var playerBox = ModelUtils.GetModel(playerList, "PlayerBox" + currentCount);
                if (!playerBox)
                {
                    continue;
                }

                playerBox.SetActive(true);


                var playerAvatarBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Avatar");
                if (playerAvatarBox)
                {
                    var newColor = otherPlayer.Color;
                    newColor.a = 1f;

                    playerAvatarBox.GetComponent<RawImage>().color = newColor;
                }

                var playerNameBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Name");
                if (playerNameBox)
                {
                    playerNameBox.GetComponent<TextMeshProUGUI>().text = otherPlayer.Name + " - " + otherPlayer.NetId;
                }

                var playerDeadBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Dead");
                if (playerDeadBox)
                {
                    playerDeadBox.SetActive(otherPlayer.IsDead);
                }

                var playerVoteButton = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_VoteButton");
                if (playerVoteButton)
                {
                    var isButtonActive = otherPlayer.NetId != _player.netId && !otherPlayer.IsDead && !_player.IsDead;
                    playerVoteButton.SetActive(isButtonActive);
                    
                    var button = playerVoteButton.GetComponent<Button>();
                    button.onClick.AddListener(() => OnVoteButtonClicked(otherPlayer.NetId, false));
                }

                var playerReporterBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Reported");
                if (playerReporterBox)
                {
                    playerReporterBox.SetActive(otherPlayer.IsReporting);
                }

                var playerVoteCount = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_VoteCount");
                if (playerVoteCount)
                {
                    playerVoteCount.SetActive(false);
                }
                
                var playerVotedBox = ModelUtils.GetModel(playerBox, $"PlayerBox{currentCount}_Voted");
                if (playerVotedBox)
                {
                    playerVotedBox.SetActive(false);
                }

                currentCount++;
            }
        }



        private void OnVoteButtonClicked(uint votedNetId, bool isSkipping)
        {
            var playerVote = new PlayerVote(true, isSkipping, votedNetId);
            _player.CmdSetPlayerVote(playerVote);
        }

        private void OnLobbyNameChanged(string displayName)
        {
            _player.CmdChangeDisplayName(displayName);
        }
    }
}