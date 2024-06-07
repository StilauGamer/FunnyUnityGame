using System;
using System.Collections;
using System.Linq;
using Emergency;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        private GameObject _canvas;

        
        private Player _player;
        private NetworkIdentity _networkIdentity;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            _networkIdentity = GetComponent<NetworkIdentity>();
            if (isServer && !connectionToClient.isAuthenticated)
            {
                _networkIdentity.AssignClientAuthority(connectionToClient);
            }
            
            if (!isLocalPlayer)
            {
                return;
            }
            
            _player = GetComponent<Player>();
            StartCoroutine(InitializeUI());
        }

        public override void OnStopClient()
        {
            Destroy(_canvas);
        }

        [Client]
        private IEnumerator InitializeUI()
        {
            yield return new WaitForSeconds(0.1f);
            _canvas = GameObject.Find("Canvas"); 

            var currentScene = SceneManager.GetActiveScene().name;
            Debug.Log("Current Scene: " + currentScene);
            switch (currentScene)
            {
                case "GameScene":
                    Debug.Log("Setting up game UI");
                    SetupGameUI();
                    break;
                
                case "LobbyScene":
                    Debug.Log("Setting up lobby UI");
                    SetupLobbyUI();
                    break;
            }
        }

        [Client]
        private void SetupGameUI()
        {
            if (!_deathScreen)
            {
                _deathScreen = Instantiate(deathScreenPrefab, _canvas.transform);
                _deathScreen.SetActive(false);
            }

            if (!_reportButton)
            {
                _reportButton = Instantiate(reportButtonPrefab, _canvas.transform);
                _reportButton.SetActive(false);
            }

            if (!_bodyReportedScreen)
            {
                _bodyReportedScreen = Instantiate(bodyReportedPrefab, _canvas.transform);
                _bodyReportedScreen.SetActive(false);
            }
        }

        [Client]
        private void SetupLobbyUI()
        {
            if (_emergencyScreen)
            {
                return;
            }
            
            Debug.Log("Setting up emergency screen");
            _emergencyScreen = Instantiate(emergencyScreenPrefab, _canvas.transform);
        }
        
        
        
        public void ToggleBodyReportedScreen(bool isReported)
        {
            if (!_bodyReportedScreen)
            {
                return;
            }
            
            _bodyReportedScreen.SetActive(isReported);
        }

        public void ToggleDeathScreen(bool isDead)
        {
            if (!_deathScreen)
            {
                return;
            }
            
            _deathScreen.SetActive(isDead);
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

        
        [Command(requiresAuthority = false)]
        public void CmdUpdateUI()
        {
            var playerKilledConnectionId = EmergencyMeeting.instance.PlayerKilled;
            if (!NetworkServer.connections.TryGetValue(playerKilledConnectionId, out var playerKilledConnection))
            {
                return;
            }
            
            var playerKilled = playerKilledConnection.identity.GetComponent<Player>();
            if (!playerKilled)
            {
                Debug.LogError("Player not found");
                return;
            }
            
            Debug.Log("Updating UI - Server");
            RpcUpdateUI(playerKilled);
        }
        
        [ClientRpc]
        private void RpcUpdateUI(Player playerKilled)
        {
            Debug.Log("Updating UI - Client");
            if (!_emergencyScreen)
            {
                Debug.LogError("Emergency screen not found");
                return;
            }
            
            var textComponents = _emergencyScreen.GetComponentsInChildren<TMP_Text>();
            foreach (var text in textComponents)
            {
                if (text.name != "EmergencyTitle")
                {
                    continue;
                }
                
                text.text = "Emergency Meeting - " + playerKilled.DisplayName;
            }
        }
    }
}