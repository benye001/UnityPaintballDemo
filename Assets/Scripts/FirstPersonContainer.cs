using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonContainer : MonoBehaviour
{
    public void DestroyChildren()
	{
		foreach (CharacterUniqueReferenceHandler current in GetComponentsInChildren<CharacterUniqueReferenceHandler>())
		{
			Destroy(current.gameObject);
		}
	}


}
