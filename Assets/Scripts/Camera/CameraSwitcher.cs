using System.Collections.Generic;
using UnityEngine;

public static class CameraSwitcher
{
    private static ZoneCameraController _controller;
    private static readonly List<CameraZoneData> _stack = new();

    public static CameraZoneData ActiveZone => GetHighestPriority();

    public static bool HasPreviousZone => _stack.Count > 1;

    public static void SetController(ZoneCameraController controller)
    {
        _controller = controller;
        if (_controller != null && _stack.Count > 0)
            _controller.TransitionToZone(ActiveZone);
    }

    public static void EnterZone(CameraZoneData zone)
    {
        if (_controller == null)
        {
            Debug.LogWarning("[CameraSwitcher] No active ZoneCameraController.");
            return;
        }

        _stack.Remove(zone);
        _stack.Add(zone);

        _controller.TransitionToZone(ActiveZone);   // highest priority, not just last
    }

    public static void ExitZone(CameraZoneData zone)
    {
        _stack.Remove(zone);

        if (_stack.Count > 0)
            _controller?.TransitionToZone(ActiveZone);
    }

    public static void SnapToZone(CameraZoneData zone)
    {
        _stack.Clear();
        _stack.Add(zone);
        _controller?.SnapToZone(zone);
    }

    public static void Clear() => _stack.Clear();

    // ── Returns the zone with the highest priority value in the stack ──
    private static CameraZoneData GetHighestPriority()
    {
        if (_stack.Count == 0) return null;

        CameraZoneData best = _stack[0];
        for (int i = 1; i < _stack.Count; i++)
            if (_stack[i].priority > best.priority)
                best = _stack[i];

        return best;
    }
}