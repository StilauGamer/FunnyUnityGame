using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Enums;
using Game.Models;
using Mirror;
using PlayerScripts.Models;
using UnityEngine;
using Utils;

namespace PlayerScripts
{
    public class Player : NetworkBehaviour
    {
        [Header("Player")]
        public PlayerUI playerUI;
        public PlayerCam playerCam;
        public PlayerGame playerGame;
        public PlayerInput playerInput;
        public PlayerTasks playerTasks;
        public PlayerMovement playerMovement;

        public GameObject playerFog;
        public GameObject playerRenderer;
        public NetworkTeam networkTeam;
        public Collider playerWallCollider;
        public Collider playerGroundCollider;

        [Header("Player State")]
        [SyncVar(hook = nameof(OnDeathChanged))]
        internal bool IsDead;
        
        [SyncVar(hook = nameof(OnReportBodyChanged))]
        internal bool IsReporting;
        
        [SyncVar]
        internal string DisplayName;
        
        [SyncVar]
        internal bool IsReadyForMeeting;

        [SyncVar]
        internal bool IsImposter;
        
        [SyncVar(hook = nameof(OnPlayerVoteChanged))]
        internal PlayerVote PlayerVote;
        
        [SyncVar(hook = nameof(OnBodyColorChanged))]
        internal Color BodyColor;
        
        [SyncVar(hook = nameof(OnLobbyChanged))]
        internal bool InLobby;
        
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        
        
        public override void OnStartClient()
        {
            if (!isServer)
            {
                return;
            }

            
            netIdentity.AssignClientAuthority(connectionToClient);
            InLobby = !LobbyManager.Instance.HasGameStarted();
            
            
            if (LobbyManager.Instance && !LobbyManager.Instance.Host)
            {
                Debug.Log("Setting host to: " + netId);
                LobbyManager.Instance.Host = this;
            }
            
            Debug.Log("Adding player to lobby");
            LobbyManager.Instance.PlayersInLobby++;
        }

        public override void OnStopClient()
        {
            if (isServer)
            {
                LobbyManager.Instance.PlayersInLobby--;
            }
        }

        public override void OnStartLocalPlayer()
        {
            playerFog.SetActive(true);
        }
        
        
        
        [Command]
        public void CmdChangeDisplayName(string newName)
        {
            DisplayName = newName;
        }
        
        [Command]
        internal void CmdSetPlayerVote(PlayerVote playerVote, NetworkConnectionToClient sender = null)
        {
            PlayerVote = playerVote;
            GameManager.Instance.UpdateVotingResults(sender?.identity.netId ?? 0);
        }
        
        

        public void KillNearestPlayer()
        {
            if (IsDead)
            {
                return;
            }
            
            CmdKillClosestPlayer();
        }
        
        [Command]
        private void CmdKillClosestPlayer()
        {
            var closestPlayer = FindClosestPlayer(gameObject);
            if (!closestPlayer)
            {
                return;
            }

            var deadPlayer = new DeadPlayer(
                closestPlayer.netIdentity.connectionToClient,
                closestPlayer.transform.position,
                closestPlayer.playerMovement.Orientation,
                closestPlayer.BodyColor
            );
            closestPlayer.ServerKill(deadPlayer);
            closestPlayer.ServerSetNewTeam("team_dead", false);
        }
        
        [Server]
        internal void ServerKill(DeadPlayer? deadPlayer = null)
        {
            Debug.Log("Player: " + netId + " just got killed");
            if (IsDead)
            {
                return;
            }
            
            IsDead = true;
            RpcToggleDeathScreen(true);
            if (deadPlayer.HasValue)
            {
                GameManager.Instance.KillPlayer(deadPlayer.Value);
            }
            
            
            var isGameOver = GameManager.Instance.IsGameOver();
            if (isGameOver != TeamWon.None)
            {
                LobbyManager.Instance.EndGame(isGameOver);
            }
        }
        
        
        
        public void Respawn()
        {
            if (!IsDead)
            {
                
                return;
            }
            
            CmdRespawn();
        }
        
        [Command]
        private void CmdRespawn()
        {
            IsDead = false;
            RpcToggleDeathScreen(false);
        }
        
        
        
        
        
