using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Enums;
using JetBrains.Annotations;
using Mirror;
using PlayerScripts;
using UnityEngine;

namespace Game
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance;

        public Color blueColor;
        public Color redColor;
        public Color whiteColor;
        public Color greenColor;
        
        [SyncVar(hook = nameof(OnPlayerInLobbyChanged))]
        internal int PlayersInLobby;
        
        [SyncVar(hook = nameof(OnPlayerReadyChanged))]
        private int _playersReady;
        
        [SyncVar(hook = nameof(OnGameStartedChanged))]
        private bool _gameStarted;
        private bool CanLobbyStart => (float) _playersReady / PlayersInLobby >= .5f;
        
        
        [SyncVar]
        [CanBeNull]
        internal Player Host;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        internal bool HasGameStarted()
        {
            return _gameStarted;
        }
        
        private void OnPlayerReadyChanged(int _, int _2)
        {
            var allPlayers = GetAllPlayers();
            allPlayers.ForEach(player => player.playerUI.UpdateReadyButton(CanLobbyStart));
        }

        private void OnPlayerInLobbyChanged(int _, int _2)
        {
            var allPlayers = GetAllPlayers();
            allPlayers.ForEach(player => player.playerUI.UpdateReadyButton(CanLobbyStart));
        }
        
        private void OnGameStartedChanged(bool _, bool _2)
        {
            Debug.Log("Game started: " + _gameStarted);
            StartCoroutine(ToggleRenderer());
        }
        
        private IEnumerator ToggleRenderer()
        {
            yield return new WaitUntil(() => NetworkServer.active);
            GetAllPlayers().ForEach(p => p.InLobby = !_gameStarted);
        }

        internal List<Player> GetAllPlayers()
        {
            return FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        }
        
        
        [Server]
        internal void ReadyUp(bool isHost, bool isReady)
        {
            if (PlayersInLobby == 1)
            {
                Debug.Log("Starting game with only one player in lobby.");
                StartGame();
                return;
            }
            
            Debug.Log("Server - Ready Up - Host: " + isHost + " - Ready: " + isReady + " - Players in lobby: " + PlayersInLobby + " - Can lobby start: " + CanLobbyStart);
            switch (isReady)
            {
                case true when !isHost:
                    _playersReady++;
                    break;
                
                case false when !isHost:
                    _playersReady--;
                    break;
            }
            
            if (CanLobbyStart && isHost)
            {
                StartGame();
            }
        }

        [Server]
        private void StartGame()
        {
            if (_gameStarted)
            {
                return;
            }

            var players = GetAllPlayers();
            if (players.Count > 8)
            {
                var impostors = players.Count / 3;
                var randomImpostors = Random.Range(1, impostors);
                for (var i = 0; i < randomImpostors; i++)
                {
                    var randomPlayer = Random.Range(0, players.Count);
                    if (players[randomPlayer].IsImposter)
                    {
                        i--;
                        continue;
                    }
                    
                    players[randomPlayer].IsImposter = true;
                }
            }
            else
            {
                var randomImpostor = Random.Range(0, players.Count);
                players[randomImpostor].IsImposter = true;
            }
            
            
            foreach (var player in players)
            {
                var randomColor = Random.Range(0, 4);
                var color = new List<Color>
                    {blueColor, redColor, whiteColor, greenColor}[randomColor];
                
                
                player.BodyColor = color;
                player.playerCam.ToggleInput(false);
                player.playerMovement.RpcSetConstraints(RigidbodyConstraints.FreezeRotation);
            }
            
            
            _gameStarted = true;
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }
        
        [Server]
        internal void EndGame(TeamWon teamWon)
        {
            // Show end game screen
            Debug.Log("Game ended with team: " + teamWon);
            
            NetworkManager.singleton.ServerChangeScene("LobbyScene");
            
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                player.ServerGameEnded();
                player.ServerSetNewTeam("team_crewmate", true);
            }
            
            _gameStarted = false;
            _playersReady = 0;
        }
    }
}