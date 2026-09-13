using UnityEngine;

public enum FireMode
{
    Semi,
    Auto
}

[CreateAssetMenu(menuName = "TTHS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public GameObject worldPrefab;
    public int magazineSize = 25;
    public float fireRate = 10f;
    public float reloadTime = 1.4f;
    public float shootRange = 80f;
    public FireMode fireMode = FireMode.Auto;
}
