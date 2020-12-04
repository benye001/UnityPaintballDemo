using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class PlayerInfoHandlerList
{
	[SerializeField] public List<PlayerInfoHandler> List;
	
	public PlayerInfoHandlerList()
	{
		List = new List<PlayerInfoHandler>();
	}
	
	public void Add(PlayerInfoHandler playerInfoHandler, int id)
	{
		playerInfoHandler.PlayerInfo = new PlayerInfo
		{
			Id = id,
			Name = "Player" + id,

			Score = 0
		};

		List.Add(playerInfoHandler);
	}

	public void RemoveByPlayerId(int id)
	{
		for (int i = 0; i < List.Count; i++)
		{
			if (List[i].PlayerInfo.Id == id)
			{
				List.RemoveAt(i);
			}
		}
	}

	public PlayerInfoHandler GetByPlayerId(int id)
	{
		PlayerInfoHandler chosen = null;
		
		for (int i = 0; i < List.Count; i++)
		{
			PlayerInfoHandler current = List[i];

			if (current.PlayerInfo.Id == id)
			{
				chosen = current;
			}
		}

		return chosen;
	}

	public PlayerInfoHandler GetByIndex(int index)
	{
		return List[index];
	} 
}
