using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Objects")]
    public GameObject weapon;

    [Header("Stats")]
    public float Damage;
    public float ShootBetweenTime;
    public float ReloadTime;
    public int Ammo;

    [Space(10)]
    [Header("Animations")]
    public AnimationClip Walk;
    public AnimationClip Run;
    public AnimationClip Shoot;
    public AnimationClip Aim;
    public AnimationClip Falling;
}
