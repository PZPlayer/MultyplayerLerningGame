using UnityEngine;
using System;

public interface IShootable
{
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

public interface IBaseWeapon: IShootable, IReloadble, IAimable
{

}

public interface IWeaponHandler
{
    public event Action<bool> OnShootStart;
    public event Action<bool> OnAimStart;
}