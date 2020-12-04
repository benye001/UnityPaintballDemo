using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamScoreUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI teamAText, teamBText;

    public void UpdateUI(int teamAScore, int teamBScore)
	{
		teamAText.text = "Team A: " + teamAScore;
		teamBText.text = "Team B: " + teamBScore;
	}
}
