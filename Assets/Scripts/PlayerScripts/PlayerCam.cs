using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerCam : NetworkBehaviour
    {
        public float sensitivity = 5f;

        public Transform orientation;
        public Transform playerObject;
        
        private Camera _camera;
        private float _xRotation;
        private float _yRotation;

        public override void OnStartLocalPlayer()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            
            _camera = Camera.main;
            foreach (Transform child in playerObject.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isLocalPlayer)
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
            playerObject.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
    }
}
