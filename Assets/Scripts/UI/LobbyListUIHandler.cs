using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class LobbyListUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject lobbyListItemPrefab;
    private LobbyListItemUIHandler[] listItemUIHandlers;

    public event ListItemClick ListItemClickHandler;

    public void UpdateUI(CSteamID[] lobbySteamIds)
	{
        int lobbyCount = lobbySteamIds.Length;

		#region Destroy existing LobbyListItemUIHandlers

		if (listItemUIHandlers != null)
        {
            for (int i = 0; i < listItemUIHandlers.Length; i++)
            {
                Destroy(listItemUIHandlers[i].gameObject);
            }
        }

		#endregion

		listItemUIHandlers = new LobbyListItemUIHandler[lobbyCount];
        
        for (int i = 0; i < lobbyCount; i++)
		{
            LobbyListItemUIHandler listItemUIHandler = Instantiate(lobbyListItemPrefab, transform).GetComponent<LobbyListItemUIHandler>();

            listItemUIHandler.Index = i;
            listItemUIHandler.UpdateUI("Lobby Name", "?? / 14"); //temporary implementation, need to figure out how to retrieve lobby name and player count
            listItemUIHandler.ListItemClickHandler += OnListItemClick; //automatically subscribe the master event to the children events

            listItemUIHandlers[i] = listItemUIHandler;
		}
	}

    /// <summary>
    /// Event method which is invoked by the automatically generated children of this GameObject, LobbyListItemUIHandlers
    /// </summary>
    /// <param name="index">index of the LobbyListItemUIHandler which was clicked</param>
    private void OnListItemClick(int index)
	{
        /*
         *  now by invoking a similar delegate to the one which was just invoked with the same parameter, 
         *  MainMenuGeneralUIHandler can simply subscribe to a single event from LobbyListUIHandler
         *  instead of having to subscribe to every single LobbyListItemUIHandler, which MainMenuGeneralUIHandler should not even be referencing at all anyways
         */
        ListItemClickHandler?.Invoke(index);
	}
}
