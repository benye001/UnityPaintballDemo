using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LobbyListItemUIHandler : MonoBehaviour
{
    public int Index;
    public event ListItemClick ListItemClickHandler;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyDetailsText;

    private Button button;

	private void Start()
	{
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
	}

	public void UpdateUI(string lobbyName, string lobbyDetails)
    {
        lobbyNameText.text = lobbyName;
        lobbyDetailsText.text = lobbyDetails;
    }

    private void OnClick()
	{
        ListItemClickHandler?.Invoke(Index);
	}
}
