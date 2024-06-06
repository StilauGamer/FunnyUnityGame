using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace Music
{
    public class MusicPlayer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnPlayingChanged))]
        private bool _playing;
        private AudioSource _audioSource;
        
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnMouseOver()
        {
            if (isClient && Input.GetKeyDown(KeyCode.F))
            {
                ToggleMusic();
            }
        }

        [Command(requiresAuthority = false)]
        public void ToggleMusic()
        {
            _playing = !_playing;
        }

        private void OnPlayingChanged(bool oldPlaying, bool newPlaying)
        {
            if (newPlaying)
            {
                _audioSource.Play();
                StartCoroutine(MoveButtonDown());
            }
            else
            {
                _audioSource.Stop();
                StartCoroutine(MoveButtonUp());
            }
        }
        
        

        IEnumerator MoveButtonUp()
        {
            while (gameObject.transform.localPosition.y <= 2.852)
            {
                var currentPos = gameObject.transform.localPosition;
                currentPos.y += 0.0005f;
                
                gameObject.transform.localPosition = currentPos;
                yield return TimeSpan.FromSeconds(0.1);
            }
        }

        IEnumerator MoveButtonDown()
        {
            while (gameObject.transform.localPosition.y >= 2.812)
            {
                var currentPos = gameObject.transform.localPosition;
                currentPos.y -= 0.0005f;

                gameObject.transform.localPosition = currentPos;
                yield return TimeSpan.FromSeconds(0.1);
            }
        }
    }
}
