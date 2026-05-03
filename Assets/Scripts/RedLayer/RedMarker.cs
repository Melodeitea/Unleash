using UnityEngine;
using System;

/// <summary>
/// Small marker component that indicates whether an object is currently 'red' (enabled by Woman in Black).
/// Puzzles can query this component to decide usability. Exposes an event when the flag changes.
/// </summary>
public class RedMarker : MonoBehaviour
{
    [SerializeField]
    bool isRed;

    /// <summary>
    /// True when the object was toggled red by a LightSwitch (or other system).
    /// </summary>
    public bool IsRed
    {
        get => isRed;
        set
        {
            if (isRed == value) return;
            isRed = value;
            OnRedChanged?.Invoke(isRed);
        }
    }

    /// <summary>
    /// Event invoked when IsRed changes. Passes the new value.
    /// </summary>
    public event Action<bool> OnRedChanged;
}