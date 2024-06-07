using System.Collections;
using Mirror;
using PlayerScripts.Enums;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 6f;
        public float sprintSpeed = 8f;

        public float groundDrag;

        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        

        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask groundMask;

        [Header("Slope Handling")]
        public float maxSlopeAngle;
        
        [Header("Player")]
        public Player player;
        public Transform orientation;
        
        
        internal float CurrentSpeed;
        internal float Horizontal;
        internal float Vertical;
        
        internal bool ReadyToJump = true;
        internal bool IsGrounded;


        private MovementState _currentState;
        private PlayerTerrainType _currentTerrainType;
        private RaycastHit _slopeHit;
        private bool _exitingSlope;
        
        private Vector3 _moveDirection;
        private Rigidbody _rigidbody;
        
        
        public override void OnStartLocalPlayer()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            IsGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
            
            SpeedControl();
            StateHandler();

            if (IsGrounded)
            {
                _rigidbody.drag = groundDrag;
            }
            else
            {
                _rigidbody.drag = 0;
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            MovePlayer();
        }

        private void StateHandler()
        {
            _currentTerrainType = IsOnSlope() ? PlayerTerrainType.Slope : PlayerTerrainType.Flat;
            if (!IsGrounded)
            {
                _currentState = MovementState.InAir;
                return;
            }
            
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _currentState = MovementState.Sprinting;
                CurrentSpeed = sprintSpeed;
            }
            else
            {
                _currentState = MovementState.Walking;
                CurrentSpeed = walkSpeed;
            }
        }
        
        private void MovePlayer()
        {
            if (player.IsDead)
            {
                return;
            }
            
            _moveDirection = orientation.forward * Vertical + orientation.right * Horizontal;

            if (_currentTerrainType == PlayerTerrainType.Slope && !_exitingSlope)
            {
                _rigidbody.AddForce(GetSlopeMoveDirection() * (CurrentSpeed * 20f), ForceMode.Force);

                if (_rigidbody.velocity.y > 0)
                {
                    _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }
            else if (IsGrounded)
            {
                _rigidbody.AddForce(_moveDirection.normalized * (CurrentSpeed * 10f), ForceMode.Force);
            }
            else if (!IsGrounded)
            {
                _rigidbody.AddForce(_moveDirection.normalized * (CurrentSpeed * 10f * airMultiplier), ForceMode.Force);
            }

            _rigidbody.useGravity = _currentTerrainType != PlayerTerrainType.Slope;


            // Uncomment this to force stop the player when no input is given
            // if (_horizontal == 0 && _vertical == 0)
            // {
            //     _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
            // }
        }

        private void SpeedControl()
        {
            if (_currentTerrainType == PlayerTerrainType.Slope && _exitingSlope)
            {
                if (_rigidbody.velocity.magnitude <= CurrentSpeed)
                {
                    return;
                }
                
                _rigidbody.velocity = _rigidbody.velocity.normalized * CurrentSpeed;
            }
            else
            {
                var flatVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

                if (flatVelocity.magnitude <= CurrentSpeed)
                {
                    return;
                }

                var limitedVelocity = flatVelocity.normalized * CurrentSpeed;
                _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
            }
        }

        internal void Jump()
        {
            _exitingSlope = true;
            
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            
            _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            StartCoroutine(ResetJump());
        }

        private IEnumerator ResetJump()
        {
            yield return new WaitForSeconds(jumpCooldown);
            ReadyToJump = true;
            _exitingSlope = false;
        }

        private bool IsOnSlope()
        {
            if (!Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
            {
                return false;
            }

            var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
        }
    }
}
