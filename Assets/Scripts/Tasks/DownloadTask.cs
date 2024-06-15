using System.Collections;
using PlayerScripts;
using UnityEngine;

namespace Tasks
{
    public class DownloadTask : TaskInteractable
    {
        public GameObject taskUI;
        public float downloadTimeInSeconds = 5f;
        
        private float _startTime;
        private float _progress;
        
        internal bool DownloadComplete;
        
        private bool _downloadStarted;
        private Player _activePlayer;
        
        public override void Use(Player player)
        {
            if (isTaskCompleted)
            {
                return;
            }
            
            _activePlayer = player;

            _activePlayer.playerCam.SetCanTurn(false);
            _activePlayer.playerCam.ToggleInput(true);
            _activePlayer.playerUI.SendUIEffect(10, taskUI);
            _activePlayer.playerUI.SendUIEffectVisibility(10, true);
            _activePlayer.playerUI.SetupButtonListener(10, "Download:Button", StartDownload);
        }

        public override IEnumerator Complete()
        {
            isTaskCompleted = true;
            DownloadComplete = true;
            
            _activePlayer.playerUI.SendUIEffectText(10, "Progress:Title", "Download Complete!");
            _activePlayer.playerUI.SendUIEffectVisibility(10, "Progress:Text", false);
            yield return new WaitForSeconds(2f);
            
            _activePlayer.playerCam.SetCanTurn(true);
            _activePlayer.playerCam.ToggleInput(false);

            _activePlayer.playerUI.ClearUIEffect(10);
        }

        private void StartDownload()
        {
            if (_downloadStarted)
            {
                return;
            }
            
            _downloadStarted = true;
            _activePlayer.playerUI.SendUIEffectText(10, "Progress:Title", "Downloading...");
            _activePlayer.playerUI.SendUIEffectVisibility(10, "Progress:Text", true);
            _activePlayer.StartCoroutine(UpdateProgress());
        }

        private IEnumerator UpdateProgress()
        {
            _startTime = Time.realtimeSinceStartup;
            while (_progress < 100)
            {
                _progress = (Time.realtimeSinceStartup - _startTime) / downloadTimeInSeconds * 100;
                _activePlayer.playerUI.SendUIEffectText(10, "Progress:Text", $"{_progress:0} %");
                yield return null;
            }
            
            
            _activePlayer.StartCoroutine(Complete());
        }
    }
}