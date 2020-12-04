using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneralUIHandler : MonoBehaviour
{
    private PlayerStatsUIHandler playerStats;
    private RespawnScreenUIHandler respawnScreen;

    private TeamScoreUIHandler teamScore;
    private HealthUIHandler health;

    private void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("PlayerStatsUIHandler").GetComponent<PlayerStatsUIHandler>();
        respawnScreen = GameObject.FindGameObjectWithTag("RespawnScreenUIHandler").GetComponent<RespawnScreenUIHandler>();

        teamScore = GameObject.FindGameObjectWithTag("TeamScoreUIHandler").GetComponent<TeamScoreUIHandler>();
        health = GameObject.FindGameObjectWithTag("HealthUIHandler").GetComponent<HealthUIHandler>();
    }

    private void Update()
	{
        playerStats.SetVisibleState(Input.GetKey(KeyCode.Tab));
                
        if (GameManager.Instance != null && GameManager.Instance.LocalPlayerMainController != null) //this takes some time to be set by the server
        {
            respawnScreen.SetVisibleState(GameManager.Instance.LocalPlayerMainController.Health.Death);
        }
        
    }

    public void UpdatePlayerStats(PlayerInfo[] teamA, PlayerInfo[] teamB)
	{
        playerStats.UpdateUI(teamA, teamB);
	}

    public void UpdateTeamScore(int teamAScore, int teamBScore)
	{
        teamScore.UpdateUI(teamAScore, teamBScore);
	}

    public void UpdateHealth(float value)
	{
        health.UpdateUI(value);
	}
}
