using Game;
using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerGame : NetworkBehaviour
    {
        [Header("Player")]
        private Player _player;
        
        private void Awake()
        {
            _player = GetComponent<Player>();
        }
        
        [Command]
        internal void CmdReadyForMeeting(bool isReady)
        {
            if (!_player)
            {
                Debug.LogWarning("Player not found");
                return;
            }
            
            Debug.Log("Setting ready for meeting to " + isReady + " old value: " + _player.IsReadyForMeeting);
            _player.IsReadyForMeeting = isReady;
        }
        
        [Command]
        internal void CmdReadyForGame(bool isReady, NetworkConnectionToClient sender = null)
        {
            if (!LobbyManager.Instance.Host || sender == null)
            {
                Debug.LogWarning("Host not found");
                return;
            }
            
            
            var isHost = LobbyManager.Instance.Host.netId == sender.identity.netId;
            LobbyManager.Instance.ReadyUp(isHost, isReady);
        }
    }
}