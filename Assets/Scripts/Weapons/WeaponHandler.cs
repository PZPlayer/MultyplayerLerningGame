using Mirror;
using System;
using UnityEngine;

// ВАЖНО: теперь NetworkBehaviour, а не MonoBehaviour —
// иначе isServer/isOwned/connectionToClient/NetworkServer.Spawn недоступны.
public class WeaponHandler : NetworkBehaviour, IWeaponHandler, IBoostrapble
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

    // Реплицируем ссылку на заспавненное оружие всем клиентам через SyncVar.
    // Сам объект клиенты НЕ инстанциируют сами — Mirror делает это автоматически,
    // когда сервер вызывает NetworkServer.Spawn (если префаб зарегистрирован в NetworkManager).
    [SyncVar(hook = nameof(OnCurrentWeaponChanged))]
    private NetworkIdentity currentWeaponIdentity;

    void IBoostrapble.BoostrapAwake()
    {
        // Только компонентный поиск — БЕЗ проверок isServer/isOwned
        _lookFOV = GetComponent<IPlayerLookFOV>();
        _controllable = GetComponent<IControllable>();
        _camEffects = GetComponent<ICameraEffects>();

        // Анимации — чисто локальная визуальщина, можно здесь
        _anmtr.runtimeAnimatorController = _baseOverrideController;
        SetOverrideController(_startWeapon);
    }

    // Вызывается только на сервере, isServer уже true
    public override void OnStartServer()
    {
        EquipWeapon(_startWeapon);
    }

    // Вызывается только у владельца, isOwned уже true
    public override void OnStartLocalPlayer()
    {
        _controllable.InteractFirstButton += OnShootPressed;
        _controllable.InteractSecondButton += OnAimPressed;
    }

    private void OnShootPressed(bool shootPressed)
    {
        if (_currentWeapon == null) return; // оружие может ещё не успеть засинкаться
        IShootable shoot = _currentWeapon.GetComponent<IShootable>();
        if (shoot == null) return;

        if (shootPressed) shoot.Shoot();
        else shoot.StopShooting();
    }

    private void OnGoodShot() => OnShootStart?.Invoke(true);

    private void OnAimPressed(bool aimPressed)
    {
        if (_currentWeapon == null) return;
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

    [Server]
    private void EquipWeapon(WeaponData data)
    {
        // Спавним в мировых координатах spawn-поинта — SetParent больше не нужен здесь:
        // Mirror НЕ реплицирует родителя клиентам, так что делать это на сервере бессмысленно,
        // парентинг теперь происходит локально на каждой машине внутри OnCurrentWeaponChanged.
        GameObject weapon = Instantiate(data.weapon, _weaponSpawnPoint.position, _weaponSpawnPoint.rotation);

        // Вот эта строка — то, чего не хватало.
        // Она и регистрирует объект в сети, и сразу назначает authority
        // тому клиенту, который владеет этим WeaponHandler (connectionToClient).
        // После этого у владельца isOwned на Pistol станет true.
        NetworkServer.Spawn(weapon, connectionToClient);

        currentWeaponIdentity = weapon.GetComponent<NetworkIdentity>();
    }

    // Вызывается НА КАЖДОЙ машине (сервер, владелец, любой другой клиент-наблюдатель),
    // когда currentWeaponIdentity меняется. Mirror не синкает Transform.parent сам —
    // поэтому парентим вручную локально здесь, а не один раз на сервере.
    private void OnCurrentWeaponChanged(NetworkIdentity oldId, NetworkIdentity newId)
    {
        if (newId == null) return;

        // worldPositionStays: false — сразу "защёлкиваем" оружие точно в spawn-поинт
        // в локальных координатах, а не оставляем его мировую позицию как есть.
        newId.transform.SetParent(_weaponSpawnPoint, worldPositionStays: false);
        newId.transform.localPosition = Vector3.zero;
        newId.transform.localRotation = Quaternion.identity;

        _currentWeapon = newId.GetComponent<WeaponBase>();

        if (_currentWeapon.TryGetComponent<IShootable>(out var shoot))
            shoot.OnSuccsesfulShot += OnGoodShot;

        if (_currentWeapon.TryGetComponent<IWeaponInitiliazble>(out var initiliazble))
            initiliazble.Initiliaze(_camEffects, _camera);
    }

    private void SetOverrideController(WeaponData data)
    {
        // ПОБОЧНЫЙ БАГ, не связанный с сетью: newOverrideController нигде не создаётся
        // через `new AnimatorOverrideController(...)`, поэтому строки ниже кинут NullReferenceException.
        // Скорее всего нужно что-то вроде:
        // newOverrideController = new AnimatorOverrideController(_baseOverrideController);
        // _anmtr.runtimeAnimatorController = newOverrideController;
        if (data.Run) newOverrideController["HumanM@Run01_Forward"] = data.Run;
        if (data.Walk) newOverrideController["HumanM@Walk01_Forward"] = data.Walk;
        if (data.Shoot) newOverrideController["HumanM@Gun_Aim02_Shoot01"] = data.Shoot;
        if (data.Aim) newOverrideController["HumanM@Gun_Aim02"] = data.Aim;
        if (data.Falling) newOverrideController["Jumping Down 1"] = data.Falling;
    }

    private void OnDestroy()
    {
        if (_controllable == null) return;
        _controllable.InteractFirstButton -= OnShootPressed;
        _controllable.InteractSecondButton -= OnAimPressed;
    }
}