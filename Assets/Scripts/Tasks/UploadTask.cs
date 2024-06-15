using System.Collections;
using PlayerScripts;
using UnityEngine;

namespace Tasks
{
    public class UploadTask : TaskInteractable
    {
        public GameObject taskUI;
        public DownloadTask downloadTask;
        public float uploadTimeInSeconds = 5f;

        private float _startTime;
        private float _progress;

        private bool _uploadStarted;
        private Player _activePlayer;
        
        public override void Use(Player player)
        {
            if (!downloadTask.DownloadComplete || isTaskCompleted)
            {
                return;
            }

            _activePlayer = player;
            
            _activePlayer.playerCam.SetCanTurn(false);
            _activePlayer.playerCam.ToggleInput(true);
            _activePlayer.playerUI.SendUIEffect(10, taskUI);
            _activePlayer.playerUI.SendUIEffectVisibility(10, true);
            _activePlayer.playerUI.SetupButtonListener(10, "Upload:Button", StartUpload);
        }

        public override IEnumerator Complete()
        {
            isTaskCompleted = true;
            Debug.Log("Task completed!");
            
            _activePlayer.playerUI.SendUIEffectText(10, "Progress:Title", "Upload Complete!");
            _activePlayer.playerUI.SendUIEffectVisibility(10, "Progress:Text", false);
            yield return new WaitForSeconds(2f);
            
            _activePlayer.playerCam.SetCanTurn(true);
            _activePlayer.playerCam.ToggleInput(false);

            _activePlayer.playerUI.ClearUIEffect(10);
        }

        private void StartUpload()
        {
            if (_uploadStarted)
            {
                return;
            }

            _uploadStarted = true;
            _activePlayer.playerUI.SendUIEffectText(10, "Progress:Title", "Uploading...");
            _activePlayer.playerUI.SendUIEffectVisibility(10, "Progress:Text", true);
            _activePlayer.StartCoroutine(UpdateProgress());
        }

        private IEnumerator UpdateProgress()
        {
            _startTime = Time.realtimeSinceStartup;
            while (_progress < 100)
            {
                _progress = (Time.realtimeSinceStartup - _startTime) / uploadTimeInSeconds * 100;
                _activePlayer.playerUI.SendUIEffectText(10, "Progress:Text", $"{_progress:0} %");
                yield return null;
            }
            
            
            _activePlayer.StartCoroutine(Complete());
        }
    }
}