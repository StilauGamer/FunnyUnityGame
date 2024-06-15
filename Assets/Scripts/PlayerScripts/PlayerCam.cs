using Game;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayerScripts
{
    public class PlayerCam : NetworkBehaviour
    {
        public float sensitivity = 5f;

        public Transform orientation;
        public Transform playerObject;
        public Transform playerHeadBone;

        [SyncVar]
        internal bool CanTurn;
        internal Camera Camera;
        
        private float _xRotation;
        private float _yRotation;

        private void Start()
        {
            Camera = Camera.main;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (newScene.name != "LobbyScene")
            {
                Camera = Camera.main;
            }
        }
        
        
        [TargetRpc]
        public void RpcResetRotation()
        {
            _xRotation = 0;
            _yRotation = 0;
            
            if (Camera)
            {
                Camera.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            
            orientation.rotation = Quaternion.Euler(0, 0, 0);
            playerObject.rotation = Quaternion.Euler(0, 0, 0);
            playerHeadBone.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        public void ToggleInput(bool active)
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            Cursor.visible = active;
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!isLocalPlayer || !CanTurn || !Camera || GameManager.Instance.IsMeetingActive())
            {
                return;
            }
            
            Camera.transform.position = transform.position;
            
            var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
            var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;
            
            _yRotation += mouseX;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            
            Camera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            playerObject.rotation = Quaternion.Euler(0, _yRotation, 0);
            playerHeadBone.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
        
        
        public void SetCanTurn(bool canTurn)
        {
            CanTurn = canTurn;
        }
        
        [Command]
        public void CmdSetCanTurn(bool canTurn)
        {
            CanTurn = canTurn;
        }
    }
}
