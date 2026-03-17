using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightSwitch : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag used for the player (object that can press E).")]
    public string playerTag = "Player";
    [Tooltip("Key to interact with the switch.")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Color / Targets")]
    [Tooltip("Material that will be applied to referenced objects tagged 'Red'.")]
    public Material selectedMaterial;
    [Tooltip("List of objects referenced by this switch. Only those with tag 'Red' will get recolored.")]
    public List<GameObject> referencedObjects = new List<GameObject>();
    [Tooltip("List of objects that are hidden by default and should appear when the switch is activated.")]
    public List<GameObject> hiddenReferences = new List<GameObject>();

    [Header("Screen Overlay")]
    [Tooltip("Full-screen UI Image used to tint the screen (assign a red Image in the Canvas).")]
    public Image screenOverlay;
    [Tooltip("Overlay alpha when active (0..1).")]
    [Range(0f, 1f)]
    public float overlayAlpha = 0.25f;

    bool _playerInRange;
    bool _isActive;

    // Keep original materials so we can restore them on toggle off
    readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    void Start()
    {
        // Cache original materials for referenced objects (renderers)
        foreach (var go in referencedObjects)
        {
            if (go == null) continue;
            var rend = go.GetComponent<Renderer>();
            if (rend != null && !_originalMaterials.ContainsKey(rend))
            {
                _originalMaterials[rend] = rend.sharedMaterials;
            }
        }

        // Ensure hiddenReferences are hidden at start
        foreach (var go in hiddenReferences)
        {
            if (go == null) continue;
            go.SetActive(false);
        }

        // Initialize overlay (make transparent)
        if (screenOverlay != null)
        {
            var c = screenOverlay.color;
            c.a = 0f;
            screenOverlay.color = c;
            // leave the GameObject active (so color changes are visible); ensure raycast target is off on the Image
        }
    }

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            Toggle();
        }
    }

    void Toggle()
    {
        _isActive = !_isActive;
        ApplyColor(_isActive);
        SetHidden(_isActive);
        SetOverlay(_isActive);
    }

    void ApplyColor(bool apply)
    {
        foreach (var go in referencedObjects)
        {
            if (go == null) continue;

            // Only affect objects that are tagged "Red"
            if (!go.CompareTag("Red")) continue;

            var rend = go.GetComponent<Renderer>();
            if (rend == null) continue;

            if (apply)
            {
                if (selectedMaterial == null) continue;
                // Apply the selectedMaterial to all material slots
                var mats = rend.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = selectedMaterial;
                }
                rend.materials = mats;
            }
            else
            {
                // Restore original materials if we cached them
                if (_originalMaterials.TryGetValue(rend, out var orig))
                {
                    rend.sharedMaterials = orig;
                }
            }
        }
    }

    void SetHidden(bool show)
    {
        foreach (var go in hiddenReferences)
        {
            if (go == null) continue;
            go.SetActive(show);
        }
    }

    void SetOverlay(bool show)
    {
        if (screenOverlay == null) return;
        var c = screenOverlay.color;
        c.a = show ? overlayAlpha : 0f;
        screenOverlay.color = c;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
        }
    }
}
