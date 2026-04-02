using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a document GameObject (with an isTrigger collider).
/// Player presses E when in range to open the document UI (assign `readingUIRoot`).
/// While the document UI is open, pressing Space toggles a side-text view that shows a numeric representation
/// (ASCII codes) of the main text for readability.
/// Optional: assign components to disable while reading (e.g., player controller scripts).
/// </summary>
[DisallowMultipleComponent]
public class DocumentReader : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag used for the player.")]
    public string playerTag = "Player";
    [Tooltip("Key to interact with the document.")]
    public KeyCode interactKey = KeyCode.E;
    [Tooltip("Key to toggle the numeric side text while viewing.")]
    public KeyCode toggleSideKey = KeyCode.Space;

    [Header("UI")]
    [Tooltip("Root GameObject of the document UI (assign in inspector). This object will be activated/deactivated.")]
    public GameObject readingUIRoot;
    [Tooltip("Main TextMeshProUGUI inside the reading UI that shows the readable document text.")]
    public TextMeshProUGUI mainText;
    [Tooltip("Side TextMeshProUGUI that will show numeric representation. This object will be toggled.")]
    public TextMeshProUGUI sideText;

    [Header("Optional")]
    [Tooltip("Components to disable while the player is reading (e.g., movement scripts).")]
    public Behaviour[] componentsToDisable;

    bool _playerInRange;
    bool _isViewing;
    bool _sideVisible;
    bool[] _previousComponentStates;

    void Awake()
    {
        // Ensure UI is hidden at start (do not assume inspector state)
        if (readingUIRoot != null)
            readingUIRoot.SetActive(false);
        if (sideText != null && sideText.gameObject.activeSelf)
            sideText.gameObject.SetActive(false);

        if (componentsToDisable != null && componentsToDisable.Length > 0)
            _previousComponentStates = new bool[componentsToDisable.Length];
    }

    void Update()
    {
        // Open/close document
        if (Input.GetKeyDown(interactKey))
        {
            if (_isViewing)
            {
                CloseDocument();
            }
            else if (_playerInRange)
            {
                OpenDocument();
            }
        }

        // Toggle numeric side text only when viewing
        if (_isViewing && Input.GetKeyDown(toggleSideKey))
        {
            ToggleSideText();
        }
    }

    void OpenDocument()
    {
        if (readingUIRoot == null)
        {
            Debug.LogWarning($"DocumentReader on '{name}' has no readingUIRoot assigned.");
            return;
        }

        _isViewing = true;
        _sideVisible = false;

        readingUIRoot.SetActive(true);
        if (sideText != null)
            sideText.gameObject.SetActive(false);

        // show cursor so player can read / interact with UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // disable requested components
        if (componentsToDisable != null)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                var comp = componentsToDisable[i];
                if (comp == null) continue;
                _previousComponentStates[i] = comp.enabled;
                comp.enabled = false;
            }
        }
    }

    void CloseDocument()
    {
        _isViewing = false;
        _sideVisible = false;

        if (readingUIRoot != null)
            readingUIRoot.SetActive(false);
        if (sideText != null)
            sideText.gameObject.SetActive(false);

        // restore cursor (locked for gameplay)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // restore components
        if (componentsToDisable != null)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                var comp = componentsToDisable[i];
                if (comp == null) continue;
                // If we saved previous state, restore it; otherwise enable by default
                if (_previousComponentStates != null && i < _previousComponentStates.Length)
                    comp.enabled = _previousComponentStates[i];
                else
                    comp.enabled = true;
            }
        }
    }

    void ToggleSideText()
    {
        if (sideText == null)
        {
            Debug.LogWarning($"DocumentReader on '{name}' has no sideText assigned.");
            return;
        }

        _sideVisible = !_sideVisible;
        if (_sideVisible)
        {
            // populate numeric representation from mainText if available
            var source = mainText != null ? mainText.text : string.Empty;
            sideText.text = ConvertToNumeric(source);
        }

        sideText.gameObject.SetActive(_sideVisible);
    }

    // Convert text into a readable numeric representation (ASCII codes grouped per-character).
    // Example: "Hi" -> "72 105"
    string ConvertToNumeric(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var sb = new StringBuilder(input.Length * 3);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\n' || c == '\r')
            {
                sb.AppendLine();
                continue;
            }

            // represent space as a visible token (optional)
            if (c == ' ')
            {
                sb.Append("?"); // visible space marker
            }
            else
            {
                sb.Append(((int)c).ToString());
            }

            // separate with a single space
            if (i < input.Length - 1)
                sb.Append(' ');
        }
        return sb.ToString();
    }

    // Trigger range detection (document must have a trigger collider)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            _playerInRange = false;
    }
}