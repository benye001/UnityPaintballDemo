using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerSpawner : MonoBehaviour
{
	[SerializeField] private GameObject networkManagerLANPrefab;
	[SerializeField] private GameObject networkManagerSteamworksPrefab;
	
	private void Start()
	{
		GameObject networkManager = GameObject.FindGameObjectWithTag("NetworkManager");
		if (networkManager != null)
		{
			Destroy(networkManager);
		}

		StartCoroutine(WaitThenLoad());
	}

	private IEnumerator WaitThenLoad()
	{
		yield return new WaitForSeconds(0.5f);
		
		if (PlayerPrefs.GetInt(PlayerPrefKeys.LAN_MODE) == 0)
		{
			Instantiate(networkManagerSteamworksPrefab);
		}
		else
		{
			Instantiate(networkManagerLANPrefab);
		}

		SceneManager.LoadScene("MainMenu");
	}
}
