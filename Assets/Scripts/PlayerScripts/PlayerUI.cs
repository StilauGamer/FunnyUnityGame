using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game;
using Game.Models;
using JetBrains.Annotations;
using Mirror;
using PlayerScripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        [SerializeField]
        private GameObject emergencyScreenPrefab;

        [SerializeField]
        private GameObject reportButtonPrefab;

        [SerializeField]
        private GameObject bodyReportedPrefab;
        
        [SerializeField]
        private GameObject imposterScreenPrefab;

        [Header("Lobby UI")]
        [SerializeField]
        private GameObject lobbyUiPrefab;

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


        private readonly Dictionary<short, GameObject> _playerUIEffects = new();
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

            var closestPlayer = _player.FindClosestPlayer(_player.gameObject, true);
            if (!closestPlayer)
            {
                SendUIEffectVisibility(2, false);
                return;
            }

            SendUIEffectVisibility(2, true);
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
                SendUIEffectVisibility(6, false);
            }

            switch (scene)
            {
                case "LobbyScene":
                    Debug.Log("PlayerUI - OnPlayerReady - LobbyScene");
                    _player.playerCam.ToggleInput(true);
                    _player.playerCam.CmdSetCanTurn(false);
                    
                    SendUIEffectVisibility(1, false);
                    SendUIEffectVisibility(3, false);
                    SendUIEffectVisibility(4, false);
                    SendUIEffectVisibility(5, false);
                    SendUIEffectVisibility(6, true);

                    _player.playerGame.CmdReadyForMeeting(false);
                    break;
                
                case "GameScene":
                    Debug.Log("PlayerUI - OnPlayerReady - GameScene");
                    if (_player.IsImposter)
                    {
                        SendUIEffectVisibility(5, true);
                    }
                    
                    _player.playerCam.ToggleInput(false);
                    _player.playerCam.CmdSetCanTurn(true);

                    SendUIEffectVisibility(4, false);

                    _player.playerGame.CmdReadyForMeeting(false);
                    break;
                case "EmergencyScene":
                    Debug.Log("PlayerUI - OnPlayerReady - LobbyScene");
                    _player.playerCam.ToggleInput(true);
                    _player.playerCam.CmdSetCanTurn(false);

                    SendUIEffectVisibility(1, false);
                    SendUIEffectVisibility(3, false);
                    SendUIEffectVisibility(4, true);
                    SendUIEffectVisibility(5, false);

                    _player.playerGame.CmdReadyForMeeting(true);
                    break;
            }
        }



        [TargetRpc]
        internal void RpcToggleBodyReportedScreen(bool isReported)
        {
            SendUIEffectVisibility(3, isReported);
        }

        internal IEnumerator ToggleDeathScreen(bool isDead)
        {
            SendUIEffectVisibility(1, isDead);
            yield return new WaitForSeconds(2f);
            SendUIEffectVisibility(1, false);
        }



        private IEnumerator InitializeUI()
        {
            yield return new WaitForSeconds(.1f);

            SendUIEffect(1, deathScreenPrefab);
            SendUIEffectVisibility(1, false);
            
            SendUIEffect(2, reportButtonPrefab);
            SendUIEffectVisibility(2, false);
            
            SendUIEffect(3, bodyReportedPrefab);
            SendUIEffectVisibility(3, false);
            
            SendUIEffect(4, emergencyScreenPrefab);
            SendUIEffectVisibility(4, false);
            
            SendUIEffect(5, imposterScreenPrefab);
            SendUIEffectVisibility(5, false);

            SendUIEffect(6, lobbyUiPrefab);
            SetupButtonListener(6, "StartButton", OnReadyButtonClicked);
            SetupInputListener(6, "LobbyName", OnLobbyNameChanged);
        }

        private void OnReadyButtonClicked()
        {
            _isReady = !_isReady;
            _player.playerGame.CmdReadyForGame(_isReady);
        }

        internal void UpdateReadyButton(bool canStart)
        {
            if (!LobbyManager.Instance.Host)
            {
                return;
            }

            if (!_player)
            {
                Debug.LogWarning("Player is null");
                return;
            }


            if (LobbyManager.Instance.Host.netId == _player.netId)
            {
                SendUIEffectImage(6, "StartButton", canStart ? startButtonSprite : waitingButtonSprite);
                return;
            }


            SendUIEffectImage(6, "StartButton", _isReady ? unreadyButtonSprite : readyButtonSprite);
        }



        [TargetRpc]
        internal void RpcShowVoteResults(List<VotePlayer> allPlayers)
        {
            foreach (var otherPlayer in allPlayers)
            {
                var boxLocation = _playerMeetingLocations[otherPlayer.NetId];
                SendUIEffectVisibility(4, "PlayerBox" + boxLocation + "_VoteButton", false);
                SendUIEffectVisibility(4, "PlayerBox" + boxLocation + "_Reported", false);
                
                var voteCount = allPlayers.Sum(p => p.TargetNetId == otherPlayer.NetId ? 1 : 0);
                SendUIEffectVisibility(4, "PlayerBox" + boxLocation + "_Voted", voteCount > 0);
                SendUIEffectText(4, "PlayerBox" + boxLocation + "_VoteCount", voteCount.ToString());
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
            
            SendUIEffectVisibility(4, "PlayerBox" + location + "_Voted", true);
        }

        internal void HideVoteButtons()
        {
            var allLocations = _playerMeetingLocations.Values;
            SendUIEffectVisibility(4, "SkipVoteButton", false);
            
            foreach (var location in allLocations)
            {
                SendUIEffectVisibility(4, "PlayerBox" + location + "_VoteButton", false);
            }
        }

        internal void StartEmergencyUI(List<VotePlayer> allPlayers)
        {
            Debug.Log("Starting emergency UI for " + _player.netId);
            SendUIEffectVisibility(4, true);

            
            var currentCount = 0;
            SendUIEffectVisibility(4, "SkipVoteButton", !_player.IsDead);
            SetupButtonListener(4, "SkipVoteButton", () => OnVoteButtonClicked(0, true));


            foreach (var otherPlayer in allPlayers)
            {
                _playerMeetingLocations[otherPlayer.NetId] = currentCount;
                SendUIEffectVisibility(4, "PlayerBox" + currentCount, true);
                
                var newColor = otherPlayer.Color;
                newColor.a = 1f;
                SendUIEffectImageColor<RawImage>(4, "PlayerBox" + currentCount + "_Avatar", newColor);

                SendUIEffectText(4, "PlayerBox" + currentCount + "_Name", otherPlayer.Name + " - " + otherPlayer.NetId);
                SendUIEffectVisibility(4, $"PlayerBox{currentCount}_Dead", otherPlayer.IsDead);
                
                var isVoteButtonActive = otherPlayer.NetId != _player.netId && !otherPlayer.IsDead && !_player.IsDead;
                SendUIEffectVisibility(4, $"PlayerBox{currentCount}_VoteButton", isVoteButtonActive);
                SetupButtonListener(4, $"PlayerBox{currentCount}_VoteButton", () => OnVoteButtonClicked(otherPlayer.NetId, false));
                
                SendUIEffectVisibility(4, $"PlayerBox{currentCount}_Reported", otherPlayer.IsReporting);
                SendUIEffectVisibility(4, $"PlayerBox{currentCount}_Voted", false);
                SendUIEffectVisibility(4, $"PlayerBox{currentCount}_VoteCount", false);

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

        
        
        [CanBeNull]
        internal GameObject GetUIEffect(short key)
        {
            return _playerUIEffects.GetValueOrDefault(key);
        }
        
        internal void SendUIEffect(short key, [CanBeNull] GameObject uiEffect)
        {
            if (key == -1 || !uiEffect)
            {
                return;
            }
            
            var effect = Instantiate(uiEffect, GameManager.Instance.canvas.transform);
            if (_playerUIEffects.TryAdd(key, effect))
            {
                return;
            }

            Debug.LogError("UI Effect already exists with key: " + key);
        }

        internal void ClearUIEffect(short key)
        {
            if (!_playerUIEffects.Remove(key, out var effect))
            {
                return;
            }

            Destroy(effect);
        }

        internal void SendUIEffectVisibility(short key, bool visible)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }
            
            effect.SetActive(visible);
        }
        
        internal void SendUIEffectVisibility(short key, string child, bool visible)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }
            
            uiTransform.gameObject.SetActive(visible);
        }
        
        internal void SendUIEffectText(short key, string child, string text)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }

            var textComponent = uiTransform.GetComponent<TextMeshProUGUI>();
            if (!textComponent)
            {
                return;
            }

            textComponent.text = text;
        }
        
        internal void SendUIEffectImage(short key, string child, Sprite sprite)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }

            var image = uiTransform.GetComponent<Image>();
            if (!image)
            {
                return;
            }

            image.sprite = sprite;
        }
        
        internal void SendUIEffectImageColor<T>(short key, string child, Color color) where T : MaskableGraphic
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }

            var image = uiTransform.GetComponent<T>();
            if (!image)
            {
                return;
            }

            image.color = color;
        }
        
        internal void SetupButtonListener(short key, string child, UnityAction action)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }

            var button = uiTransform.GetComponent<Button>();
            if (!button)
            {
                return;
            }

            button.onClick.AddListener(action);
        }
        
        internal void SetupInputListener(short key, string child, UnityAction<string> action)
        {
            if (!_playerUIEffects.TryGetValue(key, out var effect))
            {
                return;
            }

            var uiTransform = effect.transform.FindChildRecursive(child);
            if (!uiTransform)
            {
                return;
            }

            var inputField = uiTransform.GetComponent<TMP_InputField>();
            if (!inputField)
            {
                return;
            }

            inputField.onValueChanged.AddListener(action);
        }
    }
}