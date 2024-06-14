using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Enums;
using Game.Models;
using Mirror;
using PlayerScripts;
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
        
        [Header("Dead Players")]
        public GameObject deadPlayerPrefab;
        private readonly List<GameObject> _deadPlayers = new();
        
        [SyncVar]
        private bool _meetingActive;

        
        private void Awake()
        {
            if (!Instance)
            {
                Debug.Log("GameManager created, setting instance");
                
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(canvas);
                DontDestroyOnLoad(eventSystem);
            }
            else
            {
                Debug.LogWarning("GameManager already exists, destroying the new ones");
                
                Destroy(gameObject);
                Destroy(canvas);
                
                var standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (standaloneInputModule)
                {
                    Destroy(standaloneInputModule);
                }
                Destroy(eventSystem);
            }
        }

        public bool IsMeetingActive()
        {
            return _meetingActive;
        }


        [Server]
        internal void UpdateVotingResults(uint senderNetId)
        {
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            allPlayers.ForEach(p => p.playerUI.RpcSetPlayerVoted(senderNetId));
            
            if (!allPlayers.All(p => p.PlayerVote is { HasVoted: true } || p.IsDead))
            {
                return;
            }


            var votedPlayers = allPlayers.Select(p => new VotePlayer(p)).ToList();
            allPlayers.ForEach(p => p.playerUI.RpcShowVoteResults(votedPlayers));
            StartCoroutine(FinishVotingResults());
        }
        
        [Server]
        private IEnumerator FinishVotingResults()
        {
            yield return new WaitForSeconds(2.5f);
            KillMostVotedPlayer();
            
            
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            foreach (var player in allPlayers)
            {
                player.IsReporting = false;
            }
        }

        [Server]
        private void KillMostVotedPlayer()
        {
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            var mostVotedPlayers = allPlayers
                .Where(p => !p.IsDead)
                .GroupBy(p => p.PlayerVote.VotedFor)
                .OrderByDescending(g => g.Count())
                .ToList();
            
            if (mostVotedPlayers.Count == 0 || !mostVotedPlayers[0].Any())
            {
                EndMeeting();
                return;
            }
            
            
            var amountOfPeopleSkipped = allPlayers.Sum(p => p.PlayerVote.IsSkipping ? 1 : 0);
            if (mostVotedPlayers.Count >= 2 &&
                mostVotedPlayers[0].Count() == mostVotedPlayers[1].Count() ||
                mostVotedPlayers[0].Count() == amountOfPeopleSkipped)
            {
                EndMeeting();
                return;
            }

            
            var mostVotedPlayer = mostVotedPlayers[0].FirstOrDefault();
            if (mostVotedPlayer)
            {
                mostVotedPlayer.ServerKill();
                mostVotedPlayer.ServerSetNewTeam("team_dead", false);
            }
            
            EndMeeting();
        }
        
        
        
        [Command(requiresAuthority = false)]
        public void CmdToggleMeeting(bool startMeeting)
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
            if (!LobbyManager.Instance.HasGameStarted())
            {
                return;
            }
            
            _meetingActive = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("EmergencyScene");
        }
        
        [Server]
        private void EndMeeting()
        {
            _meetingActive = false;
            if (!LobbyManager.Instance.HasGameStarted())
            {
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.singleton.ServerChangeScene("GameScene");
            
            
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            allPlayers.ForEach(p => p.PlayerVote.ResetVote());
            
            
            _deadPlayers.ForEach(NetworkServer.Destroy);
            _deadPlayers.Clear();
        }
        
        
        [Server]
        internal TeamWon IsGameOver()
        {
            var allPlayers = LobbyManager.Instance.GetAllPlayers();
            var amountOfImpostors = allPlayers.Count(p => p.IsImposter && !p.IsDead);
            var amountOfCrewmates = allPlayers.Count(p => !p.IsImposter && !p.IsDead);
            
            if (amountOfImpostors > amountOfCrewmates || amountOfImpostors == 1 && amountOfCrewmates == 1)
            {
                return TeamWon.Impostor;
            }
            
            if (amountOfImpostors == 0)
            {
                return TeamWon.Crewmate;
            }
            
            return TeamWon.None;
        }


        [Server]
        internal void KillPlayer(DeadPlayer deadPlayer)
        {
            var deadPlayerObject = Instantiate(deadPlayerPrefab, deadPlayer.Position, deadPlayer.Rotation);
            var deadPlayerScript = deadPlayerObject.GetComponent<PlayerDead>();
            
            
            deadPlayerScript.Color = deadPlayer.Color;
            NetworkServer.Spawn(deadPlayerObject);
            
            deadPlayerScript.RpcDie();
            _deadPlayers.Add(deadPlayerObject);
        }
        
        

        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            Debug.Log("Scene loaded: " + newScene.name + " - Server");
            if (_meetingActive && newScene.name != "EmergencyScene" || !_meetingActive && newScene.name != "GameScene")
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

            var players = LobbyManager.Instance
                .GetAllPlayers()
                .OrderBy(p => p.IsDead)
                .ToList();

            var votePlayers = players.Select(p => new VotePlayer(p)).ToList();
            foreach (var player in players)
            {
                player.RpcStartMeeting(votePlayers);
            }
        }
    }
}