        private void OnDeathChanged(bool _, bool _2)
        {
        }
        
        private void OnBodyColorChanged(Color _, Color newColor)
        {
            var mogusAlive = ModelUtils.GetModel(gameObject, "mogus_alive");
            var mogusDead = ModelUtils.GetModel(gameObject, "mogus_dead");

            if (isLocalPlayer)
            {
                return;
            }
            
            if (!mogusAlive || !mogusDead)
            {
                Debug.LogError("Mogus model not found");
                return;
            }

            mogusAlive = mogusAlive.transform.Find("Cube.001").gameObject;
            var mogusAliveRenderer = mogusAlive.GetComponent<Renderer>();
            var mogusDeadRenderer = mogusDead.GetComponent<Renderer>();
            
            if (!mogusAliveRenderer || !mogusDeadRenderer)
            {
                Debug.LogError("Mogus renderer not found");
                return;
            }
            
            foreach (var material in mogusAliveRenderer.materials)
            {
                Debug.Log("Mogus Alive Material: " + material.name);
                if (material.name == "Body.001 (Instance)")
                {
                    material.color = newColor;
                }
            }
            
            foreach (var material in mogusDeadRenderer.materials)
            {
                Debug.Log("Mogus Dead Material: " + material.name);
                if (material.name == "Body (Instance)")
                {
                    material.color = newColor;
                }
            }
        }
        
        private void OnReportBodyChanged(bool _, bool reporting)
        {
            if (isLocalPlayer && reporting)
            {
                StartCoroutine(ReportBody());
            }
        }

        private void OnPlayerVoteChanged(PlayerVote _, PlayerVote newPlayerVote)
        {
            Debug.Log("Player: " + netId + " just has voted for: " + newPlayerVote.VotedFor);
            if (isLocalPlayer)
            {
                playerUI.HideVoteButtons();
            }
        }
        
        private void OnLobbyChanged(bool _, bool _2)
        {
            if (!isClient)
            {
                return;
            }
            
            if (InLobby)
            {
                Debug.Log("Setting player: " + netId + " to in lobby");
                var rigidBody = GetComponent<Rigidbody>();
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                playerRenderer.SetActive(isLocalPlayer);
                playerGroundCollider.enabled = false;
                playerWallCollider.enabled = false;
            }
            else
            {
                Debug.Log("Setting player: " + netId + " to not in lobby");
                playerRenderer.SetActive(!isLocalPlayer);
                playerGroundCollider.enabled = true;
                playerWallCollider.enabled = true;
            }
        }

        
        
        private IEnumerator ReportBody()
        {
            yield return new WaitForSeconds(2.5f);

            GameManager.Instance.CmdToggleMeeting(true);
        }

        

        [TargetRpc]
        public void RpcStartMeeting(List<VotePlayer> allPlayers)
        {
            playerUI.StartEmergencyUI(allPlayers);
        }

        [Server]
        internal void ServerSetNewTeam(string teamId, bool forceShown)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            networkTeam.teamId = teamId;
            networkTeam.forceShown = forceShown;
        }
        
        
        

        [TargetRpc]
        private void RpcToggleDeathScreen(bool isDead)
        {
            StartCoroutine(playerUI.ToggleDeathScreen(isDead));
        }


        [Server]
        internal void ServerGameEnded()
        {
            IsDead = false;
            IsImposter = false;
            IsReporting = false;
            IsReadyForMeeting = false;
            PlayerVote = new PlayerVote();
            
            playerCam.RpcResetRotation();
            playerMovement.RpcSetConstraints(RigidbodyConstraints.FreezeAll);
            transform.position = new Vector3(0, 1.26f, 0);
        }
        
        
        public Player FindClosestPlayer(GameObject self, bool shouldBeDead = false)
        {
            Player closestPlayer = null;
            var closestDistance = 3f;
            
            var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.gameObject == self || player.IsDead != shouldBeDead)
                {
                    continue;
                }
                
                var distance = Vector3.Distance(player.transform.position, self.transform.position);
                if (distance > closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closestPlayer = player;
            }
            
            return closestPlayer;
        }
    }
}