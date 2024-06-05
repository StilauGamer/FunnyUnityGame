using System;
using System.Collections;
using UnityEngine;

namespace Music
{
    public class MusicPlayer : MonoBehaviour
    {
        private bool _playing;
        private AudioSource _audioSource;
        
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnMouseOver()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleMusic();
            }
        }


        public bool IsPlaying()
        {
            return _playing;
        }

        public void ToggleMusic()
        {
            if (_playing)
            {
                _playing = false;
                _audioSource.Stop();
                
                StartCoroutine(MoveButtonUp());
                return;
            }
            
            
            _playing = true;
            _audioSource.Play();
            
            StartCoroutine(MoveButtonDown());
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
