using System.Collections.Generic;
using UnityEngine;

public class WheelVisuals : MonoBehaviour
{
    public CarController carController;
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;
    public Transform modelRoot;

    public float wheelRadius = 35f;
    public float maxSteerAngle = 30f;
    public float steerReturnSpeed = 120f;

    private readonly List<WheelSlot> wheelSlots = new List<WheelSlot>();

    void Start()
    {
        if (carController == null)
        {
            carController = GetComponent<CarController>();
        }

        if (HasManualPivots())
        {
            RegisterManualPivot(frontLeft, true);
            RegisterManualPivot(frontRight, true);
            RegisterManualPivot(rearLeft, false);
            RegisterManualPivot(rearRight, false);
        }
        else
        {
            TryAutoSetupWheels();
        }
    }

    void LateUpdate()
    {
        if (carController == null || wheelSlots.Count == 0)
        {
            return;
        }

        float speed = carController.CurrentSpeed;
        float steerInput = carController.SteerInput;
        float turnSpeed = carController.TurnSpeed;
        float rollSign = speed >= 0f ? 1f : -1f;
        float rollDelta = (speed * Time.deltaTime / wheelRadius) * Mathf.Rad2Deg * rollSign;

        // The front wheels' steering angle should always match the steering input directly,
        // regardless of whether the car is moving forward or backward — just like a real
        // steering wheel: turning it right always points the front wheels right.
        float targetSteerDegrees = turnSpeed > 0f
            ? Mathf.Clamp(steerInput / turnSpeed * maxSteerAngle, -maxSteerAngle, maxSteerAngle)
            : 0f;

        foreach (WheelSlot slot in wheelSlots)
        {
            Vector3 center = transform.TransformPoint(slot.LocalCenter);
            Vector3 axle = transform.TransformDirection(slot.LocalAxle).normalized;

            if (slot.IsFront && Mathf.Abs(slot.AppliedSteerAngle) > 0.001f)
            {
                foreach (Transform part in slot.Parts)
                {
                    if (part == null)
                    {
                        continue;
                    }

                    part.RotateAround(center, transform.up, -slot.AppliedSteerAngle);
                }
            }

            foreach (Transform part in slot.Parts)
            {
                if (part == null)
                {
                    continue;
                }

                part.RotateAround(center, axle, rollDelta);
            }

            if (slot.IsFront)
            {
                slot.CurrentSteerAngle = Mathf.MoveTowards(
                    slot.CurrentSteerAngle,
                    targetSteerDegrees,
                    steerReturnSpeed * Time.deltaTime
                );

                foreach (Transform part in slot.Parts)
                {
                    if (part == null)
                    {
                        continue;
                    }

                    part.RotateAround(center, transform.up, slot.CurrentSteerAngle);
                }

                slot.AppliedSteerAngle = slot.CurrentSteerAngle;
            }
        }
    }

    bool HasManualPivots()
    {
        return frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;
    }

    void RegisterManualPivot(Transform pivot, bool isFront)
    {
        if (pivot == null)
        {
            return;
        }

        List<Transform> parts = new List<Transform>();
        foreach (Transform child in pivot.GetComponentsInChildren<Transform>(true))
        {
            if (child != pivot)
            {
                parts.Add(child);
            }
        }

        if (parts.Count == 0)
        {
            parts.Add(pivot);
        }

        wheelSlots.Add(CreateWheelSlot(parts, pivot, isFront));
    }

