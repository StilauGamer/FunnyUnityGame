using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 2f;
        public float jumpHeight = 5f;
        private float _stamina = 100;
        private bool _running;
        private bool _tired;
        private bool _isGrounded;
    
        private PlayerUI _playerUI;
        private Rigidbody _rigidbody;
    
        private RaycastHit _ground;


        void Start()
        {
            _playerUI = GetComponent<PlayerUI>();
            _rigidbody = GetComponent<Rigidbody>();
        }
    
        void Update()
        {
            IsGrounded();
            if (Input.GetKeyUp(KeyCode.LeftShift) && _tired)
            {
                _tired = false;
            }
        
            if (Input.GetKey(KeyCode.LeftShift) && _stamina > 1 && !_tired)
            {
                _running = true;
                _stamina -= 5f * Time.deltaTime;

                if (_stamina < 1)
                {
                    _tired = true;
                }
            }
            else
            {
                _running = false;
                if (_stamina < 100)
                {
                    _stamina += 2.5f * Time.deltaTime;
                }
            }

        
            speed = _running ? 5f : 2f;
            _playerUI.UpdateSpeedText(speed);
            _playerUI.UpdateStaminaText(_stamina);
            _playerUI.UpdateTiredText(_tired);
            _playerUI.UpdateJumpText(_stamina > 10f && _isGrounded);
        
        
            if (Input.GetKey(KeyCode.W))
            {
                _rigidbody.MovePosition(transform.position += transform.forward * (speed * Time.deltaTime));
                // transform.position += transform.forward * (speed * Time.deltaTime);
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

            if (Input.GetKeyDown(KeyCode.Space) && _stamina > 10f && _isGrounded)
            {
                _stamina -= 10f;
                _rigidbody.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
            }
        }


        private void IsGrounded()
        {
            Physics.SphereCast(new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down), 0.5f, out _ground, 1.125f);
            _isGrounded = _ground.collider;
        }
    }
}
