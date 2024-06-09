using System.Collections.Generic;
using System.Linq;
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
        
        [SyncVar]
        private bool _gameStarted = false;

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

        internal bool IsGameActive()
        {
            return _gameStarted;
        }

        internal List<Player> GetAllPlayers()
        {
            return FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        }

        internal void StartGame()
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