using System.Collections;
using UnityEngine;

public class DroppedWeaponPickup : MonoBehaviour, IPickupable
{
    [HideInInspector] public WeaponEquipController owner;

    public void OnPickedUp(PlayerController player)
    {
        if (owner != null)
        {
            owner.PickupWeapon();
        }
    }
}