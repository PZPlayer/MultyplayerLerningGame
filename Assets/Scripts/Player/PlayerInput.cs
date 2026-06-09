using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Thanks.Player
{
    public class PlayerInput : MonoBehaviour, IControllable
    {
        private bool interactFirst;
        private bool interactSecond;
        private bool jump;
        private bool sprint;
        private Vector2 direction;

        public event Action<bool> InteractFirstButton;

        public event Action<bool> InteractSecondButton;

        public event Action<bool> JumpButton;

        public event Action<bool> SprintButton;

        public Vector2 Move() => direction;


        private void OnMove(InputValue value)
        {
            direction = value.Get<Vector2>();
        }

        private void OnAttack(InputValue value)
        {
            interactFirst = value.isPressed;
            InteractFirstButton?.Invoke(interactFirst);
        }

        private void OnRight(InputValue value)
        {
            interactSecond = value.isPressed;
            InteractSecondButton?.Invoke(interactSecond);
        }

        private void OnJump(InputValue value)
        {
            jump = value.isPressed;
            JumpButton?.Invoke(jump);
        }

        private void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
            SprintButton?.Invoke(sprint);
        }
    }
}
