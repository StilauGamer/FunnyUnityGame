﻿using System.Collections;
using System.Linq;
using Emergency;
using Game;
using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInput : NetworkBehaviour
    {
        public Player player;
        
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

            player.IsReporting = true;
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            foreach (var otherPlayer in allPlayers)
            {
                otherPlayer.playerUI.ToggleBodyReportedScreen(true);
            }
        }
    }
}