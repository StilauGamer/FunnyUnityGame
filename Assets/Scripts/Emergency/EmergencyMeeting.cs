using Mirror;
using PlayerScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emergency
{
    public class EmergencyMeeting : NetworkBehaviour
    {
        public static EmergencyMeeting Instance;
        
        [SyncVar]
        internal bool MeetingActive;
        
        [SyncVar]
        internal Player PlayerReporting;
        
        [SyncVar]
        internal Player PlayerKilled;

        public override void OnStartServer()
        {
            if (!Instance)
            {
                Instance = this;
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
            MeetingActive = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("LobbyScene");
        }


        [Server]
        public void EndMeeting()
        {
            MeetingActive = false;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (isServer)
            {
                Debug.Log("Scene loaded: " + scene.name);
            }
            
            if ((!MeetingActive || scene.name != "LobbyScene") && (MeetingActive || scene.name != "GameScene"))
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}