using System.Collections;
using Emergency;
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

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ReportBody();
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

        [Command]
        private void ReportBody()
        {
            #if UNITY_EDITOR
            var closestPlayer = player;
            #else
            var closestPlayer = player.FindClosestPlayer(player.gameObject, true);
            if (!closestPlayer)
            {
                return;
            }
            #endif

            RpcReportBody(closestPlayer);
        }

        [ClientRpc]
        private void RpcReportBody(Player playerKilled)
        {
            StartCoroutine(StartReportedBody(playerKilled));
        }

        private IEnumerator StartReportedBody(Player playerKilled)
        {
            var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (var otherClient in players)
            {
                otherClient.playerUI.ToggleBodyReportedScreen(true);
                yield return null;
            }
            
            
            yield return new WaitForSeconds(2.5f);
            EmergencyMeeting.instance.ToggleMeeting(false, player.connectionToClient, playerKilled.connectionToClient);
        }
    }
}