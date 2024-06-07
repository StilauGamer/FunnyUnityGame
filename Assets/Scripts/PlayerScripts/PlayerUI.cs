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
            
            InitializeUI();
        }

        public override void OnStopClient()
        {
            Destroy(_canvas);
        }

        [Client]
        private void InitializeUI()
        {
            _player = GetComponent<Player>();
            _canvas = GameObject.Find("Canvas"); 

            var currentScene = SceneManager.GetActiveScene().name;
            switch (currentScene)
            {
                case "GameScene":
                    if (_deathScreen)
                    {
                        break;
                    }
                    
                    _deathScreen = Instantiate(deathScreenPrefab, _canvas.transform);
                    _deathScreen.SetActive(false);
                    
                    _reportButton = Instantiate(reportButtonPrefab, _canvas.transform);
                    _reportButton.SetActive(false);
                    break;
                
                case "LobbyScene":
                    if (_emergencyScreen)
                    {
                        break;
                    }
                    
                    _emergencyScreen = Instantiate(emergencyScreenPrefab, _canvas.transform);
                    break;
            }
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
            
            var closestPlayer = _player.FindClosestPlayer(_player.gameObject, true);
            if (!closestPlayer)
            {
                _reportButton.SetActive(false);
                return;
            }

            _reportButton.SetActive(true);
        }
    }
}