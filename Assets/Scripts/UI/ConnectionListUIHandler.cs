using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionListUIHandler : MonoBehaviour
{
	#region References

	[SerializeField] private GameObject verticalLayoutGroup;
	private ConnectionListItemUIHandler[] connectionListItemUIHandlers;

	#endregion

	private void Start()
	{
		connectionListItemUIHandlers = verticalLayoutGroup.GetComponentsInChildren<ConnectionListItemUIHandler>();
	}

	public void UpdateUI(PlayerInfo[] playerInfos)
	{
		for (int i = 0; i < connectionListItemUIHandlers.Length; i++)
		{
			if (playerInfos.Length > i)
			{
				connectionListItemUIHandlers[i].UpdateUI(playerInfos[i]);
			}
			else
			{
				connectionListItemUIHandlers[i].UpdateUI();
			}

			
		}
	}
}
