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
        public void ToggleMeeting(bool meetingActive)
        {
            if (!meetingActive)
            {
                StartMeeting();
            }
            else
            {
                EndMeeting();
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
        }
    }
}