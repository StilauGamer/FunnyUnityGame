using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using PlayerScripts;
using PlayerScripts.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public GameObject canvas;
        public EventSystem eventSystem;
        
        [SyncVar]
        private bool _meetingActive;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(canvas);
                DontDestroyOnLoad(eventSystem);
            }
            else
            {
                Destroy(gameObject);
                Destroy(canvas);
                Destroy(eventSystem);
            }
        }

        public bool IsMeetingActive()
        {
            return _meetingActive;
        }


        [Server]
        internal void UpdateVotingResults()
        {
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            if (!allPlayers.All(p => p.PlayerVote.HasVoted || p.IsDead))
            {
                return;
            }

            allPlayers.ForEach(p => p.playerUI.ShowVoteResults());
        }
        
        
        
        [Command(requiresAuthority = false)]
        public void ToggleMeeting(bool startMeeting)
        {
            if (startMeeting)
            {
                StartMeeting();
            }
            else
            {
                EndMeeting();
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