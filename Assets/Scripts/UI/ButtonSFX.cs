using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
	[SerializeField] private AudioSource sfx;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() => sfx?.Play());
	}
}