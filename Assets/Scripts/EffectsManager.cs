using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.MOP2;

public class EffectsManager : MonoBehaviour
{
	#region Singleton Pattern

	private static EffectsManager instance;
    public static EffectsManager Instance
	{
        get
		{
            if (instance == null)
			{
                instance = GameObject.FindGameObjectWithTag("EffectsManager").GetComponent<EffectsManager>();
			}
            return instance;
		}
	}

	#endregion

	[SerializeField] private ObjectPool paintballPool;
    
    private void Start()
    {
        paintballPool.Initialize();
    }

    public void SpawnPaintball(Bullet bullet)
	{
		Paintball paintball = paintballPool.GetObject().GetComponent<Paintball>();
		bullet.MovedHandler += paintball.OnBulletMoved;
		bullet.DestroyedHandler += paintball.OnBulletDestroyed;

		paintball.AssociatedBulletDestroyedHandler += OnPaintballAssociatedBulletDestroyed;
	}

	public void OnPaintballAssociatedBulletDestroyed(Paintball paintball)
	{
		paintballPool.Release(paintball.gameObject);
	}
}
