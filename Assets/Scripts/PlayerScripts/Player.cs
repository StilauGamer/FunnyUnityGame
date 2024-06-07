using Mirror;
using UnityEngine;

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
        

        [SyncVar(hook = nameof(OnDeathChanged))]
        internal bool IsDead;
        [SyncVar]
        internal string DisplayName;
        
        
        private NetworkIdentity _networkIdentity;
        
        public override void OnStartLocalPlayer()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();
            if (isServer)
            {
                _networkIdentity.AssignClientAuthority(connectionToClient);
            }
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

        private void OnDeathChanged(bool oldDead, bool newDead)
        {
            var mogusAlive = GetModel(gameObject, "mogus_alive");
            var mogusDead = GetModel(gameObject, "mogus_dead");

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

            return;

            GameObject GetModel(GameObject gameObjectInternal, string nameInternal)
            {
                if (gameObjectInternal.name == nameInternal)
                {
                    return gameObjectInternal;
                }
                
                foreach (Transform child in gameObjectInternal.transform)
                {
                    var model = GetModel(child.gameObject, nameInternal);
                    if (model != null)
                    {
                        return model;
                    }
                }
                
                return null;
            }
        }

        [ClientRpc]
        private void RpcToggleDeathScreen(bool isDead)
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            playerUI.ToggleDeathScreen(isDead);
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
            if (closestPlayer == null)
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