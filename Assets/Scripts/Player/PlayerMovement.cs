using UnityEngine;

namespace Thanks.Player
{
    public class PlayerMovement : MonoBehaviour, IMovement
    {
        [Header("Movement Settings")]

        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speed;
        [SerializeField] private float speedUpTimer;
        [SerializeField] private float slowDownTimer;

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

        private float lastTimeJumped;
        private float lastTimeStopped;
        private float lastTimeMoving;
        private float lastTimeGround;

        private Vector3 bodyVelocity;
        private Vector3 curDirection;

        private bool jumpedThisFrame = false;
        private bool isGrounded() => Physics.OverlapSphere(feetPoint.position, feetRadius, jumpLayers).Length != 0;


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

        public bool Jump()
        {
            if (!jumpedThisFrame && (isGrounded() || Time.time - lastTimeGround < booferJumpTimer) && Time.time - lastTimeJumped > coolDownBetweenJumps)
            {
                lastTimeGround = 0;
                jumpedThisFrame = true;
                lastTimeJumped = Time.time;
                bodyVelocity.y += jumpForce;
                return true;
            }

            return false;
        }

        public void UnpressedJump()
        {
            bodyVelocity.y -= loosePowerAfterRelease;
        }

        public void MoveTo(Vector3 direction)
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
                Vector3 horizontalVelocity = moveDirection * speed;
                bodyVelocity = new Vector3(horizontalVelocity.x, bodyVelocity.y, horizontalVelocity.z);
            }
        }
    }

}