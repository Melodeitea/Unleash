using UnityEngine;

public class FlagRevealObject : MonoBehaviour
{
    [Header("Flag")]
    [SerializeField] private string requiredFlag;

    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    Renderer[] _renderers;
    Collider[] _colliders;

    void Awake()
    {
        if (targetObject == null) return;

        _renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        _colliders = targetObject.GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        UpdateState();
    }

    void Update()
    {
        UpdateState();
    }

    void UpdateState()
    {
        if (targetObject == null || string.IsNullOrEmpty(requiredFlag))
            return;

        bool visible =
            GameFlags.Instance != null &&
            GameFlags.Instance.GetFlag(requiredFlag);

        foreach (var r in _renderers)
            r.enabled = visible;

        foreach (var c in _colliders)
            c.enabled = visible;
    }
}