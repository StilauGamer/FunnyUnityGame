using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class NetworkedPlayerCamera : NetworkBehaviour
    {
        private Camera _camera;
        private float _localRotationX;

        void Awake()
        {
            _camera = Camera.main;
        }

        public override void OnStartLocalPlayer()
        {
            if (_camera == null)
            {
                return;
            }

            _camera.orthographic = false;
            _camera.transform.SetParent(transform);
            _camera.transform.position = transform.position + new Vector3(0, 0.6f, 0);
            _camera.transform.rotation = transform.rotation;
        }

        public override void OnStopClient()
        {
            if (!isLocalPlayer || !_camera)
            {
                return;
            }

            _camera.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(_camera.gameObject, SceneManager.GetActiveScene());

            _camera.orthographic = true;
            _camera.transform.position = new Vector3(0, 0, -10);
        }

        void Update()
        {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");

            transform.Rotate(0, mouseX * 5f, 0);
        }
    }
}
