using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuGeneralUIHandler : MonoBehaviour
{
	#region Steamworks Callbacks

	protected Callback<LobbyMatchList_t> LobbyMatchListReceived;

	#endregion

	//set by OnLobbyMatchListReceived, serialized for editor debugging
	[SerializeField] private CSteamID[] lobbyMatchList;

	private NetworkManager networkManager;
	private LobbyListUIHandler lobbyList;

	[Header("Steamworks")]
	[SerializeField] private Button hostLobbyButton;
    [SerializeField] private Button findLobbiesButton;

	[Header("LAN")]
	[SerializeField] private Button hostLANButton;
	[SerializeField] private Button joinLANButton;

	[SerializeField] private Button toggleLANButton;

	private void Start()
	{
		GameObject networkManagerGameObject = GameObject.FindGameObjectWithTag("NetworkManager");

		if (networkManagerGameObject == null)
		{
			Debug.LogError("No NetworkManager found. Make sure start from the Splash scene");
		}
		else
		{
			networkManager = networkManagerGameObject.GetComponent<NetworkManager>();
		}

		lobbyList = GameObject.FindGameObjectWithTag("LobbyListUIHandler").GetComponent<LobbyListUIHandler>();

		if (PlayerPrefs.GetInt(PlayerPrefKeys.LAN_MODE) == 0)
		{
			hostLobbyButton.interactable = true;
			findLobbiesButton.interactable = true;

			hostLANButton.interactable = false;
			joinLANButton.interactable = false;
		}
		else
		{
			hostLobbyButton.interactable = false;
			findLobbiesButton.interactable = false;

			hostLANButton.interactable = true;
			joinLANButton.interactable = true;
		}

		#region Event linking

		hostLobbyButton.onClick.AddListener(OnHostLobbyButtonClick);
		findLobbiesButton.onClick.AddListener(OnFindLobbiesButtonClick);

		hostLANButton.onClick.AddListener(OnHostLANButtonClick);
		joinLANButton.onClick.AddListener(OnJoinLANButtonClick);

		toggleLANButton.onClick.AddListener(OnToggleLANButtonClick);

		lobbyList.ListItemClickHandler += OnLobbyListItemClick;

		#endregion

		if (SteamManager.Initialized)
		{
			LobbyMatchListReceived = Callback<LobbyMatchList_t>.Create(OnLobbyMatchListReceived);
		}
	}

	private void OnHostLobbyButtonClick()
	{
		networkManager.StartHost();
	}

	private void OnFindLobbiesButtonClick()
	{
		SteamMatchmaking.RequestLobbyList();
	}

	private void OnHostLANButtonClick()
	{
		//networkManager.transport
		networkManager.StartHost();
	}

	private void OnJoinLANButtonClick()
	{
		networkManager.networkAddress = "localhost";
		networkManager.StartClient();
	}

	private void OnToggleLANButtonClick()
	{
		int LAN_MODE = PlayerPrefs.GetInt(PlayerPrefKeys.LAN_MODE);

		if (LAN_MODE == 0)
		{
			LAN_MODE = 1;
		}
		else
		{
			LAN_MODE = 0;
		}

		PlayerPrefs.SetInt(PlayerPrefKeys.LAN_MODE, LAN_MODE);
		
		SceneManager.LoadScene("Splash");
	}

	/// <summary>
	/// Delegate method which is invoked by LobbyListUIHandler
	/// </summary>
	/// <param name="index"> index of LobbyListItemUIHandler which was clicked, from 0 to lobbyMatchList.Length </param>
	private void OnLobbyListItemClick(int index)
	{
		networkManager.networkAddress = lobbyMatchList[index].m_SteamID.ToString();
		networkManager.StartClient();
	}

	/// <summary>
	/// Steamworks callback which internally loads all the lobbies and their ids and their info into the SteamMatchmaking instance, 
	/// and passes in the # of lobbies which match the set lobby filters (none set, by default this means it will return games which haven't already started)
	/// from 0 to LobbyMatchList_t.m_nLobbiesMatching
	/// </summary>
	/// <param name="pCallback"></param>
	private void OnLobbyMatchListReceived(LobbyMatchList_t pCallback)
	{
		Debug.Log("received");
		
		uint lobbiesMatching = pCallback.m_nLobbiesMatching;
		lobbyMatchList = new CSteamID[lobbiesMatching];

		for (int i = 0; i < lobbiesMatching; i++)
		{
			/*
			 *	
			 *	because OnLobbyMatchListReceived was invoked by the Steamworks callback, 
			 *	it's safe to call SteamMatchmaking.GetLobbyByIndex as it is now populated
			 *	
			 */
			lobbyMatchList[i] = SteamMatchmaking.GetLobbyByIndex(i); 
		}

		lobbyList.UpdateUI(lobbyMatchList);
	}
}
