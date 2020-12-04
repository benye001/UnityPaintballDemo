using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IControlledByMainController
{

	public event DeathChanged DeathChangedHandler;

	#region References

	private GeneralUIHandler generalUIHandler;

	#endregion

	#region Internal Values

	[SyncVar(hook = nameof(OnHealthChanged))] private float health = 100f; // in effect the SyncVar is an automatic RPC call whenever the value is changed. thus it can only be changed server side

	[SyncVar(hook = nameof(OnDeathChanged))]private bool death = true;
	public bool Death => death;

	#endregion

	public void Initialize()
	{
		
		generalUIHandler = GameObject.FindGameObjectWithTag("GeneralUIHandler").GetComponent<GeneralUIHandler>();

		if (isLocalPlayer)
		{
			LocalPlayerInitialize();
		}
	}

	private void LocalPlayerInitialize()
	{
		// ...nothing yet
	}

	public void Tick()
	{
		if (isLocalPlayer)
		{
			LocalPlayerTick();
		}
	}

	private void LocalPlayerTick()
	{
		// ...nothing yet
	}
	
	[Server]
	public void Server_TakeDamage(float damage)
	{
		health -= damage;
	}

	private void OnHealthChanged(float oldHealthValue, float newHealthValue) // just like an RPC, it's executed on the client-server as well as all clients
	{
		health = newHealthValue; // when you set a hook in your SyncVar, the value is no longer changed automatically

		if (isLocalPlayer) 
		{
			generalUIHandler?.UpdateHealth(health);
		}	

		if (isServer && health <= 0f)
		{
			death = true;
		}

		//Debug.LogError("OnHealthChanged: " + health); //LogError shows in the dev console on development builds

		// ... animations, UI changes, visual effects
	}

	private void OnDeathChanged(bool oldValue, bool newValue)
	{
		death = newValue;
		DeathChangedHandler?.Invoke(newValue);
	}

	public void ResetToDefaults()
	{
		health = 100f;
		death = false;
	}
}
