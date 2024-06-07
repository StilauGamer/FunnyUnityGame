using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInput : NetworkBehaviour
    {
        public Player player;
        
        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            UpdateInputs();
        }
        
        
        private void UpdateInputs()
        {
            player.playerMovement.Horizontal = Input.GetAxisRaw("Horizontal");
            player.playerMovement.Vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(KeyCode.V))
            {
                NetworkManager.singleton.ServerChangeScene("LobbyScene");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                NetworkManager.singleton.ServerChangeScene("GameScene");
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                player.KillNearestPlayer();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                player.Respawn();
            }
            
            TryJump();
        }
        
        private void TryJump()
        {
            if (!Input.GetKey(KeyCode.Space) || !player.playerMovement.ReadyToJump || !player.playerMovement.IsGrounded)
            {
                return;
            }

            player.playerMovement.ReadyToJump = false;
            player.playerMovement.Jump();
        }
    }
}