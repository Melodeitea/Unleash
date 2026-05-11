using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Flashlight : MonoBehaviour
{
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Light")]
    public Light spotLight;

    [Header("Rotation")]
    public float sensitivity = 2f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("Reveal Objects")]
    [Tooltip("All renderers on these objects and their children will swap materials.")]
    [SerializeField] private List<GameObject> revealObjects = new();

    [SerializeField] private Material revealMaterial;

    [Header("Flag")]
    [Tooltip("Flag set while flashlight is ON, cleared when OFF.")]
    [SerializeField] private string flashlightFlag = "flashlight_on";

    bool _isOn;
    float _pitch;
    float _yaw;

    public bool IsOn => _isOn;

    // -------------------------------------------------------
    class CachedRenderer
    {
        public Renderer renderer;
        public Material[] originalMaterials;
    }

    readonly List<CachedRenderer> _cachedRenderers = new();
    // -------------------------------------------------------

    void Reset()
    {
        spotLight = GetComponentInChildren<Light>();
    }

    void Start()
    {
        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>();

        CacheRenderers();

        var e = transform.localEulerAngles;
        _pitch = NormalizeAngle(e.x);
        _yaw = NormalizeAngle(e.y);

        ApplyLightState();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();

        if (_isOn && Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            _yaw += mx * sensitivity;
            _pitch -= my * sensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
        }
    }

    void CacheRenderers()
    {
        _cachedRenderers.Clear();

        foreach (var obj in revealObjects)
        {
            if (obj == null) continue;

            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var rend in renderers)
            {
                _cachedRenderers.Add(new CachedRenderer
                {
                    renderer = rend,
                    originalMaterials = rend.sharedMaterials
                });
            }
        }
    }

    public void Toggle() => SetState(!_isOn);

    public void SetState(bool on)
    {
        _isOn = on;

        ApplyLightState();
        ApplyMaterials();
        ApplyFlag();
    }

    // ── Light ────────────────────────────────────────────────
    void ApplyLightState()
    {
        if (spotLight != null)
            spotLight.enabled = _isOn;
    }

    // ── Materials ────────────────────────────────────────────
    void ApplyMaterials()
    {
        foreach (var entry in _cachedRenderers)
        {
            if (entry.renderer == null) continue;

            if (_isOn && revealMaterial != null)
            {
                int count = entry.renderer.sharedMaterials.Length;

                var mats = new Material[count];

                for (int i = 0; i < count; i++)
                    mats[i] = revealMaterial;

                entry.renderer.sharedMaterials = mats;
            }
            else
            {
                entry.renderer.sharedMaterials = entry.originalMaterials;
            }
        }
    }

    // ── Flag ─────────────────────────────────────────────────
    void ApplyFlag()
    {
        if (string.IsNullOrEmpty(flashlightFlag)) return;
        if (GameFlags.Instance == null) return;

        if (_isOn)
            GameFlags.Instance.SetFlag(flashlightFlag);
        else
            GameFlags.Instance.ClearFlag(flashlightFlag);
    }

    // ── Helpers ──────────────────────────────────────────────
    public void SetRotationEuler(Vector3 euler)
    {
        _pitch = NormalizeAngle(euler.x);
        _yaw = NormalizeAngle(euler.y);

        transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
    }

    public Vector3 GetRotationEuler() =>
        new Vector3(_pitch, _yaw, 0f);

    static float NormalizeAngle(float a)
    {
        a %= 360f;

        if (a > 180f)
            a -= 360f;

        return a;
    }
}