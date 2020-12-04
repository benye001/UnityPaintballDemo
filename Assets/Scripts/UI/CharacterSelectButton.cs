using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void CharacterSelectButtonClicked(CharacterName name);

[RequireComponent(typeof(Button))]
public class CharacterSelectButton : MonoBehaviour
{
    public event CharacterSelectButtonClicked CharacterSelectButtonClickedHandler;

    private Button button;

    [SerializeField] private CharacterName characterName;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
	{
        CharacterSelectButtonClickedHandler?.Invoke(characterName);
	}
}
