using UnityEngine;

namespace Player
{
    public class PlayerCam : MonoBehaviour
    {
        public GameObject player;
        public float cameraSpeed = 5f;
        private float _localRotationX;
    
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");

            player.transform.Rotate(0, mouseX * cameraSpeed, 0);
        
            _localRotationX += -mouseY * cameraSpeed;
            _localRotationX = Mathf.Clamp(_localRotationX, -90f, 90f);
        
        
            transform.localRotation = Quaternion.Euler(_localRotationX, transform.localEulerAngles.y, 0);
        }
    }
}
