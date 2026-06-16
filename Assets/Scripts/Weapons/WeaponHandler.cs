using Mirror.BouncyCastle.Utilities;
using System;
using UnityEngine;

public class WeaponHandler : MonoBehaviour, IWeaponHandler
{
    public event Action<bool> OnShootStart;
    public event Action<bool> OnAimStart;

    [SerializeField] private WeaponBase _currentWeapon;
    [SerializeField] private WeaponData _startWeapon;
    [SerializeField] private Transform _weaponSpawnPoint;
    [SerializeField] private Camera _camera;
    [SerializeField] private Animator _anmtr;
    [SerializeField] private AnimatorOverrideController _baseOverrideController;
    
    private IControllable _controllable;
    private IPlayerLookFOV _lookFOV;
    private ICameraEffects _camEffects;
    private AnimatorOverrideController newOverrideController;

    private void Start()
    {
        _lookFOV = transform.GetComponent<IPlayerLookFOV>();
        _controllable = GetComponent<IControllable>();
        _camEffects = GetComponent<ICameraEffects>();
        _controllable.InteractFirstButton += OnShootPressed;
        _controllable.InteractSecondButton += OnAimPressed;

        EquipWeapon(_startWeapon);
    }

    private void OnShootPressed(bool shootPressed)
    {
        IShootable shoot = _currentWeapon.GetComponent<IShootable>();

        if(shoot == null) return;

        

        if (shootPressed)
        {
            shoot.Shoot();
        }
        else
        {
            shoot.StopShooting();
        }
    }

    private void OnGoodShot() => OnShootStart?.Invoke(true);

    private void OnAimPressed(bool aimPressed)
    {
        IAimable aimable = _currentWeapon.GetComponent<IAimable>();

        if (aimable == null) return;

        if (aimPressed)
        {
            aimable.Aim();
            _lookFOV?.SetAimFOV();
        }
        else
        {
            aimable.StopAiming();
            _lookFOV?.ResetFOV();
        }

        OnAimStart?.Invoke(aimPressed);
    }

    private void EquipWeapon(WeaponData data)
    {
        _anmtr.runtimeAnimatorController = _baseOverrideController;
        SetOverrideController(data);
        GameObject weapon = Instantiate(data.weapon, _weaponSpawnPoint.transform.position, _weaponSpawnPoint.transform.rotation);
        weapon.transform.SetParent(_weaponSpawnPoint);
        _currentWeapon = weapon.GetComponent<WeaponBase>();

        if (_currentWeapon.GetComponent<IShootable>() != null) _currentWeapon.GetComponent<IShootable>().OnSuccsesfulShot += OnGoodShot;

        if (_currentWeapon.TryGetComponent<IWeaponInitiliazble>(out IWeaponInitiliazble initiliazble))
        {
            initiliazble.Initiliaze(_camEffects, _camera);
        }
    }

    private void SetOverrideController(WeaponData data)
    {
        if (data.Run)
            newOverrideController["HumanM@Run01_Forward"] = data.Run;
        if (data.Walk)
            newOverrideController["HumanM@Walk01_Forward"] = data.Walk;
        if (data.Shoot)
            newOverrideController["HumanM@Gun_Aim02_Shoot01"] = data.Shoot;
        if (data.Aim)
            newOverrideController["HumanM@Gun_Aim02"] = data.Aim;
        if (data.Falling)
            newOverrideController["Jumping Down 1"] = data.Falling;
    }

    private void OnDestroy()
    {
        _controllable.InteractFirstButton -= OnShootPressed;
        _controllable.InteractSecondButton -= OnAimPressed;
    }
}
