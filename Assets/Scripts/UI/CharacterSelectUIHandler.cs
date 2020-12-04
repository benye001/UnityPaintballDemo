using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectUIHandler : MonoBehaviour
{
	private void Start()
	{
		CharacterSelectButton[] characterSelectButtons = GetComponentsInChildren<CharacterSelectButton>();
		for(int i = 0; i < characterSelectButtons.Length; i++)
		{
			characterSelectButtons[i].CharacterSelectButtonClickedHandler += OnCharacterSelectClicked;
		}
	}

	private void OnCharacterSelectClicked(CharacterName characterName)
	{
		GameManager.Instance.LocalPlayerMainController.AnimationHandler.NetworkSafe_SetSelectedCharacterName(characterName);
	}
}
