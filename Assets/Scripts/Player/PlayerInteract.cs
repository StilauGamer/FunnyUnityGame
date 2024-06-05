using System;
using Music;
using UnityEngine;

namespace Player
{
    public class PlayerInteract : MonoBehaviour
    {
        public LayerMask interactableLayer;
        private MusicPlayer _cachedMusicPlayer;

        void Update()
        {
            return;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Physics.Raycast(transform.position, transform.forward, out var raycastHit, 100, interactableLayer);
                if (!raycastHit.collider)
                {
                    return;
                }


                var audioPlayer = raycastHit.collider.transform == _cachedMusicPlayer?.transform
                    ? _cachedMusicPlayer
                    : raycastHit.collider.GetComponent<MusicPlayer>();
                
                if (audioPlayer)
                {
                    audioPlayer.ToggleMusic();
                    _cachedMusicPlayer = audioPlayer;
                }
            }
        }
    }
}