using System.Collections;
using System.Linq;
using Mirror;
using PlayerScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emergency
{
    public class EmergencyMeeting : NetworkBehaviour
    {
        public static EmergencyMeeting instance;
        
        [SyncVar]
        private bool _meetingActive;
        
        [SyncVar]
        internal int PlayerReporting;
        
        [SyncVar]
        internal int PlayerKilled;

        public override void OnStartServer()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        
        
        [Command(requiresAuthority = false)]
        public void ToggleMeeting(bool meetingActive, NetworkConnectionToClient reporter = null, NetworkConnectionToClient killed = null)
        {
            if (!meetingActive)
            {
                StartMeeting();
                
                PlayerReporting = reporter?.connectionId ?? -1;
                PlayerKilled = killed?.connectionId ?? -1;
            }
            else
            {
                EndMeeting();
                
                PlayerReporting = -1;
                PlayerKilled = -1;
            }
        }

        [Server]
        public void StartMeeting()
        {
            _meetingActive = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("LobbyScene");
        }


        [Server]
        public void EndMeeting()
        {
            _meetingActive = false;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (isServer)
            {
                Debug.Log("Scene loaded: " + scene.name);
            }
            
            if ((!_meetingActive || scene.name != "LobbyScene") && (_meetingActive || scene.name != "GameScene"))
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            StartCoroutine(UpdateAllUIs());
        }

        private IEnumerator UpdateAllUIs()
        {
            yield return new WaitUntil(() =>
            {
                var players = FindObjectsOfType<Player>();
                Debug.Log("Players: " + players.Length);
                return players.Length == NetworkServer.connections.Count;
            });
            
            // Have to figure out a better way to do this...
            // I have to wait for the players to be spawned before updating the UI
            // Otherwise, the UI won't be updated, or it will be updated with the wrong data
            yield return new WaitForSeconds(1f);
            
            var players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                player.playerUI.CmdUpdateUI();
            }
        }
    }
}