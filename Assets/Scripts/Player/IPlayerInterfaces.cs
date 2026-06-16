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

public interface ICameraEffects
{
    /// <summary>
    /// Сместить камеру (punch) на заданный угол с постоянной скоростью (град/сек).
    /// После достижения цели камера остаётся в новом положении (без возврата).
    /// </summary>
    /// <param name="delta">Смещение (x = yaw, y = pitch) в градусах</param>
    /// <param name="speed">Скорость смещения (град/сек)</param>
    void Punch(Vector2 delta, float speed);

    /// <summary>
    /// Начать тряску камеры с заданной интенсивностью и длительностью.
    /// </summary>
    /// <param name="intensity">Максимальное отклонение в градусах</param>
    /// <param name="duration">Длительность тряски (сек)</param>
    /// <param name="decay">Коэффициент затухания (0-1, 1 = без затухания)</param>
    void Shake(float intensity, float duration, float decay = 0.9f);

    /// <summary>
    /// Остановить все эффекты (punch и shake) мгновенно.
    /// </summary>
    void StopAll();
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