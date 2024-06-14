using Game;
using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInput : NetworkBehaviour
    {
        private Player _player;
        
        private void Awake()
        {
            _player = GetComponent<Player>();
        }
        
        
        private void Update()
        {
            if (!isLocalPlayer || GameManager.Instance.IsMeetingActive())
            {
                return;
            }
            
            UpdateInputs();
        }
        
        private void UpdateInputs()
        {
            _player.playerMovement.Horizontal = Input.GetAxisRaw("Horizontal");
            _player.playerMovement.Vertical = Input.GetAxisRaw("Vertical");
            
            if (_player.IsDead)
            {
                return;
            }
            

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ReportBody();
            }
            
            if (Input.GetKeyDown(KeyCode.R) && _player.IsImposter)
            {
                _player.KillNearestPlayer();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                _player.Respawn();
            }
            
            TryJump();
        }

        private bool _isHidden;
        
        
        
        private void TryJump()
        {
            if (!Input.GetKey(KeyCode.Space) || !_player.playerMovement.ReadyToJump || !_player.playerMovement.IsGrounded || !_player.playerMovement.canJump)
            {
                return;
            }

            _player.playerMovement.ReadyToJump = false;
            _player.playerMovement.Jump();
        }

        [Command]
        private void ReportBody()
        {
            var closestPlayer = _player.FindClosestPlayer(_player.gameObject, true);
            if (!closestPlayer)
            {
                return;
            }

            _player.IsReporting = true;
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            foreach (var otherPlayer in allPlayers)
            {
                otherPlayer.playerUI.RpcToggleBodyReportedScreen(true);
            }
        }
    }
}