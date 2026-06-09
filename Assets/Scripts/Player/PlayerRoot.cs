using UnityEngine;

namespace Thanks.Player
{
    public class PlayerRoot : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour movementScript;
        [SerializeField] private MonoBehaviour controllerScript;

        private IControllable _controller;
        private IMovement _movement;
        private bool ifJumpWasPressed;
        private bool ifSecondWasPressed;
        private bool ifFirstWasPressed;

        private void Start()
        {
            _controller = controllerScript as IControllable;
            _movement = movementScript as IMovement;

            _controller.InteractFirstButton += OnInteractFirst;
            _controller.InteractSecondButton += OnInteractSecond;
            _controller.JumpButton += OnJump;
        }

        private void Update()
        {
            _movement.MoveTo(new Vector3(_controller.Move().x, 0, _controller.Move().y));
        }

        private void OnJump(bool isPressed)
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

        private void OnInteractFirst(bool isPressed)
        {
            ifFirstWasPressed = isPressed;
        }

        private void OnDestroy()
        {
            _controller.InteractFirstButton -= OnInteractFirst;
            _controller.InteractSecondButton -= OnInteractSecond;
            _controller.JumpButton -= OnJump;
        }
    }
}