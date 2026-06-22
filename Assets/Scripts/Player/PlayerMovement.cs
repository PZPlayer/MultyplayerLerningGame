using UnityEngine;
using Mirror;
using System;

namespace Thanks.Player
{
    public class PlayerMovement : NetworkBehaviour, IMovement, IDashable, IBoostrapble
    {
        // ---- События (те же, что были) ----
        public event Action<float> OnMove;
        public event Action OnJump;
        public event Action OnDash;

        // ---- Поля (твои оригинальные) ----
        [Header("Required Components")]
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

        // ---- Приватные поля (были) ----
        private IPlayerLookFOV _fov;
        private float lastTimeJumped;
        private float lastTimeStopped;
        private float lastTimeMoving;
        private float lastTimeGround;
        private Vector3 bodyVelocity;
        private Vector3 curDirection;
        private bool jumpedThisFrame = false;
        private bool isRunning = false;

        // ---- НОВЫЕ ПОЛЯ ДЛЯ СЕТЕВОЙ СИНХРОНИЗАЦИИ ----
        [SyncVar(hook = nameof(OnNetworkSpeedChanged))]
        private float networkSpeed;

        [SyncVar(hook = nameof(OnNetworkIsRunningChanged))]
        private bool networkIsRunning;

        [SyncVar(hook = nameof(OnNetworkGroundedChanged))]
        private bool networkGrounded;

        // ---- Хуки для SyncVar (вызываются на всех клиентах) ----
        private void OnNetworkSpeedChanged(float oldVal, float newVal)
        {
            // Вызываем событие OnMove у всех клиентов (и локального, и удалённых)
            OnMove?.Invoke(newVal);
        }

        private void OnNetworkIsRunningChanged(bool oldVal, bool newVal)
        {
            // Если нужно отдельное событие для бега – можно добавить, но пока не требуется
        }

        private void OnNetworkGroundedChanged(bool oldVal, bool newVal)
        {
            // Можем использовать, если где-то подписываются на grounded
        }

        // ---- Стандартные методы Unity ----
        void IBoostrapble.BoostrapAwake()
        {
            _fov = fovControllerScript as IPlayerLookFOV;
        }

        private void Update()
        {
            // Физика только на локальном игроке
            if (isLocalPlayer)
            {
                Gravity();
                jumpedThisFrame = false;
                characterController.Move(bodyVelocity * Time.deltaTime);

                // Синхронизируем состояние земли
                bool grounded = isGrounded();
                if (grounded != networkGrounded)
                {
                    CmdUpdateGrounded(grounded);
                }
            }
        }

        private void Gravity()
        {
            if (isGrounded())
            {
                lastTimeGround = Time.time;
                if (bodyVelocity.y < 0) bodyVelocity.y = 0;
                return;
            }
            bodyVelocity.y -= gravityValue * Time.deltaTime;
        }

        // ---- Публичные методы (интерфейсы) ----
        public bool isGrounded()
        {
            // Используется только локально, но мы уже синхронизируем через SyncVar
            return Physics.OverlapSphere(feetPoint.position, feetRadius, jumpLayers).Length != 0;
        }

        // ---- Реализация IMovement ----
        bool IMovement.Jump()
        {
            if (!isLocalPlayer) return false;

            if (!jumpedThisFrame && (isGrounded() || Time.time - lastTimeGround < booferJumpTimer)
                && Time.time - lastTimeJumped > coolDownBetweenJumps)
            {
                OnJump?.Invoke(); // локальный вызов
                CmdJump();        // отправить на сервер, чтобы разослать RPC
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
            if (!isLocalPlayer) return;

            isRunning = isSprinting;

            // Обновляем FOV локально
            if (isSprinting)
                _fov.ChangeFOV(fovSprint, fovChangeSpeed);
            else
                _fov.ResetFOV(); 

            // Отправляем состояние бега на сервер
            CmdUpdateRunning(isRunning);
        }

        void IMovement.UnpressedJump()
        {
            if (!isLocalPlayer) return;
            bodyVelocity.y -= loosePowerAfterRelease;
        }

        void IMovement.MoveTo(Vector3 direction)
        {
            if (!isLocalPlayer) return;

            // Обновляем curDirection
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

            // Вычисляем горизонтальную скорость
            if (curDirection != Vector3.zero)
            {
                Vector3 cameraForward = characterController.transform.forward;
                Vector3 cameraRight = characterController.transform.right;
                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();

                Vector3 moveDirection = (cameraForward * curDirection.z) + (cameraRight * curDirection.x);
                float finalSpeed = isRunning ? runningSpeed : speed;
                Vector3 horizontalVelocity = moveDirection * finalSpeed;
                bodyVelocity = new Vector3(horizontalVelocity.x, bodyVelocity.y, horizontalVelocity.z);

                // Отправляем скорость на сервер (синхронизация)
                CmdUpdateSpeed(finalSpeed);
            }
            else
            {
                // Если стоим, отправляем скорость 0
                CmdUpdateSpeed(0f);
            }
        }

        // ---- Реализация IDashable ----
        public void ApplyDashImpulse(Vector3 impulse)
        {
            if (!isLocalPlayer) return;

            OnDash?.Invoke(); // локальный вызов
            CmdDash();        // разослать всем

            Vector3 cameraForward = characterController.transform.forward;
            Vector3 cameraRight = characterController.transform.right;
            Vector3 moveDirection = (cameraForward * impulse.z) + (cameraRight * impulse.x);
            bodyVelocity += moveDirection;
            characterController.Move(bodyVelocity * Time.deltaTime);
        }

        // ---- Команды (вызываются с клиента, выполняются на сервере) ----
        [Command]
        private void CmdUpdateSpeed(float speedVal)
        {
            networkSpeed = speedVal;
        }

        [Command]
        private void CmdUpdateRunning(bool running)
        {
            networkIsRunning = running;
        }

        [Command]
        private void CmdUpdateGrounded(bool grounded)
        {
            networkGrounded = grounded;
        }

        [Command]
        private void CmdJump()
        {
            RpcJump();
        }

        [Command]
        private void CmdDash()
        {
            RpcDash();
        }

        // ---- ClientRpc (выполняются на всех клиентах) ----
        [ClientRpc]
        private void RpcJump()
        {
            // Вызываем событие OnJump на всех клиентах (включая удалённых)
            OnJump?.Invoke();
        }

        [ClientRpc]
        private void RpcDash()
        {
            OnDash?.Invoke();
        }

        // ---- Важно: не отключаем скрипт на не-локальных ----
        public override void OnStartLocalPlayer()
        {
            // Ничего не делаем, чтобы скрипт оставался включённым везде
        }
    }
}