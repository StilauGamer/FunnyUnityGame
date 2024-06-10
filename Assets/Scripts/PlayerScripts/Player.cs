using System.Collections;
using System.Collections.Generic;
using Game;
using Mirror;
using PlayerScripts.Models;
using UnityEngine;
using Utils;

namespace PlayerScripts
{
    public class Player : NetworkBehaviour
    {
        [Header("Player")]
        public PlayerCam playerCam;
        public PlayerMovement playerMovement;
        public ParticleSystem deathParticles;

        
        [Header("Player UI")]
        public PlayerUI playerUI;
        

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
        
        
        private NetworkIdentity _networkIdentity;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        public override void OnStartClient()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();
            if (!isServer)
            {
                return;
            }

            _networkIdentity.AssignClientAuthority(connectionToClient);
            
            if (!LobbyManager.Instance.Host)
            {
                Debug.Log("Setting host to: " + netId);
                LobbyManager.Instance.Host = this;
            }
            
            Debug.Log("Adding player to lobby");
            LobbyManager.Instance.playersInLobby++;
        }

        public override void OnStopClient()
        {
            if (isServer)
            {
                LobbyManager.Instance.playersInLobby--;
            }
        }

        public override void OnStartLocalPlayer()
        {
            var mogusAlive = ModelUtils.GetModel(gameObject, "mogus_alive");
            var mogusDead = ModelUtils.GetModel(gameObject, "mogus_dead");
            
            if (!mogusAlive || !mogusDead)
            {
                Debug.LogError("Mogus model not found");
                return;
            }
            
            mogusAlive = mogusAlive.transform.Find("Cube.001").gameObject;
            var mogusAliveRenderer = mogusAlive.GetComponent<Renderer>();
            var mogusDeadRenderer = mogusDead.GetComponent<Renderer>();
            mogusAliveRenderer.gameObject.SetActive(false);
            mogusDeadRenderer.gameObject.SetActive(false);
        }

        public void KillNearestPlayer()
        {
            if (IsDead)
            {
                return;
            }
            
            #if UNITY_EDITOR
                CmdKill();
            #else
                CmdKillClosestPlayer();
            #endif
        }
        
        public void Respawn()
        {
            if (!IsDead)
            {
                
                return;
            }
            
            CmdRespawn();
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
        
        private void OnReportBodyChanged(bool _, bool _2)
        {
            if (isLocalPlayer)
            {
                StartCoroutine(ReportBody());
            }
        }

        private void OnPlayerVoteChanged(PlayerVote _, PlayerVote _2)
        {
            if (isLocalPlayer)
            {
                playerUI.HideVoteButtons();
            }
        }
        
        private IEnumerator ReportBody()
        {
            yield return new WaitForSeconds(2.5f);

            GameManager.Instance.ToggleMeeting(true);
        }


        [TargetRpc]
        public void StartMeeting(List<Player> allPlayers)
        {
            playerUI.StartEmergencyUI(allPlayers);
        }
        
        public void ReadyForMeeting(bool isReady)
        {
            CmdReadyForMeeting(isReady);
        }
        
        private void OnDeathChanged(bool _, bool newDead)
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
            
            if (newDead)
            {
                mogusAlive.gameObject.SetActive(false);
                mogusDead.gameObject.SetActive(true);                
                deathParticles.Play();
            }
            else
            {
                mogusAlive.gameObject.SetActive(true);
                mogusDead.gameObject.SetActive(false);
            }
        }

        [TargetRpc]
        private void RpcToggleDeathScreen(bool isDead)
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            playerUI.ToggleDeathScreen(isDead);
        }
        
        [Command]
        public void CmdChangeDisplayName(string newName)
        {
            DisplayName = newName;
        }
        
        [Command(requiresAuthority = false)]
        private void CmdReadyForMeeting(bool isReady)
        {
            IsReadyForMeeting = isReady;
        }
        
        [Command]
        private void CmdRespawn()
        {
            IsDead = false;
            RpcToggleDeathScreen(false);
        }

        [Command]
        private void CmdKill()
        {
            IsDead = true;
            RpcToggleDeathScreen(true);
        }
        
        [Command]
        private void CmdKillClosestPlayer()
        {
            var closestPlayer = FindClosestPlayer(gameObject);
            if (!closestPlayer)
            {
                return;
            }

            closestPlayer.Kill();
        }
        
        [Server]
        private void Kill()
        {
            IsDead = true;
            RpcToggleDeathScreen(true);
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