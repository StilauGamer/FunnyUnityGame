using Mirror;
using PlayerScripts;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public string playerUsername;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
            
            var playerScript = player.GetComponent<Player>();
            playerScript.DisplayName = playerUsername;
        }
    }
}