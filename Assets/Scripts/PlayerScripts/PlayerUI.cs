using Mirror;
using TMPro;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerUI : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private GameObject deathScreenPrefab;
        private GameObject _deathScreen;
        
        
        private TMP_Text _speedText;
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
            var canvas = GameObject.Find("Canvas"); 
            
            _deathScreen = Instantiate(deathScreenPrefab, canvas.transform);
            _deathScreen.SetActive(false);
        }

        public void ToggleDeathScreen(bool isDead)
        {
            if (!_deathScreen)
            {
                Debug.LogError("Death screen not found");
                return;
            }
            
            _deathScreen.SetActive(isDead);
        }
        
        private void Update()
        {
            if (!isLocalPlayer || !_speedText)
            {
                return;
            }
            
            _speedText.text = $"Speed: {_player.playerMovement.CurrentSpeed}";
        }
    }
}