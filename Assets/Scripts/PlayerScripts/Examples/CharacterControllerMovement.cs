using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class CharacterControllerMovement : NetworkBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 6f;
        public float sprintSpeed = 8f;

        [Header("Jumping")]
        public float jumpHeight = 3f;
        
        [Header("Gravity")]
        public float gravity = -19.62f;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        
        [Header("Player")]
        public Transform orientation;
        public Transform groundCheck;
        public NetworkAnimator animator;
        public CharacterController characterController;
        
        
        private bool _isGrounded;
        private float _vertical;
        private float _horizontal;
        private float _currentSpeed;
        private Vector3 _moveDirection;
        private Vector3 _velocity;

        void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            _vertical = Input.GetAxis("Vertical");
            _horizontal = Input.GetAxis("Horizontal");
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            if (_velocity.y < 0 && _isGrounded && Input.GetKey(KeyCode.LeftShift))
            {
                _currentSpeed = sprintSpeed;
            }
            else if (_velocity.y < 0 && _isGrounded)
            {
                _currentSpeed = walkSpeed;
            }
            
            
            Debug.Log("Current Speed: " + _currentSpeed);
            _moveDirection = orientation.right * _horizontal + orientation.forward * _vertical;
            characterController.Move(_moveDirection * walkSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;
            characterController.Move(_velocity * Time.deltaTime);
            
            if ((_horizontal != 0 || _vertical != 0) && _isGrounded)
            {
                animator.animator.SetBool("walk", true);
            }
            else
            {
                animator.animator.SetBool("walk", false);
            }
        }
    }
}