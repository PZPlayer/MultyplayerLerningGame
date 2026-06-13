using Thanks.Player;
using UnityEngine;

public class PlayerAnimationsManager : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovement _movement;

    private IWeaponHandler _weaponHandler;

    private void Start()
    {
        _weaponHandler = GetComponent<WeaponHandler>();

        _movement.OnDash += OnPlayerDashed;
        _movement.OnJump += OnPlayerJumpped;
        _movement.OnMove += OnChangePosition;
        _weaponHandler.OnShootStart += OnShoot;
        _weaponHandler.OnAimStart += OnAim;
    }

    private void FixedUpdate()
    {
        _animator.SetBool("IsGround", _movement.isGrounded());
    }

    private void OnShoot(bool pressed)
    {
        _animator.SetTrigger("Shoot");
    }

    private void OnAim(bool pressed)
    {
        _animator.SetBool("Aiming", pressed);
    }

    private void OnChangePosition(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    private void OnPlayerJumpped()
    {
        _animator.SetTrigger("Jump");
    }

    private void OnPlayerDashed()
    {
        _animator.SetTrigger("Dash");
    }

    private void OnDestroy()
    {
        _movement.OnDash -= OnPlayerDashed;
        _movement.OnJump -= OnPlayerJumpped;
        _movement.OnMove -= OnChangePosition;
        _weaponHandler.OnShootStart -= OnShoot;
        _weaponHandler.OnAimStart -= OnAim;
    }
}
