using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class RespawnScreenUIHandler : MonoBehaviour
{
	
	#region References

	private CanvasGroup canvasGroup;
	[SerializeField] private Button spawnButton;

	private FirearmSelectUIHandler firearmSelect;

	#endregion

	private void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();

		firearmSelect = GameObject.FindGameObjectWithTag("FirearmSelectUIHandler").GetComponent<FirearmSelectUIHandler>();

		spawnButton.onClick.AddListener(OnSpawnButtonClicked);
	}

	private void OnSpawnButtonClicked()
	{
		GameManager.Instance.LocalPlayerMainController.RespawnPlayerNetworkSafe();
	}

	public void SetVisibleState(bool state)
	{
		if (state == true)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
		else
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	private void OnFirearmSelectionChanged(AbstractFirearm newFirearm)
	{
		
	}
}
