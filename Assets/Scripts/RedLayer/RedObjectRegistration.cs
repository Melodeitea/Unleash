using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RedObjectRegistration : MonoBehaviour
{
	[SerializeField] private Material defaultMaterial;
	[SerializeField] private Material revealedMaterial;

	private Renderer rend;

	private void Awake()
	{
		rend = GetComponent<Renderer>();
		RedLayerManager.Instance.Register(this);
	}

	private void OnDestroy()
	{
		if (RedLayerManager.Instance != null)
		{
			RedLayerManager.Instance.Deregister(this);
		}
	}

	public void SetState(bool active)
	{
		if (rend == null) return;

		rend.material = active ? revealedMaterial : defaultMaterial;
	}
}