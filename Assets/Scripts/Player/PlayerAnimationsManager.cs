using Thanks.Player;
using UnityEngine;
using Mirror;

public class PlayerAnimationsManager : NetworkBehaviour, IBoostrapble
{
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovement _movement; // ссылка остаётся

    private IWeaponHandler _weaponHandler;

    // SyncVars для синхронизации
    [SyncVar(hook = nameof(OnSpeedChanged))]
    private float syncSpeed;

    [SyncVar(hook = nameof(OnGroundedChanged))]
    private bool syncGrounded;

    [SyncVar(hook = nameof(OnAimingChanged))]
    private bool syncAiming;

    void IBoostrapble.BoostrapAwake()
    {
        _weaponHandler = GetComponent<WeaponHandler>();

        // Подписки как были
        _movement.OnDash += OnPlayerDashed;
        _movement.OnJump += OnPlayerJumpped;
        _movement.OnMove += OnChangePosition;
        _weaponHandler.OnShootStart += OnShoot;
        _weaponHandler.OnAimStart += OnAim;
    }

    private void FixedUpdate()
    {
        // Только локальный обновляет состояние земли
        if (isLocalPlayer)
        {
            bool grounded = _movement.isGrounded();
            if (grounded != syncGrounded)
                CmdUpdateGrounded(grounded);
        }
    }

    // ---- Обработчики событий (вызываются только на локальном игроке) ----

    private void OnShoot(bool pressed)
    {
        // Отправляем триггер через RPC
        CmdTriggerShoot();
    }

    private void OnAim(bool pressed)
    {
        // Отправляем состояние прицеливания
        CmdUpdateAiming(pressed);
    }

    private void OnChangePosition(float speed)
    {
        // Отправляем скорость на сервер
        CmdUpdateSpeed(speed);
        // Локально тоже обновляем, чтобы сразу видеть
        _animator.SetFloat("Speed", speed);
    }

    private void OnPlayerJumpped()
    {
        CmdTriggerJump();
        // Локально триггер тоже ставим, чтобы моментально
        _animator.SetTrigger("Jump");
    }

    private void OnPlayerDashed()
    {
        CmdTriggerDash();
        _animator.SetTrigger("Dash");
    }

    // ---- Команды ----

    [Command]
    private void CmdUpdateSpeed(float speed)
    {
        syncSpeed = speed;
    }

    [Command]
    private void CmdUpdateAiming(bool aiming)
    {
        syncAiming = aiming;
    }

    [Command]
    private void CmdUpdateGrounded(bool grounded)
    {
        syncGrounded = grounded;
    }

    [Command]
    private void CmdTriggerJump()
    {
        RpcPlayJump();
    }

    [Command]
    private void CmdTriggerDash()
    {
        RpcPlayDash();
    }

    [Command]
    private void CmdTriggerShoot()
    {
        RpcPlayShoot();
    }

    // ---- ClientRpc для триггеров ----

    [ClientRpc]
    private void RpcPlayJump()
    {
        // На всех клиентах (включая локального) ставим триггер,
        // но локальный уже поставил, можно дублировать, но безопаснее оставить.
        _animator.SetTrigger("Jump");
    }

    [ClientRpc]
    private void RpcPlayDash()
    {
        _animator.SetTrigger("Dash");
    }

    [ClientRpc]
    private void RpcPlayShoot()
    {
        _animator.SetTrigger("Shoot");
    }

    // ---- Хуки для SyncVar ----

    private void OnSpeedChanged(float oldVal, float newVal)
    {
        _animator.SetFloat("Speed", newVal);
    }

    private void OnGroundedChanged(bool oldVal, bool newVal)
    {
        _animator.SetBool("IsGround", newVal);
    }

    private void OnAimingChanged(bool oldVal, bool newVal)
    {
        _animator.SetBool("Aiming", newVal);
    }

    // ---- Отписки (оставлены как были) ----

    private void OnDestroy()
    {
        _movement.OnDash -= OnPlayerDashed;
        _movement.OnJump -= OnPlayerJumpped;
        _movement.OnMove -= OnChangePosition;
        _weaponHandler.OnShootStart -= OnShoot;
        _weaponHandler.OnAimStart -= OnAim;
    }
}