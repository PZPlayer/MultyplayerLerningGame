using System;
using UnityEngine;

namespace Thanks.Player
{
    public class PlayerMovement : MonoBehaviour, IMovement, IDashable
    {
        public event Action<float> OnMove;
        public event Action OnJump;
        public event Action OnDash;

        [Header("RequiiredComponents")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private MonoBehaviour fovControllerScript;

        [Space(10)]
        [Header("Movement Settings")]

        [SerializeField, Range(0, 300)] private float fovSprint;
        [SerializeField, Range(0, 300)] private float fovChangeSpeed;
        [SerializeField, Min(0)] private float speed;
        [SerializeField, Min(0)] private float runningSpeed;
        [SerializeField, Min(0)] private float speedUpTimer;
        [SerializeField, Min(0)] private float slowDownTimer;

        [Space(10)]
        [Header("Jump Settings")]

        [SerializeField] private float jumpForce;
        [SerializeField] private float gravityValue = 9.81f;
        [SerializeField] private float feetRadius;
        [SerializeField] private float loosePowerAfterRelease = 5;
        [SerializeField] private float booferJumpTimer = 0.08f;
        [SerializeField] private float coolDownBetweenJumps = 0.5f;
        [SerializeField] private Transform feetPoint;
        [SerializeField] private LayerMask jumpLayers;

        private IPlayerLookFOV _fov;

        private float lastTimeJumped;
        private float lastTimeStopped;
        private float lastTimeMoving;
        private float lastTimeGround;

        private Vector3 bodyVelocity;
        private Vector3 curDirection;

        private bool jumpedThisFrame = false;
        private bool isRunning = false;
        public bool isGrounded() => Physics.OverlapSphere(feetPoint.position, feetRadius, jumpLayers).Length != 0;

        private void Start()
        {
            _fov = fovControllerScript as IPlayerLookFOV;
        }

        private void Update()
        {
            Gravity();

            jumpedThisFrame = false;

            characterController.Move(bodyVelocity * Time.deltaTime);
        }

        private void Gravity()
        {
            if (isGrounded())
            {
                lastTimeGround = Time.time;

                if (bodyVelocity.y < 0)
                {
                    bodyVelocity.y = 0;
                }

                return;
            }

            bodyVelocity.y -= gravityValue * Time.deltaTime;
        }

        bool IMovement.Jump()
        {
            if (!jumpedThisFrame && (isGrounded() || Time.time - lastTimeGround < booferJumpTimer) && Time.time - lastTimeJumped > coolDownBetweenJumps)
            {
                OnJump?.Invoke();
                lastTimeGround = 0;
                jumpedThisFrame = true;
                lastTimeJumped = Time.time;
                bodyVelocity.y += jumpForce;
                return true;
            }

            return false;
        }

        void IMovement.SetSprint(bool isSprinting)  
        {
            isRunning = isSprinting;

            if (isSprinting)
            {
                _fov.ChangeFOV(fovSprint, fovChangeSpeed);
            }
            else
            {
                _fov.ResetFOV();
            }
        }

        void IMovement.UnpressedJump()
        {
            bodyVelocity.y -= loosePowerAfterRelease;
        }

        void IMovement.MoveTo(Vector3 direction)
        {
            if (direction == Vector3.zero)
            {
                lastTimeStopped = Time.time;
                curDirection = Vector3.MoveTowards(curDirection, direction, (1f / slowDownTimer) * Time.deltaTime);
            }
            else
            {
                lastTimeMoving = Time.time;
                curDirection = Vector3.MoveTowards(curDirection, direction, (1f / speedUpTimer) * Time.deltaTime);
            }

            if (curDirection != Vector3.zero)
            {
                Vector3 cameraForward = characterController.transform.forward;
                Vector3 cameraRight = characterController.transform.right;

                cameraForward.y = 0;
                cameraRight.y = 0;

                cameraForward.Normalize();
                cameraRight.Normalize();

                Vector3 moveDirection = (cameraForward * curDirection.z) + (cameraRight * curDirection.x);
                Vector3 horizontalVelocity = moveDirection * (isRunning ? runningSpeed : speed);
                OnMove?.Invoke(isRunning ? runningSpeed : speed);
                bodyVelocity = new Vector3(horizontalVelocity.x, bodyVelocity.y, horizontalVelocity.z);
            }
            else
            {
                OnMove?.Invoke(bodyVelocity.x + bodyVelocity.z);
            }
        }


        public void ApplyDashImpulse(Vector3 impulse)
        {
            OnDash?.Invoke();
            Vector3 cameraForward = characterController.transform.forward;
            Vector3 cameraRight = characterController.transform.right;
            Vector3 moveDirection = (cameraForward * impulse.z) + (cameraRight * impulse.x);
            bodyVelocity += moveDirection;
            characterController.Move(bodyVelocity * Time.deltaTime);
        }
    }

}