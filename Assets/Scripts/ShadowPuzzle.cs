using UnityEngine;

[RequireComponent(typeof(Puzzle))]
public class ShadowPuzzle : MonoBehaviour
{
	[Header("Target surface (must have MeshFilter + MeshRenderer)")]
	public MeshRenderer targetRenderer;
	public MeshFilter targetMeshFilter;

	[Header("Reference pattern (grayscale): white = lit required, black = shadow required)")]
	public Texture2D referencePattern;

	[Header("Sampling")]
	[Tooltip("Number of samples horizontally")]
	public int sampleX = 32;
	[Tooltip("Number of samples vertically")]
	public int sampleY = 32;
	[Range(0f, 1f)]
	[Tooltip("Minimum fraction of matching samples to consider puzzle solved")]
	public float matchThreshold = 0.85f;

	[Header("Occluder")]
	public LayerMask occluderMask = ~0; // default: everything

	[Header("Sampling timing")]
	[Tooltip("How often (seconds) to evaluate the pattern. 0 = evaluate only when requested.")]
	public float checkInterval = 0.5f;

	[Header("Flashlight (optional, will try to find one)")]
	public Flashlight flashlight;

	// internal
	float _timer;
	Puzzle _puzzle;

	void Reset()
	{
		targetRenderer = GetComponentInChildren<MeshRenderer>();
		targetMeshFilter = targetRenderer ? targetRenderer.GetComponent<MeshFilter>() : null;
	}

	void Awake()
	{
		_puzzle = GetComponent<Puzzle>();
		if (flashlight == null)
			flashlight = FindObjectOfType<Flashlight>();
		if (targetRenderer == null)
			Debug.LogError($"{nameof(ShadowPuzzle)} on '{name}' needs a targetRenderer assigned.");
		if (referencePattern == null)
			Debug.LogWarning($"{nameof(ShadowPuzzle)} on '{name}' has no referencePattern set.");
	}

	void Update()
	{
		if (checkInterval <= 0f) return;

		_timer += Time.unscaledDeltaTime;
		if (_timer >= checkInterval)
		{
			_timer = 0f;
			EvaluateAndSolveIfMatch();
		}
	}

	// Public API: evaluate the current shadow pattern and return match ratio
	public float EvaluatePattern()
	{
		if (targetRenderer == null || targetMeshFilter == null || referencePattern == null)
			return 0f;

		if (flashlight == null || flashlight.spotLight == null)
			return 0f;

		var light = flashlight.spotLight;
		var lightPos = light.transform.position;
		var lightDir = light.transform.forward;
		float halfAngle = light.spotAngle * 0.5f * Mathf.Deg2Rad;

		var mesh = targetMeshFilter.sharedMesh;
		if (mesh == null)
			return 0f;

		int total = 0;
		int matches = 0;

		// mesh bounds size in local space
		var bounds = mesh.bounds;
		Vector3 boundsSize = bounds.size;
		Vector3 boundsCenter = bounds.center;

		// iterate grid in [0,1] UV-like coordinates
		for (int y = 0; y < sampleY; y++)
		{
			float v = (y + 0.5f) / sampleY;
			for (int x = 0; x < sampleX; x++)
			{
				float u = (x + 0.5f) / sampleX;
				// map u,v to local point on the mesh plane.
				// Assumes target mesh is a quad-like mesh oriented in local XY plane.
				Vector3 localPoint = new Vector3(
					(u - 0.5f) * boundsSize.x + boundsCenter.x,
					(v - 0.5f) * boundsSize.y + boundsCenter.y,
					boundsCenter.z
				);
				Vector3 worldPoint = targetRenderer.transform.TransformPoint(localPoint);

				// check if point is inside light cone
				Vector3 toPoint = (worldPoint - lightPos);
				float dist = toPoint.magnitude;
				if (dist <= 0.0001f) continue;
				Vector3 dir = toPoint / dist;
				bool withinCone = Vector3.Dot(dir, lightDir) > Mathf.Cos(halfAngle);

				bool lit = false;
				if (withinCone)
				{
					// raycast to see if occluded
					if (!Physics.Raycast(lightPos, dir, dist - 0.01f, occluderMask, QueryTriggerInteraction.Ignore))
						lit = true;
				}

				// sample reference pattern at uv (u,v) (invert v if needed depending on texture)
				Color refCol = referencePattern.GetPixelBilinear(u, v);
				bool shouldBeLit = refCol.grayscale > 0.5f;

				if (lit == shouldBeLit) matches++;
				total++;
			}
		}

		if (total == 0) return 0f;
		return (float)matches / total;
	}

	// Evaluate and call Puzzle.Solve() when pattern matches threshold
	public bool EvaluateAndSolveIfMatch()
	{
		if (_puzzle == null || _puzzle.isSolved) return false;
		float ratio = EvaluatePattern();
		if (ratio >= matchThreshold)
		{
			_puzzle.Solve();
			return true;
		}
		return false;
	}

#if UNITY_EDITOR
	// Editor helper to debug visualize sampling points
	void OnDrawGizmosSelected()
	{
		if (targetRenderer == null || targetMeshFilter == null || flashlight == null) return;
		var mesh = targetMeshFilter.sharedMesh;
		if (mesh == null) return;
		var bounds = mesh.bounds;
		Vector3 boundsSize = bounds.size;
		Vector3 boundsCenter = bounds.center;
		Gizmos.color = Color.yellow;
		for (int y = 0; y < Mathf.Min(sampleY, 16); y++)
		{
			float v = (y + 0.5f) / sampleY;
			for (int x = 0; x < Mathf.Min(sampleX, 16); x++)
			{
				float u = (x + 0.5f) / sampleX;
				Vector3 localPoint = new Vector3(
					(u - 0.5f) * boundsSize.x + boundsCenter.x,
					(v - 0.5f) * boundsSize.y + boundsCenter.y,
					boundsCenter.z
				);
				Vector3 worldPoint = targetRenderer.transform.TransformPoint(localPoint);
				Gizmos.DrawSphere(worldPoint, Mathf.Max(0.01f, Mathf.Min(boundsSize.x, boundsSize.y) * 0.01f));
			}
		}
	}
#endif
}
