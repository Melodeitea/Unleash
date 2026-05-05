using System.Collections;
using UnityEngine;

public class InventoryControl : MonoBehaviour
{

	public GameObject inventoryScreen;
	public GameObject inventoryFade;
	public AudioSource inventoryOpen;
	public bool isOpen = false;
	public AudioSource inventoryClose;
	public bool canClose = false;


	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab) && isOpen == false && canClose == false)
		{
			// Debug.Log("Tab key was pressed.");

			isOpen = true;
			inventoryOpen.Play();
			inventoryFade.SetActive(true);
			StartCoroutine(InvOpen());
		}

		if (Input.GetKeyDown(KeyCode.Tab) && isOpen == true && canClose == true)
		{
			isOpen = false;
			inventoryClose.Play();
			inventoryFade.SetActive(true);
			StartCoroutine(InvClose());
		}
	}

	IEnumerator InvOpen()
	{
		yield return new WaitForSeconds(0.25f);
		inventoryScreen.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		inventoryFade.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		canClose = true;
	}

	IEnumerator InvClose()
	{
		yield return new WaitForSeconds(0.25f);
		inventoryScreen.SetActive(false);
		yield return new WaitForSeconds(0.25f);
		inventoryFade.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		canClose = false;
	}
}
