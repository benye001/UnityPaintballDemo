using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectionListItemUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    public void UpdateUI(PlayerInfo playerInfo)
	{
        playerNameText.text = playerInfo.Name;
	}

    public void UpdateUI()
    {
        playerNameText.text = "-";
    }
}
