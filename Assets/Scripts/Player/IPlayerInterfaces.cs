using System;
using UnityEngine;

public interface IControllable
{
    public event Action<bool> InteractFirstButton;

    public event Action<bool> InteractSecondButton;

    public event Action<bool> JumpButton;

    public event Action<bool> SprintButton;

    Vector2 Move();
}

public interface IMovement
{
    void MoveTo(Vector3 direction);

    bool Jump();

    void UnpressedJump();
}

public interface IPlayerLookInput
{
    Vector2 LookDelta { get; }
    void Enable();
    void Disable();
}