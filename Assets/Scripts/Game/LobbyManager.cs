using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using PlayerScripts;
using PlayerScripts.Models;
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
        public int playersInLobby;
        
        [SyncVar(hook = nameof(OnPlayerReadyChanged))]
        private int _playersReady;
        
        [SyncVar]
        private bool _gameStarted;
        
        [SyncVar]
        [CanBeNull]
        internal Player Host;
        
        
        private bool CanLobbyStart => (float) _playersReady / playersInLobby >= .5f;
        

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

        internal List<Player> GetAllPlayers()
        {
            return FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        }
        
        [Server]
        internal void ReadyUp(bool isHost, bool isReady)
        {
            if (playersInLobby == 1)
            {
                Debug.Log("Starting game with only one player in lobby.");
                StartGame();
                return;
            }
            
            Debug.Log("Server - Ready Up - Host: " + isHost + " - Ready: " + isReady + " - Players in lobby: " + playersInLobby + " - Can lobby start: " + CanLobbyStart);
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
            
            var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                var randomColor = Random.Range(0, 4);
                var color = new List<Color>
                    {blueColor, redColor, whiteColor, greenColor}[randomColor];
                
                
                player.BodyColor = color;
                player.playerCam.ToggleInput(false);
            }
            
            
            _gameStarted = true;
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }
    }
}