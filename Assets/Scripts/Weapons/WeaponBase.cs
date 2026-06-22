using Mirror;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    [SerializeField] protected Transform _shootPoint;
    [SerializeField] protected WeaponData _data;
}
