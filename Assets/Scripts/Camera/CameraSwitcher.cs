using System.Collections.Generic;
using UnityEngine;

public static class CameraSwitcher
{
    private static ZoneCameraController _controller;
    private static readonly List<CameraZoneData> _stack = new();

    public static CameraZoneData ActiveZone =>
        _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

    public static bool HasPreviousZone => _stack.Count > 1;

    public static void SetController(ZoneCameraController controller)
    {
        _controller = controller;
        if (_controller != null && _stack.Count > 0)
            _controller.TransitionToZone(ActiveZone);
    }

    // Player enters a zone
    public static void EnterZone(CameraZoneData zone)
    {
        if (_controller == null)
        {
            Debug.LogWarning("[CameraSwitcher] No active ZoneCameraController.");
            return;
        }

        _stack.Remove(zone); // prevent duplicates on re-entry
        _stack.Add(zone);    // last entered = top = highest priority

        _controller.TransitionToZone(ActiveZone);
    }

    // Player exits a specific zone — removes it from anywhere in the stack
    public static void ExitZone(CameraZoneData zone)
    {
        _stack.Remove(zone);

        if (_stack.Count > 0)
            _controller?.TransitionToZone(ActiveZone);
    }

    // Keep snap for scene resets
    public static void SnapToZone(CameraZoneData zone)
    {
        _stack.Clear();
        _stack.Add(zone);
        _controller?.SnapToZone(zone);
    }

    public static void Clear() => _stack.Clear();
}