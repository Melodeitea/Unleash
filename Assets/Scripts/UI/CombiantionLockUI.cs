using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombinationLockUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Animator animator;              // assign if using anim triggers
	[SerializeField] private TextMeshProUGUI codeText;
	[SerializeField] private Button[] digitButtons;          // assign all 0–9 buttons here

	[Header("Settings")]
	[SerializeField] private float fadeSpeed = 5f;
	[SerializeField] private float successWaitTime = 1.5f;
	[SerializeField] private string fadeInTrigger = "FadeIn";
	[SerializeField] private string fadeOutTrigger = "FadeOut";
	[SerializeField] private bool useAnimatorForFade = false; // flip on if you want anim-driven fade

	private string _currentInput = "";
	private int _maxDigits;
	private Action<string> _onSubmit;
	private Action _onClose;
	private bool _lockedInput;
	private bool _isOpen;

	// Cursor state so we can restore it on close
	private CursorLockMode _prevLockMode;
	private bool _prevCursorVisible;

	private void Awake()
	{
		// Start fully hidden and non-interactive
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}

	private void Update()
	{
		// gameObject IS active while open — this fires correctly
		if (!_isOpen) return;
		if (Input.GetKeyDown(KeyCode.Escape))
			_onClose?.Invoke();
	}

	// ─── Public API ───────────────────────────────────────────────────────────

	public void Open(int digits, Action<string> onSubmit, Action onClose)
	{
		gameObject.SetActive(true); // ← must be FIRST, before any coroutine

		_maxDigits = digits;
		_onSubmit = onSubmit;
		_onClose = onClose;
		_currentInput = "";
		_lockedInput = false;
		_isOpen = true;

		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

		_prevLockMode = Cursor.lockState;
		_prevCursorVisible = Cursor.visible;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		WireDigitButtons();

		StopAllCoroutines();
		if (useAnimatorForFade && animator != null)
			animator.SetTrigger(fadeInTrigger);
		else
			StartCoroutine(Fade(1f));

		UpdateDisplay();
	}

	public void Close()
	{
		_isOpen = false;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		Cursor.lockState = _prevLockMode;
		Cursor.visible = _prevCursorVisible;

		StopAllCoroutines();
		if (useAnimatorForFade && animator != null)
			animator.SetTrigger(fadeOutTrigger);
		else
			StartCoroutine(Fade(0f, () => gameObject.SetActive(false))); // ← deactivate in callback
	}

	public void AddDigit(string digit)
	{
		if (_lockedInput) return;
		if (_currentInput.Length >= _maxDigits) return;

		_currentInput += digit;
		UpdateDisplay();

		if (_currentInput.Length == _maxDigits)
			_onSubmit?.Invoke(_currentInput);
	}

	public void ClearInput()
	{
		_currentInput = "";
		UpdateDisplay();
	}

	public void LockInput()
	{
		_lockedInput = true;
		canvasGroup.interactable = false; // visually disables buttons too
	}

	public void PlaySuccessAndExit(Action onExit)
	{
		StartCoroutine(SuccessRoutine(onExit));
	}

	// ─── Private ──────────────────────────────────────────────────────────────

	private void WireDigitButtons()
	{
		foreach (Button btn in digitButtons)
		{
			if (btn == null) continue;

			// Read the digit from the button's TMP child label
			var label = btn.GetComponentInChildren<TextMeshProUGUI>();
			if (label == null) continue;

			string digit = label.text.Trim();

			// Clear previous listeners to avoid double-registration
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() => AddDigit(digit));
		}
	}

	private void UpdateDisplay()
	{
		if (codeText != null)
			codeText.text = _currentInput.PadRight(_maxDigits, '_');
	}

	private IEnumerator SuccessRoutine(Action onExit)
	{
		yield return new WaitForSeconds(successWaitTime);
		onExit?.Invoke();
	}

	private IEnumerator Fade(float target, Action onDone = null)
	{
		float start = canvasGroup.alpha;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * fadeSpeed;
			canvasGroup.alpha = Mathf.Lerp(start, target, t);
			yield return null;
		}
		canvasGroup.alpha = target;
		onDone?.Invoke();
	}
}