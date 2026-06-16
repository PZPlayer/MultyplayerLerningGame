using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookInput : NetworkBehaviour, IPlayerLookInput
{
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private bool invertY = false;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private Vector2 lookDelta;

    public Vector2 LookDelta => lookDelta;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"]; // действие "Look" должно быть в Input Action Asset
    }

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer)
            this.enabled = false;
    }

    private void OnEnable() => Enable();
    private void OnDisable() => Disable();

    public void Enable() => lookAction?.Enable();
    public void Disable() => lookAction?.Disable();

    private void Update()
    {
        Vector2 raw = lookAction.ReadValue<Vector2>();
        float x = raw.x * sensitivity;
        float y = raw.y * sensitivity * (invertY ? -1 : 1);
        lookDelta = new Vector2(x, y);
    }
}
