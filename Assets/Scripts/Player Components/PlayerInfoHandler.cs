using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoHandler : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerInfoChanged))] public PlayerInfo PlayerInfo;

    private void OnPlayerInfoChanged(PlayerInfo oldValue, PlayerInfo newValue)
	{
		PlayerInfo = newValue;
		GameManager.Instance.UpdatePlayerStatsUIHandler();
	}
}
