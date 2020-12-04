using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthUIHandler : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI healthText;

	public void UpdateUI(float health)
	{
		healthText.text = "Health: " + health;
	}
}
