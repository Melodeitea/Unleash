using UnityEngine;

[System.Serializable]
public class CameraZoneData
{
    [Tooltip("World-space position the camera moves to for this zone.")]
    public Vector3 position;
    [Tooltip("World-space euler angles the camera rotates to for this zone.")]
    public Vector3 eulerAngles;
    [Tooltip("Vertical field of view in degrees.")]
    [Range(10f, 120f)]
    public float fieldOfView = 60f;
    [Tooltip("How long the transition takes in seconds.")]
    [Min(0f)]
    public float transitionDuration = 0.6f;
    [Tooltip("Easing curve for the transition (X = normalised time, Y = normalised value).")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Quaternion Rotation => Quaternion.Euler(eulerAngles);
}

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CameraTriggerVolume : MonoBehaviour
{
    [SerializeField] private CameraZoneData zoneData = new CameraZoneData();
    [SerializeField] private Vector3 boxSize = new Vector3(4f, 3f, 4f);

    [Tooltip("Remove this zone from the stack when the player exits. " +
             "Should be on for all zones when using overlapping volumes.")]
    [SerializeField] private bool revertOnExit = true; // default true now — stack handles priority

    public CameraZoneData ZoneData => zoneData;

    private BoxCollider _box;

    private void Awake()
    {
        _box = GetComponent<BoxCollider>();
        _box.isTrigger = true;
        _box.size = boxSize;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CameraSwitcher.EnterZone(zoneData);         // was: SwitchToZone
    }

    private void OnTriggerExit(Collider other)
    {
        if (!revertOnExit || !other.CompareTag("Player")) return;
        CameraSwitcher.ExitZone(zoneData);          // was: GoToPreviousZone
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.15f);
        Gizmos.DrawCube(transform.position, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
        DrawPositionMarker(0.12f);
    }

    private void OnDrawGizmosSelected()
    {
        DrawCameraFrustum();
        DrawPositionMarker(0.18f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            zoneData.position + Vector3.up * 0.45f,
            $"  📷 {gameObject.name}  |  FOV {zoneData.fieldOfView:F0}°",
            UnityEditor.EditorStyles.boldLabel);
#endif
    }

    private void DrawPositionMarker(float size)
    {
        Quaternion rot = zoneData.Rotation;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(zoneData.position, size);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(zoneData.position, rot * Vector3.forward * size * 4f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(zoneData.position, rot * Vector3.right * size * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(zoneData.position, rot * Vector3.up * size * 2f);
    }

    private void DrawCameraFrustum()
    {
        const float near = 0.3f;
        const float far = 14f;
        const float aspect = 16f / 9f;

        float nearH = Mathf.Tan(zoneData.fieldOfView * 0.5f * Mathf.Deg2Rad) * near;
        float nearW = nearH * aspect;
        float farH = Mathf.Tan(zoneData.fieldOfView * 0.5f * Mathf.Deg2Rad) * far;
        float farW = farH * aspect;

        Vector3 pos = zoneData.position;
        Quaternion rot = zoneData.Rotation;
        Vector3 W(Vector3 local) => pos + rot * local;

        Vector3 nTL = W(new Vector3(-nearW, nearH, near));
        Vector3 nTR = W(new Vector3(nearW, nearH, near));
        Vector3 nBL = W(new Vector3(-nearW, -nearH, near));
        Vector3 nBR = W(new Vector3(nearW, -nearH, near));
        Vector3 fTL = W(new Vector3(-farW, farH, far));
        Vector3 fTR = W(new Vector3(farW, farH, far));
        Vector3 fBL = W(new Vector3(-farW, -farH, far));
        Vector3 fBR = W(new Vector3(farW, -farH, far));

        Gizmos.color = new Color(0f, 0.85f, 1f, 0.9f);

        Gizmos.DrawLine(nTL, nTR); Gizmos.DrawLine(nTR, nBR);
        Gizmos.DrawLine(nBR, nBL); Gizmos.DrawLine(nBL, nTL);
        Gizmos.DrawLine(fTL, fTR); Gizmos.DrawLine(fTR, fBR);
        Gizmos.DrawLine(fBR, fBL); Gizmos.DrawLine(fBL, fTL);
        Gizmos.DrawLine(nTL, fTL); Gizmos.DrawLine(nTR, fTR);
        Gizmos.DrawLine(nBL, fBL); Gizmos.DrawLine(nBR, fBR);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null) bc.size = boxSize;
    }
#endif
}