using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	#region Singleton pattern

	public static GameManager instance;
	public static GameManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindGameObjectWithTag("GameManager")?.GetComponent<GameManager>();
			}
			return instance;
		}
	}

	#endregion

	#region References

	private GeneralUIHandler generalUIHandler;
	private NetworkIdentity networkIdentity;

	#endregion

	/*
	 *	q:	
	 *		playerInfoHandlerList and syncedPlayerInfos look redundant, why is that?
	 * 
	 *	a:
	 *		the GameManager instance which exists on the server may need access to class references, not just the struct PlayerInfo.
	 *		
	 *		the custom class "PlayerInfoHandlerList" & references in general can't be synced over the server, 
	 *		
	 *		but the client still needs to know some basic info and player data like name and score for use in PlayerStatsUIHandler, so that's why syncedPlayerInfos exists in conjuction with playerInfoHandlerList
	 * 
	 */

	#region Internal Values

	[SerializeField] private PlayerInfoHandlerList playerInfoHandlerList = new PlayerInfoHandlerList(); //master player list which only exists on the server
	private SyncList<PlayerInfo> syncedPlayerInfos = new SyncList<PlayerInfo>(); //struct data which can be synced from server-client to clients

	[SerializeField] private List<int> teamAPlayerIds = new List<int>(); //list of player ids on teamA, only exists on the server
	[SerializeField] private List<int> teamBPlayerIds = new List<int>(); //list of player ids on teamB, only exists on the server

	[SyncVar(hook = nameof(OnTeamAPointsChanged))] private int teamAPoints = 0;
	[SyncVar(hook = nameof(OnTeamBPointsChanged))] private int teamBPoints = 0;

	public PlayerMainController LocalPlayerMainController;
	public PlayerInfoHandler LocalPlayerInfoHandler;

	public void SetLocalPlayer(GameObject player)
	{
		LocalPlayerMainController = player.GetComponent<PlayerMainController>();
		LocalPlayerInfoHandler = player.GetComponent<PlayerInfoHandler>();
	}

	#endregion

	private void Start()
	{
		generalUIHandler = GameObject.FindGameObjectWithTag("GeneralUIHandler").GetComponent<GeneralUIHandler>();
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		syncedPlayerInfos.Callback += OnSyncedPlayerInfosChanged;
	}

	private void OnTeamAPointsChanged(int oldValue, int newValue)
	{
		teamAPoints = newValue;
		OnTeamPointsChanged();
	}

	private void OnTeamBPointsChanged(int oldValue, int newValue)
	{
		teamBPoints = newValue;
		OnTeamPointsChanged();
	}

	private void OnTeamPointsChanged()
	{
		generalUIHandler.UpdateTeamScore(teamAPoints, teamBPoints);
	}

	/// <summary>
	/// Adds the newPlayer's PlayerInfoHandler component to the playerInfoHandlerList, 
	/// then calls Server_RefreshSyncedPlayerInfos so the clients can have the new player list etc
	/// </summary>
	/// <param name="newPlayer">player to be added</param>
	/// <param name="id">their unique id</param>
	[Server] //the [Server] attribute doesn't functionally add anything, it's a safety measure which prevents you from accidentally calling a server-only function from a client
	public void Server_AddPlayerAndAssignTeam(GameObject newPlayer, int id)
	{
		PlayerInfoHandler newPlayerInfoHandler = newPlayer.GetComponent<PlayerInfoHandler>();
		
		playerInfoHandlerList.Add(newPlayerInfoHandler, id);

		if (teamAPlayerIds.Count <= teamBPlayerIds.Count)
		{
			teamAPlayerIds.Add(id);
			newPlayerInfoHandler.PlayerInfo.Team = Team.A;
		}
		else
		{
			teamBPlayerIds.Add(id);
			newPlayerInfoHandler.PlayerInfo.Team = Team.B;
		}

		Server_RefreshSyncedPlayerInfos();
	}

	/// <summary>
	/// Removes the playerIdToBeRemoved from their team specific list (either teamAPlayerIds or teamBPlayerIds), 
	/// then removes playerIdToBeRemoved from the master list, playerInfoHandlerList, 
	/// and then calls on Server_RefreshedSyncedPlayerInfos so that the clients can have the updated player list & stats
	/// </summary>
	/// <param name="playerIdToBeRemoved"></param>
	[Server]
	public void Server_RemovePlayer(int playerIdToBeRemoved)
	{

		for (int i = 0; i < teamAPlayerIds.Count; i++)
		{
			PlayerInfoHandler playerInfoHandler = playerInfoHandlerList.GetByPlayerId(teamAPlayerIds[i]);

			if (playerInfoHandler != null && playerInfoHandler.PlayerInfo.Id == playerIdToBeRemoved && playerInfoHandler.PlayerInfo.Team == Team.A)
			{
				teamAPlayerIds.RemoveAt(i);
			}
		}

		for (int i = 0; i < teamBPlayerIds.Count; i++)
		{
			PlayerInfoHandler playerInfoHandler = playerInfoHandlerList.GetByPlayerId(teamBPlayerIds[i]);

			if (playerInfoHandler != null && playerInfoHandler.PlayerInfo.Id == playerIdToBeRemoved && playerInfoHandler.PlayerInfo.Team == Team.B)
			{
				teamBPlayerIds.RemoveAt(i);
			}
		}

		playerInfoHandlerList.RemoveByPlayerId(playerIdToBeRemoved);

		Server_RefreshSyncedPlayerInfos();
	}

	/// <summary>
	/// Removes all PlayerInfos from syncedPlayerInfos and then re-adds them using playerInfoHandlerList. 
	/// Server-only because playerInfoHandlerList only exists on the server
	/// </summary>
	[Server]
	private void Server_RefreshSyncedPlayerInfos()
	{
		/*
		 * 
		 *		q: 
		 *			does removing all syncedPlayerInfos and adding them all back cause excessive network traffic?
		 * 
		 *		a: 
		 *			if SyncList works like SyncVar, then they work on an update interval, (1/10th of a second by default) they aren't being updated everytime they change.
		 *			might need to double check Mirror documentation and see if a better implentation is nessecary
		 * 
		 */
		
		while (syncedPlayerInfos.Count > 0)
		{
			syncedPlayerInfos.RemoveAt(0);
		}

		for (int i = 0; i < playerInfoHandlerList.List.Count; i++)
		{
			syncedPlayerInfos.Add(playerInfoHandlerList.GetByIndex(i).PlayerInfo);
		}
	}

	/// <summary>
	/// SyncList Callback for syncedPlayerInfos. 
	/// See Mirror documentation for parameter information
	/// </summary>
	/// <param name="op"></param>
	/// <param name="index"></param>
	/// <param name="oldItem"></param>
	/// <param name="newItem"></param>
	private void OnSyncedPlayerInfosChanged(SyncList<PlayerInfo>.Operation op, int index, PlayerInfo oldItem, PlayerInfo newItem)
	{
		UpdatePlayerStatsUIHandler();
	}

	public void UpdatePlayerStatsUIHandler()
	{
		/*
		 *	q: 
		 *		why are you looping through syncedPlayerInfos to find out which player is on which team, 
		 *		instead of using teamAPlayerIds & teamBPlayerIds in conjuction with playerInfoHandlerList.GetPlayerById()?
		 * 
		 *	a: 
		 *		the hook / callback which calls this method (OnPlayerInfosChanged) will be used on both the server and the client. 
		 *		on the client, teamAPlayerIds, teamBPlayerIds & playerInfoHandlerList will all be empty / incomplete
		 *	
		 */
		
		List<PlayerInfo> teamAPlayerInfos = new List<PlayerInfo>();
		List<PlayerInfo> teamBPlayerInfos = new List<PlayerInfo>();

		for (int i = 0; i < syncedPlayerInfos.Count; i++)
		{
			if (syncedPlayerInfos[i].Team == Team.A)
			{
				teamAPlayerInfos.Add(syncedPlayerInfos[i]);
			}
			else
			{
				teamBPlayerInfos.Add(syncedPlayerInfos[i]);
			}
		}

		generalUIHandler?.UpdatePlayerStats(teamAPlayerInfos.ToArray(), teamBPlayerInfos.ToArray());
	}

	[Server]
	public void Server_RespawnPlayerById(int playerId)
	{
		playerInfoHandlerList.GetByPlayerId(playerId).GetComponent<PlayerMainController>().Server_ResetToDefaults();
	}

	public void Server_PlayerDied(Team deadPlayerTeam)
	{
		switch (deadPlayerTeam)
		{
			case Team.A:
				teamBPoints++;
				break;

			case Team.B:
				teamAPoints++;
				break;
		}
	}

	public void NetworkSafe_SelectLocalPlayerFirearm(FirearmUniqueIdentifier firearmUniqueIdentifier)
	{
		LocalPlayerMainController.WeaponHandler.NetworkSafe_SelectFirearm(firearmUniqueIdentifier);
	}
}
