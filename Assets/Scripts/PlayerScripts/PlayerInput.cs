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
            var closestPlayer = player.FindClosestPlayer(player.gameObject, true);
            if (!closestPlayer)
            {
                return;
            }

            RpcReportBody(closestPlayer);
        }

        [ClientRpc]
        private void RpcReportBody(Player playerKilled)
        {
            StartCoroutine(StartReportedBody(playerKilled));
        }

        private IEnumerator StartReportedBody(Player playerKilled)
        {
            Debug.Log("Show UI for reporting body!!!");
            yield return new WaitForSeconds(1.5f);
            EmergencyMeeting.Instance.PlayerReporting = player;
            EmergencyMeeting.Instance.PlayerKilled = playerKilled;
            
            EmergencyMeeting.Instance.ToggleMeeting(false);
        }
    }
}