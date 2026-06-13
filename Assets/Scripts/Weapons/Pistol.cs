using UnityEngine;

public class Pistol : WeaponBase, IBaseWeapon
{
    void IAimable.Aim()
    {
        Debug.Log("Aiming!");
    }

    void IReloadble.Reload() 
    {
        Debug.Log("Reloading!");
    }

    void IShootable.Shoot()
    {
        Debug.Log("Shooting!");
    }

    void IAimable.StopAiming()
    {
        Debug.Log("Stop Aiming!");
    }

    void IShootable.StopShooting()
    {
        Debug.Log("Stop Shooting!");
    }
}
