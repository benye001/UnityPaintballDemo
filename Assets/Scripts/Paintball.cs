using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paintball : MonoBehaviour
{
	public event PaintballAssociatedBulletDestroyed AssociatedBulletDestroyedHandler;

	[SerializeField] private GameObject visuals;

	public void OnBulletMoved(Vector3 position, bool visible)
	{
		transform.position = position;
		if (visuals.activeSelf != visible)
		{
			visuals.SetActive(visible);
		}
	}

	public void OnBulletDestroyed(Vector3 destroyPosition)
	{
		AssociatedBulletDestroyedHandler?.Invoke(this);
	}
}