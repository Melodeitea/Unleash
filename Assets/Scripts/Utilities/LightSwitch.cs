using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script defines a light switch that can be toggled by the player when they are in range.
// When the red layer is enabled this optionally notifies PuzzleSystem by adding/removing a puzzle flag id.
public class LightSwitch : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag used for the player (object that can press E).")]
    public string playerTag = "Player";
    [Tooltip("Key to interact with the switch.")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Color / Targets")]
    [Tooltip("Material that will be applied to referenced objects.")]
    public Material selectedMaterial;
    [Tooltip("List of objects referenced by this switch. These objects will be recolored on toggle.")]
    public List<GameObject> referencedObjects = new List<GameObject>();
    [Tooltip("List of objects that are hidden by default and should appear when the switch is activated.")]
    public List<GameObject> hiddenReferences = new List<GameObject>();

    [Header("Puzzle integration")]
    [Tooltip("Optional puzzle-flag id to add to PuzzleSystem when red-layer is ON. Leave empty to disable automatic puzzle notifications.")]
    public string puzzleFlagOnRed;

    [Header("Screen Overlay (optional)")]
    [Tooltip("Full-screen UI Image used to tint the screen (assign a red Image in the Canvas). Leave null to disable overlay.")]
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
                // Cache sharedMaterials so we can restore them exactly
                _originalMaterials[rend] = rend.sharedMaterials;
            }

            // Ensure each referenced object has a RedMarker (so puzzles can query it)
            if (go != null && go.GetComponent<RedMarker>() == null)
                go.AddComponent<RedMarker>();
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
            // ensure overlay does not block raycasts if used
            screenOverlay.raycastTarget = false;
        }
    }

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(interactKey))
            Toggle();
    }

    void Toggle()
    {
        _isActive = !_isActive;
        ApplyColor(_isActive);
        SetHidden(_isActive);
        SetOverlay(_isActive);
        NotifyPuzzleSystem(_isActive);
    }

    void ApplyColor(bool apply)
    {
        foreach (var go in referencedObjects)
        {
            if (go == null) continue;

            var rend = go.GetComponent<Renderer>();
            if (rend == null) continue;

            if (apply)
            {
                if (selectedMaterial == null) continue;

                // Build new sharedMaterials array filled with the selected material
                var orig = rend.sharedMaterials;
                var newMats = new Material[orig.Length];
                for (int i = 0; i < newMats.Length; i++)
                    newMats[i] = selectedMaterial;
                rend.sharedMaterials = newMats;

                // mark the object as red for puzzles
                var marker = go.GetComponent<RedMarker>() ?? go.AddComponent<RedMarker>();
                marker.IsRed = true;
            }
            else
            {
                // Restore original materials if we cached them
                if (_originalMaterials.TryGetValue(rend, out var orig))
                {
                    rend.sharedMaterials = orig;
                }

                // clear red marker
                var marker = go.GetComponent<RedMarker>();
                if (marker != null) marker.IsRed = false;
            }
        }
    }

    void SetHidden(bool show)
    {
        foreach (var go in hiddenReferences)
            if (go != null) go.SetActive(show);
    }

    void SetOverlay(bool show)
    {
        if (screenOverlay == null) return;
        var c = screenOverlay.color;
        c.a = show ? overlayAlpha : 0f;
        screenOverlay.color = c;
    }

    void NotifyPuzzleSystem(bool redOn)
    {
        if (string.IsNullOrEmpty(puzzleFlagOnRed)) return;
        var ps = FindObjectOfType<PuzzleSystem>();
        if (ps == null) return;

        if (redOn)
            ps.AddItem(puzzleFlagOnRed);
        else
            ps.RemoveItem(puzzleFlagOnRed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) _playerInRange = false;
    }
}
