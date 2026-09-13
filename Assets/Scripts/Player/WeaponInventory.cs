using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();
    public int currentWeaponIndex = 0;

    [Header("Cambio de arma (opcional)")]
    public bool enableWeaponSwitching = false;
    public KeyCode nextWeaponKey = KeyCode.E;
    public KeyCode previousWeaponKey = KeyCode.Q;

    public event System.Action<WeaponData> OnWeaponChanged;

    void Update()
    {
        if (!enableWeaponSwitching || weapons == null || weapons.Count <= 1)
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f)
        {
            SwitchNext();
        }
        else if (scroll < -0.01f)
        {
            SwitchPrevious();
        }

        if (Input.GetKeyDown(nextWeaponKey))
        {
            SwitchNext();
        }
        else if (Input.GetKeyDown(previousWeaponKey))
        {
            SwitchPrevious();
        }

        for (int i = 0; i < Mathf.Min(weapons.Count, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }
    }

    public WeaponData GetCurrentWeapon()
    {
        if (weapons == null || weapons.Count == 0)
        {
            return null;
        }

        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, weapons.Count - 1);
        return weapons[currentWeaponIndex];
    }

    public bool SwitchWeapon(int index)
    {
        if (weapons == null || weapons.Count == 0)
        {
            return false;
        }

        int clampedIndex = Mathf.Clamp(index, 0, weapons.Count - 1);
        if (clampedIndex == currentWeaponIndex)
        {
            return false;
        }

        currentWeaponIndex = clampedIndex;
        OnWeaponChanged?.Invoke(GetCurrentWeapon());
        return true;
    }

    public void SwitchNext()
    {
        if (weapons == null || weapons.Count == 0)
        {
            return;
        }

        SwitchWeapon((currentWeaponIndex + 1) % weapons.Count);
    }

    public void SwitchPrevious()
    {
        if (weapons == null || weapons.Count == 0)
        {
            return;
        }

        int nextIndex = currentWeaponIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = weapons.Count - 1;
        }

        SwitchWeapon(nextIndex);
    }
}
