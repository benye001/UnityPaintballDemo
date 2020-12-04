using System;
using UnityEngine;
/// <summary>
/// Defines a Bullet that exists within the scene, including non-network-safe data & references like EventHandlers & GameObjects
/// </summary>
[Serializable]
public class Bullet
{
    /// <summary>
    /// Invoked when the bullet is destroyed
    /// Some kind of ParticleManager or EffectsManager is going to subscribe to this later in order to spawn paint splatter effects
    /// </summary>
    public event BulletDestroyed DestroyedHandler;

    /// <summary>
    /// Invoked when the bullet moves
    /// </summary>
    public event BulletMoved MovedHandler;

    private Vector3 firstIterationPosition;
    private Vector3 previousIterationPosition;

    public BulletNetworkSafeData NetworkSafeData;

    private CurvedLerp gravityForceCurvedLerp;
    private float gravityForce = 1.5f;
    private float yVelocityTerminal = 100f;

    private Vector3 velocity;
    public Vector3 Velocity => velocity;

    private bool awaitingServerDestruction;
    /// <summary>
    /// Lets the client know it doesn't have to iterate or display this bullet 
    /// without actually removing it from the liveBullets list 
    /// until the server tells the client to do so
    /// </summary>
    public bool AwaitingServerDestruction => awaitingServerDestruction;

    /// <summary>
    /// Determines how far the bullet has to travel before it becomes visible to the player
    /// </summary>
    private float distanceFromOriginUntilVisible;
    private bool visible;

    public Bullet(BulletNetworkSafeData networkSafeData, CurvedLerp bulletDropCurvedLerp)
	{   
        NetworkSafeData = new BulletNetworkSafeData(networkSafeData);

        this.gravityForceCurvedLerp = new CurvedLerp(bulletDropCurvedLerp);

        velocity = NetworkSafeData.ShootDirection * NetworkSafeData.IterationDistance;

        firstIterationPosition = NetworkSafeData.Position;
        DestroyedHandler = null;
        MovedHandler = null;
        awaitingServerDestruction = false;

        //Bullet class is for function, EffectsManager::SpawnPaintball is only for visuals
        EffectsManager.Instance.SpawnPaintball(this);

        //If the bullet is always visible from the same position, it looks too much like an exact pattern, this doesn't functionally change anything but breaks up the visuals a bit
        distanceFromOriginUntilVisible = UnityEngine.Random.Range(-10f, 5f);

        //EffectsManager::SpawnPaintball subscribed the visual paintball to this Bullet class' event handlers, so now we can invoke them and do whatever with them over in the Paintball class
        MovedHandler?.Invoke(NetworkSafeData.Position, visible);
    }

    /// <summary>
    /// Flags AwaitingServerDestruction to true,
    /// and invokes DestroyedHandler if not done already
    /// </summary>
    /// <param name="destructionPosition"></param>
    public void DestroySelf(Vector3 destructionPosition)
	{
        if (!AwaitingServerDestruction) //in the case that this Bullet was already marked for destruction, the DestroyedHandler has already been invoked, there's no reason to invoke it twice
        {
            DestroyedHandler?.Invoke(destructionPosition); //for visual effects later on, or something
        }

        awaitingServerDestruction = true; //in the case of a client calling this locally, it will not truly be destroyed but simply skipped over in the iteration loop until the server decides to destroy it
	}

    /// <summary>
    /// Iterate the Position of the bullet forwards based on current Position, Direction & IterationDistance
    /// Also depletes NetworkSafeData.Lifetime until it reaches 0f, at which point AwaitingServerDestruction is set to true
    /// </summary>
    public void Iterate()
	{

        if (!visible)
		{
            if (Vector3.Distance(firstIterationPosition, NetworkSafeData.Position) > distanceFromOriginUntilVisible)
			{
                visible = true;
			}
        }

        

        float multiplier = gravityForceCurvedLerp.LerpAlongCurve();
        float gravityAdditionThisIteration = gravityForce * multiplier;

        velocity.y = Mathf.Clamp(
            velocity.y - gravityAdditionThisIteration, //attempted calculation
            /*  
             *  if the current downwards Y velocity already exceeds terminal velocity, then essentially don't do anything to the velocity. 
             *  else, clamp it to -yVelocityTerminal and let the gravity calculation happen 
             * 
             */
            (velocity.y < -yVelocityTerminal) ? velocity.y : -yVelocityTerminal,   
            /*
             * 
             * Mathf.Infinity as a positive clamp to prevent any conflicts in inspector parameters (bulletSpeed) Vs actual results
             * 
             */
            Mathf.Infinity);

        NetworkSafeData.Position = NetworkSafeData.Position + (velocity * Time.deltaTime);

        //  inform subscribers that the bullet has moved
        MovedHandler?.Invoke(NetworkSafeData.Position, visible);

        Debug.DrawLine(previousIterationPosition, NetworkSafeData.Position, Color.red);



        NetworkSafeData.Lifetime -= Time.deltaTime;

        if (NetworkSafeData.Lifetime <= 0f)
		{
            awaitingServerDestruction = true;
		}

        previousIterationPosition = NetworkSafeData.Position;
    }
}