    void TryAutoSetupWheels()
    {
        Transform searchRoot = modelRoot != null ? modelRoot : transform;
        List<Transform> rimTransforms = new List<Transform>();
        CollectTransforms(searchRoot, rimTransforms, "Rim_Main");

        if (rimTransforms.Count < 4)
        {
            Debug.LogWarning("WheelVisuals: no se encontraron 4 rines para configurar ruedas automáticamente.");
            return;
        }

        List<Transform> tyreTransforms = new List<Transform>();
        List<Transform> rotorTransforms = new List<Transform>();
        CollectTransforms(searchRoot, tyreTransforms, "TYRE_mm_tyre");
        CollectTransforms(searchRoot, rotorTransforms, "ROTOR_mm_rotor");

        HashSet<Transform> assignedParts = new HashSet<Transform>();
        List<WheelCandidate> candidates = new List<WheelCandidate>();

        foreach (Transform rim in rimTransforms)
        {
            List<Transform> wheelParts = new List<Transform> { rim };
            Transform tyre = FindClosestTransform(tyreTransforms, rim.position, assignedParts);
            Transform rotor = FindClosestTransform(rotorTransforms, rim.position, assignedParts);

            if (tyre != null)
            {
                wheelParts.Add(tyre);
                assignedParts.Add(tyre);
            }

            if (rotor != null)
            {
                wheelParts.Add(rotor);
                assignedParts.Add(rotor);
            }

            assignedParts.Add(rim);

            candidates.Add(new WheelCandidate
            {
                Rim = rim,
                Parts = wheelParts,
                LocalPosition = transform.InverseTransformPoint(GetPartsCenter(wheelParts))
            });
        }

        candidates.Sort((a, b) => b.LocalPosition.z.CompareTo(a.LocalPosition.z));
        List<WheelCandidate> frontWheels = candidates.GetRange(0, 2);
        List<WheelCandidate> rearWheels = candidates.GetRange(2, 2);

        frontWheels.Sort((a, b) => a.LocalPosition.x.CompareTo(b.LocalPosition.x));
        rearWheels.Sort((a, b) => a.LocalPosition.x.CompareTo(b.LocalPosition.x));

        wheelSlots.Add(CreateWheelSlot(frontWheels[0].Parts, frontWheels[0].Rim, true));
        wheelSlots.Add(CreateWheelSlot(frontWheels[1].Parts, frontWheels[1].Rim, true));
        wheelSlots.Add(CreateWheelSlot(rearWheels[0].Parts, rearWheels[0].Rim, false));
        wheelSlots.Add(CreateWheelSlot(rearWheels[1].Parts, rearWheels[1].Rim, false));

        frontLeft = frontWheels[0].Rim;
        frontRight = frontWheels[1].Rim;
        rearLeft = rearWheels[0].Rim;
        rearRight = rearWheels[1].Rim;
    }

    WheelSlot CreateWheelSlot(List<Transform> parts, Transform axleReference, bool isFront)
    {
        Vector3 worldCenter = GetPartsCenter(parts);
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 localAxle = transform.InverseTransformDirection(GetRollAxis(axleReference));

        return new WheelSlot
        {
            Parts = new List<Transform>(parts),
            LocalCenter = localCenter,
            LocalAxle = localAxle.normalized,
            IsFront = isFront
        };
    }

    static Vector3 GetRollAxis(Transform reference)
    {
        return reference.right;
    }

    static Vector3 GetPartsCenter(List<Transform> parts)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (Transform part in parts)
        {
            if (part == null)
            {
                continue;
            }

            center += part.position;
            count++;
        }

        return count > 0 ? center / count : Vector3.zero;
    }

    static Transform FindClosestTransform(List<Transform> candidates, Vector3 referencePosition, HashSet<Transform> assignedParts)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform candidate in candidates)
        {
            if (candidate == null || assignedParts.Contains(candidate))
            {
                continue;
            }

            float distance = Vector3.Distance(candidate.position, referencePosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    static void CollectTransforms(Transform root, List<Transform> results, string nameContains)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (!child.name.Contains(nameContains) || results.Contains(child))
            {
                continue;
            }

            results.Add(child);
        }
    }

    class WheelSlot
    {
        public List<Transform> Parts;
        public Vector3 LocalCenter;
        public Vector3 LocalAxle;
        public bool IsFront;
        public float CurrentSteerAngle;
        public float AppliedSteerAngle;
    }

    struct WheelCandidate
    {
        public Transform Rim;
        public List<Transform> Parts;
        public Vector3 LocalPosition;
    }
}