using UnityEngine;

[CreateAssetMenu(menuName = "TTHS/Vehicle Data")]
public class VehicleData : ScriptableObject
{
    public float reverseSpeedCap = 7f;
    public float reverseAcceleration = 12f;
    public float neutralAcceleration = 0f;
    public float[] forwardSpeedCaps = { 8f, 14f, 20f, 26f, 32f };
    public float[] forwardAccelerations = { 16f, 18f, 20f, 22f, 24f };
}
