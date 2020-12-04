using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PaintballNetworkManager : NetworkManager
{
	[SerializeField] private GameObject gameManagerPrefabReference;

	private GameManager gameManager;

	private bool gameSceneLoaded = false;

	[SerializeField] private int totalPlayersThatHaveConnected;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public override void OnStartHost()
	{
		/*
		 * 
		 *	totalPlayersThatHaveConnected is used to create unique player ids. 
		 *	since it's possible to start a game, connect players, and then return to the main menu and start a new game,
		 *	and because NetworkManager is persistent across scenes via DontDestroyOnLoad(), totalPlayersThatHaveConnected needs to be reset whenever a new game is started to be safe
		 * 
		 */
		totalPlayersThatHaveConnected = 0;

		base.OnStartHost();

		/*
		 *	without a coroutine to asynchronously wait for the scene to be loaded before instantiating the GameManager, 
		 *	it would end up getting spawned back in the MainMenu scene. weird quirk of Mirror/UNET
		 */
		StartCoroutine(WaitForGameSceneLoadedThenSpawnGameManager());

	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		#region Spawning logic

		Transform startPos = GetStartPosition();
		GameObject player = startPos != null
			? Instantiate(playerPrefab, startPos.position, startPos.rotation)
			: Instantiate(playerPrefab);

		#endregion

		NetworkServer.AddPlayerForConnection(conn, player);

		PlayerMainController newPlayerMainController = player.GetComponent<PlayerMainController>();

		/*
		 * tells the player to call it's local instance of GameManager.SetLocalPlayer, so that it's local instance of GameManager knows who the local player is
		 */
		newPlayerMainController.TargetRpc_SetLocalGameManagerLocalPlayerReferenceToSelf(conn);

		/*
		 * temporary implementation of SetCharacterUniqueReferenceHandler
		 *
		newPlayerMainController.SetCharacterUniqueReferenceHandler(player.GetComponentInChildren<CharacterUniqueReferenceHandler>());*/


		gameManager.Server_AddPlayerAndAssignTeam(player, totalPlayersThatHaveConnected);

		totalPlayersThatHaveConnected++;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		int playerId = conn.identity.gameObject.GetComponent<PlayerInfoHandler>().PlayerInfo.Id;

		base.OnServerDisconnect(conn);

		gameManager.Server_RemovePlayer(playerId);
		
	}

	private IEnumerator WaitForGameSceneLoadedThenSpawnGameManager()
	{
		while (!gameSceneLoaded)
		{
			yield return null;
		}

		GameObject newGameManager = Instantiate(gameManagerPrefabReference);
		gameManager = newGameManager.GetComponent<GameManager>();

		/*
		 * 
		 *	NetworkServer.Spawn creates an instance of the newGameManager on the clients as well
		 *	these clients don't have any authority over this network object
		 *	only the server has authority over it
		 *	thus, the clients cannot call any Cmds to the server via the GameManager itself 
		 *	
		 *	you'll notice the workaround to this is that PlayerMainController has Cmd_ methods (which can be called from the client because they have authority over their own player) 
		 *	which do nothing besides call Server_ methods on the GameManager
		 *	this is done for respawning, for example
		 * 
		 */
		NetworkServer.Spawn(newGameManager);

	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "Game")
		{
			gameSceneLoaded = true;
		}
		else
		{
			gameSceneLoaded = false;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}
}
