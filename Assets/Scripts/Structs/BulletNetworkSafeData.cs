using System;
using UnityEngine;
/// <summary>
/// The data which needs to be sent over the network in order for a bullet to be fired, 
/// which can only be basic data types and a few basic unity data types like Vector3. 
/// See Mirror/UNET documentation for details
/// </summary>
[Serializable]
public struct BulletNetworkSafeData
{
    public Vector3 Position;
    public Vector3 ShootDirection;

    /// <summary>
    /// Distance this bullet will travel every tick
    /// </summary>
    public float IterationDistance;

    public float Damage;

    public float Lifetime;

    /// <summary>
    /// PlayerId of the player who shot the bullet
    /// </summary>
    public int ShooterPlayerId;

    /// <summary>
    /// Create new BulletNetworkSafeData
    /// </summary>
    /// <param name="position">Starting bullet position</param>
    /// <param name="direction">Starting bullet direction</param>
    /// <param name="iterationDistance">Distance moved per iteration</param>
    /// <param name="damage">Damage done to players upon contact</param>
    /// <param name="shooterPlayerId">Originator of the bullet</param>
    public BulletNetworkSafeData(Vector3 position, Vector3 direction, float iterationDistance, float damage, float lifetime, int shooterPlayerId)
	{
        Position = position;
        ShootDirection = direction;

        IterationDistance = iterationDistance;

        Damage = damage;
        Lifetime = lifetime;

        ShooterPlayerId = shooterPlayerId;
    }

    /// <summary>
    /// Copy another BulletNetworkSafeData to new BulletNetworkSafeData
    /// </summary>
    /// <param name="bulletNetworkSafeData"></param>
    public BulletNetworkSafeData(BulletNetworkSafeData bulletNetworkSafeData)
	{
        Position = bulletNetworkSafeData.Position;
        ShootDirection = bulletNetworkSafeData.ShootDirection;

        IterationDistance = bulletNetworkSafeData.IterationDistance;

        Damage = bulletNetworkSafeData.Damage;
        Lifetime = bulletNetworkSafeData.Lifetime;

        ShooterPlayerId = bulletNetworkSafeData.ShooterPlayerId;
	}
}
