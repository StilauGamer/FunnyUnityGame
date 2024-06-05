using Mirror;
using UnityEngine;

namespace Player
{
    public class NetworkedPlayer : NetworkBehaviour
    {
        public float speed = 5f;
        
        void HandleMovement()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            

            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
            
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * (speed * Time.deltaTime);
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * (speed * Time.deltaTime);
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * (speed * Time.deltaTime);
            }
        }

        void Update()
        {
            HandleMovement();
        }
    }
}