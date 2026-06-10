using System;
using UnityEngine;

public interface IControllable
{
    public event Action<bool> InteractFirstButton;

    public event Action<bool> InteractSecondButton;

    public event Action<bool> JumpButton;

    public event Action DashButton;

    public event Action<bool> SprintButton;

    Vector2 Move();
}

public interface IMovement
{
    /// <summary>
    /// В каком направлении идти
    /// </summary>
    void MoveTo(Vector3 direction);

    /// <summary>
    /// Бежать/не бежать
    /// </summary>
    void SetSprint(bool isSprinting);

    /// <summary>
    /// Когда игрок НАжал на кнопку прыжка
    /// </summary>
    bool Jump();

    /// <summary>
    /// Когда игрок ОТжал на кнопку прыжка
    /// </summary>
    void UnpressedJump();
}

public interface IDashable
{
    void ApplyDashImpulse(Vector3 impulse);
}

public interface IDashInput
{
    void PerformDash(Vector3 direction);
}

public interface IPlayerLookInput
{
    Vector2 LookDelta { get; }
    void Enable();
    void Disable();
}

public interface IPlayerLookFOV
{
    /// <summary>
    /// Для плавного изменения FOV
    /// </summary>
    /// <param name="targetFOV"> Желаемое FOV </param>
    /// <param name="speed"> С какой скоростью поменяеть FOV, в секундах </param>
    void ChangeFOV(float targetFOV, float speed);

    /// <summary>
    /// Изменить FOV для прицеливания, задаеться изначально, в скрипте
    /// </summary>
    void SetAimFOV();

    /// <summary>
    /// Сбрасывает FOV
    /// </summary>
    void ResetFOV();
}