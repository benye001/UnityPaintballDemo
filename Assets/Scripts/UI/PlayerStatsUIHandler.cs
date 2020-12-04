using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerStatsUIHandler : MonoBehaviour
{
	#region References

	private CanvasGroup canvasGroup;

	[SerializeField] private ConnectionListUIHandler teamAConnectionList, teamBConnectionList;

	#endregion

	private void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}

	public void UpdateUI(PlayerInfo[] teamA, PlayerInfo[] teamB)
	{
		teamAConnectionList.UpdateUI(teamA);
		teamBConnectionList.UpdateUI(teamB);
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
}
