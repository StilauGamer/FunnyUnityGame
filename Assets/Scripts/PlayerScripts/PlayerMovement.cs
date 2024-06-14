using System.Collections;
using Game;
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

        public float groundDrag = 6f;

        public float jumpForce = 6f;
        public float jumpCooldown = 0.25f;
        public float airMultiplier = 0.4f;


        [Header("Ground Check")]
        public float playerHeight;

        public LayerMask groundMask;

        [Header("Slope Handling")]
        public float maxSlopeAngle;

        [Header("Player")]
        public Player player;

        public Transform orientation;
        public NetworkAnimator animator;

        [Header("Toggles")]
        public bool canMove = true;

        public bool canJump = true;
        public bool canSprint = true;


        internal float Horizontal;
        internal float Vertical;
        
        [SyncVar]
        internal Quaternion Orientation;

        internal bool ReadyToJump = true;
        internal bool IsGrounded;


        private bool _exitingSlope;
        private float _currentSpeed;
        private RaycastHit _slopeHit;
        private MovementState _currentState;
        private PlayerTerrainType _currentTerrainType;

        private Vector3 _moveDirection;
        private Rigidbody _rigidbody;
        private readonly int _walk;


        private PlayerMovement()
        {
            _walk = Animator.StringToHash("walk");
        }

        public override void OnStartLocalPlayer()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
        }
        
        [TargetRpc]
        internal void RpcSetConstraints(RigidbodyConstraints constraints)
        {
            _rigidbody.constraints = constraints;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            IsGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);
            Orientation = orientation.rotation;

            StateHandler();
            SpeedControl();

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

            if (IsGrounded && Horizontal == 0 && Vertical == 0)
            {
                _currentState = MovementState.Idle;
                return;
            }


            if (Input.GetKey(KeyCode.LeftShift) && canSprint)
            {
                _currentState = MovementState.Sprinting;
                _currentSpeed = sprintSpeed;
            }
            else
            {
                _currentState = MovementState.Walking;
                _currentSpeed = walkSpeed;
            }
        }

        private void MovePlayer()
        {
            if (!canMove || !LobbyManager.Instance.HasGameStarted())
            {
                return;
            }
            

            _moveDirection = orientation.forward * Vertical + orientation.right * Horizontal;
            if (animator)
            {
                animator.animator.SetBool(_walk, _currentState != MovementState.Idle && _currentState != MovementState.InAir);
            }


            if (_currentTerrainType == PlayerTerrainType.Slope && !_exitingSlope)
            {
                _rigidbody.AddForce(GetSlopeMoveDirection() * (_currentSpeed * 20f), ForceMode.Force);

                if (_rigidbody.velocity.y > 0)
                {
                    _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }
            else if (IsGrounded)
            {
                _rigidbody.AddForce(_moveDirection.normalized * (_currentSpeed * 10f), ForceMode.Force);
            }
            else if (!IsGrounded)
            {
                _rigidbody.AddForce(_moveDirection.normalized * (_currentSpeed * 10f * airMultiplier), ForceMode.Force);
            }

            _rigidbody.useGravity = _currentTerrainType != PlayerTerrainType.Slope;
        }

        private void SpeedControl()
        {
            if (_currentTerrainType == PlayerTerrainType.Slope && _exitingSlope)
            {
                if (_rigidbody.velocity.magnitude <= _currentSpeed)
                {
                    return;
                }

                _rigidbody.velocity = _rigidbody.velocity.normalized * _currentSpeed;
            }
            else
            {
                var flatVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                if (flatVelocity.magnitude <= _currentSpeed)
                {
                    return;
                }

                var limitedVelocity = flatVelocity.normalized * _currentSpeed;
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