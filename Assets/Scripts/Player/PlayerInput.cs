using Mirror;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Thanks.Player
{
    public class PlayerInput : NetworkBehaviour, IControllable, IPlayerLookInput, IBoostrapble
    {
        public event Action<bool> InteractFirstButton;
        public event Action<bool> InteractSecondButton;
        public event Action<bool> JumpButton;
        public event Action DashButton;
        public event Action<bool> SprintButton;
        public Vector2 LookDelta => lookDelta;
        public Vector2 Move() => direction;

        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private bool invertY = false;

        private InputSystem_Actions inputActions;
        private Vector2 lookDelta;
        private bool interactFirst;
        private bool interactSecond;
        private bool jump;
        private bool sprint;
        private Vector2 direction;

        void IBoostrapble.BoostrapAwake()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();

            inputActions.Player.Attack.started += OnAttackPerform;
            inputActions.Player.Attack.canceled += OnAttackPerform;
            inputActions.Player.Second.started += OnSecond;
            inputActions.Player.Second.canceled += OnSecond;
            inputActions.Player.Dash.started += OnDash;
            inputActions.Player.Dash.canceled += OnDash;
            inputActions.Player.Jump.started += OnJump;
            inputActions.Player.Jump.canceled += OnJump;
            inputActions.Player.Sprint.started += OnSprint;
            inputActions.Player.Sprint.canceled += OnSprint;
        }

        private void OnDestroy()
        {
            inputActions.Player.Attack.started -= OnAttackPerform;
            inputActions.Player.Attack.canceled -= OnAttackPerform;
            inputActions.Player.Second.started -= OnSecond;
            inputActions.Player.Second.canceled -= OnSecond;
            inputActions.Player.Dash.started -= OnDash;
            inputActions.Player.Dash.canceled -= OnDash;
            inputActions.Player.Jump.started -= OnJump;
            inputActions.Player.Jump.canceled -= OnJump;
            inputActions.Player.Sprint.started -= OnSprint;
            inputActions.Player.Sprint.canceled -= OnSprint;

            inputActions.Disable();
        }

        private void Update()
        {
            if (!isLocalPlayer)
                return;

            lookDelta = new Vector2(inputActions.Player.Look.ReadValue<Vector2>().x, inputActions.Player.Look.ReadValue<Vector2>().y * (invertY ? -1 : 1)) * sensitivity ;
            direction = inputActions.Player.Move.ReadValue<Vector2>();
        }

        private void OnAttackPerform(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer)
                return;

            interactFirst = context.started;
            InteractFirstButton?.Invoke(interactFirst);
        }

        private void OnSecond(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer)
                return;

            interactSecond = context.started;
            InteractSecondButton?.Invoke(interactSecond);
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer)
                return;

            DashButton?.Invoke();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer)
                return;

            jump = context.started;
            JumpButton?.Invoke(jump);
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer)
                return;

            sprint = context.started;
            SprintButton?.Invoke(sprint);
        }
    }
}
