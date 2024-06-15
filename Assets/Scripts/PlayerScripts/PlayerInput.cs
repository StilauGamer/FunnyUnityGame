using Game;
using Mirror;
using Tasks;
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
            ListenForInputs();
        }
        
        private void UpdateInputs()
        {
            _player.playerMovement.Horizontal = Input.GetAxisRaw("Horizontal");
            _player.playerMovement.Vertical = Input.GetAxisRaw("Vertical");
        }

        private void ListenForInputs()
        {
            TryJump();
            if (Input.GetKeyDown(KeyCode.E))
            {
                var ray = new Ray(_player.playerCam.Camera.transform.position, _player.playerCam.Camera.transform.forward);
                var interactable = Physics.Raycast(ray, out var hit, 1f)
                    ? hit.collider.GetComponent<Interactable>()
                    : null;
            
                interactable?.Use(_player);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ReportBody();
            }
            
            if (Input.GetKeyDown(KeyCode.R) && _player.IsImposter)
            {
                _player.KillNearestPlayer();
            }
        }

        private bool _isHidden;
        
        
        
        private void TryJump()
        {
            if (!Input.GetKey(KeyCode.Space) ||
                !_player.playerMovement.ReadyToJump ||
                !_player.playerMovement.IsGrounded ||
                !_player.playerMovement.canJump)
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