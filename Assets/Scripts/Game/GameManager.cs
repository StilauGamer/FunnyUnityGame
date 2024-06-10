using System.Collections;
using System.Linq;
using Mirror;
using PlayerScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public GameObject canvas;
        
        [SyncVar]
        private bool _meetingActive;
        
        [SyncVar]
        internal uint? PlayerReportingNetId;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(canvas);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsMeetingActive()
        {
            return _meetingActive;
        }
        
        
        
        [Command(requiresAuthority = false)]
        public void ToggleMeeting(bool startMeeting, uint? reporter)
        {
            if (startMeeting)
            {
                StartMeeting();

                PlayerReportingNetId = reporter;
            }
            else
            {
                EndMeeting();
                
                PlayerReportingNetId = reporter;
            }
        }

        [Server]
        private void StartMeeting()
        {
            _meetingActive = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("EmergencyScene");
        }


        [Server]
        private void EndMeeting()
        {
            _meetingActive = false;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Scene loaded: " + scene.name + " - Server");
            
            if ((!_meetingActive || scene.name != "EmergencyScene") && (_meetingActive || scene.name != "GameScene"))
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
                var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
                return players.Length == NetworkServer.connections.Count &&
                       players.All(p => p.IsReadyForMeeting);
            });

            var players = LobbyManager.Instance.GetAllPlayers();
            foreach (var player in players)
            {
                player.StartMeeting(players.ToList());
            }
        }
    }
}