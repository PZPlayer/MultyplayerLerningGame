using UnityEngine;
using System;

public interface IShootable
{
    public event Action OnSuccsesfulShot;

    void Shoot();
    void StopShooting();
}

public interface IAimable
{
    void Aim();
    void StopAiming();
}

public interface IReloadble
{
    void Reload();
}

public interface IWeaponInitiliazble
{
    void Initiliaze(ICameraEffects eff, Camera camera);
}

public interface IBaseWeapon: IShootable, IReloadble, IAimable
{

}

public interface IWeaponHandler
{
    public event Action<bool> OnShootStart;
    public event Action<bool> OnAimStart;
}