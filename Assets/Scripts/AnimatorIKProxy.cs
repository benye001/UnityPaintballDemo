using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnimatorIKTrigger(int layerIndex);

public class AnimatorIKProxy : MonoBehaviour
{
    public event AnimatorIKTrigger TriggerHandler;

	private void OnAnimatorIK(int layerIndex)
	{
		TriggerHandler?.Invoke(layerIndex);
	}
}
