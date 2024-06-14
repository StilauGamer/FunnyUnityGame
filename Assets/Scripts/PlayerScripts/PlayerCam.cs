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
        
        private Camera _camera;
        private float _xRotation;
        private float _yRotation;

        private void Start()
        {
            _camera = Camera.main;
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
                _camera = Camera.main;
            }
        }
        
        
        [TargetRpc]
        public void RpcResetRotation()
        {
            _xRotation = 0;
            _yRotation = 0;
            
            if (_camera)
            {
                _camera.transform.rotation = Quaternion.Euler(0, 0, 0);
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
            if (!isLocalPlayer || !CanTurn || !_camera || GameManager.Instance.IsMeetingActive())
            {
                return;
            }
            
            _camera.transform.position = transform.position;
            
            var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
            var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;
            
            _yRotation += mouseX;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            
            _camera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            playerObject.rotation = Quaternion.Euler(0, _yRotation, 0);
            playerHeadBone.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
        
        
        [Command]
        public void CmdSetCanTurn(bool canTurn)
        {
            CanTurn = canTurn;
        }
    }
}
