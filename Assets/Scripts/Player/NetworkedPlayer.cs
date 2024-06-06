using System.Numerics;
using Mirror;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Player
{
    public class NetworkedPlayer : NetworkBehaviour
    {
        public float speed = 5f;

        private Collider[] _results;
        private BoxCollider _collider;
        private NetworkTransformReliable _clientRigidbody;
        
        
        void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            _collider = GetComponent<BoxCollider>();
            _clientRigidbody = GetComponent<NetworkTransformReliable>();
        }

        private void FixedUpdate()
        {
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            var forwardDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var movement = forwardDirection * (vertical * speed * Time.fixedDeltaTime);

            var rightDirection = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            movement += rightDirection * (horizontal * speed * Time.fixedDeltaTime);
            
            if (movement != Vector3.zero)
            {
                CmdMove(movement);
            }
        }

        [Command(requiresAuthority = false)]
        void CmdMove(Vector3 direction)
        {
            var newPosition = _clientRigidbody.transform.position + direction;
            if (IsValidMove(newPosition))
            {
                _clientRigidbody.RpcTeleport(newPosition);
            }
        }

        private bool IsValidMove(Vector3 newPosition)
        {
            var extents = _collider.size / 2;
            var radius = Mathf.Max(extents.x, extents.y);
            
            var size = Physics.OverlapSphereNonAlloc(newPosition, radius, _results);
            Debug.Log("Size: " + size);
            return size == 0;
        }
    }
}