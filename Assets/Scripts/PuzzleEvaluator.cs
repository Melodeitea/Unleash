using UnityEngine;

/// <summary>
/// Base class for modular puzzle evaluators. Inherit and implement EvaluateAndSolveIfMatch().
/// Auto-registers with PuzzleManager on enable.
/// </summary>
[DisallowMultipleComponent]
public abstract class PuzzleEvaluator : MonoBehaviour
{
    [Tooltip("How often (seconds) to evaluate. 0 = evaluate only when requested.")]
    public float checkInterval = 0.5f;

    Puzzle _puzzle;
    float _timer;

    protected virtual void Awake()
    {
        _puzzle = GetComponent<Puzzle>();
        if (_puzzle == null)
            Debug.LogWarning($"{GetType().Name} on '{name}' expects a Puzzle component on the same GameObject.");
    }

    protected virtual void OnEnable()
    {
        PuzzleManager.RegisterEvaluator(this);
    }

    protected virtual void OnDisable()
    {
        PuzzleManager.UnregisterEvaluator(this);
    }

    // central per-evaluator timer — derived classes only implement EvaluateAndSolveIfMatch()
    void Update()
    {
        if (checkInterval <= 0f) return;
        _timer += Time.unscaledDeltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            // don't call if puzzle already solved (derived should respect Puzzle.isSolved if needed)
            EvaluateAndSolveIfMatch();
        }
    }

    /// <summary>
    /// Implement this to evaluate the puzzle conditions and call Puzzle.Solve() when satisfied.
    /// Return true if the call caused the puzzle to be solved this invocation.
    /// </summary>
    public abstract bool EvaluateAndSolveIfMatch();
}