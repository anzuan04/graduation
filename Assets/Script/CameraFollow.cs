// ============================================================================
// 6. CameraFollow.cs - 카메라 추적
// ============================================================================
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        if (!target)
            target = FindAnyObjectByType<PlayerController>()?.transform;
    }

    void FixedUpdate()
    {
        if (!target) return;

        var desiredPosition = target.position + offset;
        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}