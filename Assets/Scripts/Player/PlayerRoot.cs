using UnityEngine;

namespace Thanks.Player
{
    public class PlayerRoot : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour movementScript;
        [SerializeField] private MonoBehaviour controllerScript;
        [SerializeField] private MonoBehaviour dashScript;

        private IControllable _controller;
        private IMovement _movement;
        private IDashInput _dash;

        private bool ifJumpWasPressed;
        private bool ifSecondWasPressed;
        private bool ifFirstWasPressed;
        private Vector3 direction;

        private void Start()
        {
            _controller = controllerScript as IControllable;
            _movement = movementScript as IMovement;
            _dash = dashScript as IDashInput;

            _controller.InteractFirstButton += OnInteractFirst;
            _controller.InteractSecondButton += OnInteractSecond;
            _controller.JumpButton += OnJumpDo;
            _controller.SprintButton += OnInteractSprintButton;
            _controller.DashButton += OnInteractDash;
        }

        private void Update()
        {
            if (!controllerScript.enabled)
                this.enabled = false;

            direction = new Vector3(_controller.Move().x, 0, _controller.Move().y);
            _movement.MoveTo(direction);
        }

        private void OnJumpDo(bool isPressed)
        {
            ifJumpWasPressed = isPressed;

            if (ifJumpWasPressed)
            {
                _movement.Jump();
            }
            else
            {
                _movement.UnpressedJump();
            }
        }

        private void OnInteractSecond(bool isPressed)
        {
            ifSecondWasPressed = isPressed;
        }

        private void OnInteractDash()
        {
            _dash.PerformDash(direction);
        }

        private void OnInteractFirst(bool isPressed)
        {
            ifFirstWasPressed = isPressed;
        }

        private void OnInteractSprintButton(bool isPressed)
        {
            _movement.SetSprint(isPressed);
        }

        private void OnDestroy()
        {
            _controller.InteractFirstButton -= OnInteractFirst;
            _controller.InteractSecondButton -= OnInteractSecond;
            _controller.JumpButton -= OnJumpDo;
            _controller.SprintButton -= OnInteractSprintButton;
            _controller.DashButton -= OnInteractDash;
        }
    }
}