using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public struct FireEventArgs
{
    public float damage;
    public float speed;
    public float spread;
    public CurvedLerp dropCurve;
    public float lifetime;
    
    public FireEventArgs(float damage, float speed, float spread, CurvedLerp dropCurve, float lifetime)
	{
        this.damage = damage;
        this.speed = speed;
        this.spread = spread;
        this.dropCurve = dropCurve;
        this.lifetime = lifetime;
	}
}*/

public delegate void FirearmSelected(
    RecoilProfile recoilProfile, 
    GameObject firearmPrefab, 
    CharacterAnimationType characterAnimationType, 
    RuntimeAnimatorController characterAnimatorController, 
    RuntimeAnimatorController firstPersonRuntimeAnimatorController);
public delegate void AddRecoil(float recoil);

public class PlayerWeaponHandler : NetworkBehaviour, IControlledByMainController
{
    public event FirearmSelected FirearmSelectedHandler;
    public event AddRecoil AddRecoilHandler;

    /// <summary>
    /// the amount of recoil that will be applied horizontally, based on where the player is aiming vertically (vertical aim being x axis camera rotation) 
    /// t= 1   on the curve is aiming 90 degree aim on the x axis (downward) 
    /// t= 0   on the curve is 0 degree aim the x axis (forward)
    /// t= -1  on the curve is -90 degree aim on the x axis (upward)
    /// </summary>
    

    /// <summary>
    /// the amount of recoil that will be applied vertically, based on where the player is aiming vertically (vertical aim being x axis camera rotation) 
    /// t= 1   on the curve is aiming 90 degree aim on the x axis (downward) 
    /// t= 0   on the curve is 0 degree aim the x axis (forward)
    /// t= -1  on the curve is -90 degree aim on the x axis (upward)
    /// </summary>
    


    /*
     * 
     *  q: 
     *      what is the purpose of having AnimationCurves for recoil based on vertical aim?
     * 
     *  a:
     *      in studying AAA shooters I realized that the firearm recoil will slowly transition into a horizontal recoil instead of a vertical recoil, 
     *      depending on where the player is aiming vertically (using Star Wars Battlefront 2015 as gunplay reference, since their guns feel amazing to shoot imo)
     *      
     *      this does a few things
     *      
     *      1.  with this system, spamming bullets excessively not only increases bullet spread but makes your aim more erratic. 
     *          vertical recoil is easy to account for, it's always upward aim offset, but horizontal recoil, which will switch between left & right aim offsets, is harder to account for
     *          
     *      2.  with this system, shooting a lot of bullets will not continually push your gun up into the sky until it reaches -90 degrees upward
     *          the vertical addition to aim will have diminishing returns as it transitions into a horizontal addition to aim
     *  
     *      3.  if a player shoots straight up, or starts shooting from a high upward angle, they won't "snap their neck" and look beyond -90 degrees upward
     *  
     */

	#region References

	private PlayerMainController mainController;

	#endregion

	#region Inspector Parameters 

	/*
     *  eventually these bullet parameters will be handled by a dedicated & interchangable WeaponHandler component
     *  
     *  planned implementation is to have a base weapon prefab with some basic GameObject structure
     *  then have prefab variants for the actual weapons
     *  
     *  on each prefab variant would be an XXXWeaponHandler, which would be a class which derives from an abstract class WeaponHandler, WeaponHandler would derive from MonoBehaviour
     *  
     */
	#region Bullet Parameters
	/*[SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletSpeed = 1f;
    [SerializeField] private float bulletLifetime = 4f;
    [SerializeField] private CurvedLerp bulletDropCurve;*/
	#endregion

	[SerializeField] private LayerMask raycastExclusion;

    #endregion

    [SerializeField] private List<Bullet> liveBullets = new List<Bullet>();

    [SyncVar(hook = nameof(OnSelectedFirearmUniqueIdentifierChanged))] private FirearmUniqueIdentifier selectedFirearmUniqueIdentifier = FirearmUniqueIdentifier.Default;

    private AbstractFirearm firearm; 

	public void Initialize()
    {
        mainController = GetComponent<PlayerMainController>();

        raycastExclusion = ~raycastExclusion; // inverse the LayerMask

        //temporary
        NetworkSafe_SelectFirearm(new RevolverFirearm().FirearmUniqueIdentifier);
    }

    private void OnSelectedFirearmUniqueIdentifierChanged(FirearmUniqueIdentifier oldValue, FirearmUniqueIdentifier newValue)
    {
        StartCoroutine(WaitForEventSubscribers(newValue));
    }

    /// <summary>
    /// used when changing characters, so the weapon prefab can persist
    /// </summary>
    public void ReselectCurrentFirearm()
	{
        StartCoroutine(WaitForEventSubscribers(selectedFirearmUniqueIdentifier));
	}

    private IEnumerator WaitForEventSubscribers(FirearmUniqueIdentifier newValue)
	{
        while (FirearmSelectedHandler == null)
		{
            Debug.Log("is null");
            yield return null;
		}

        //if (newValue != selectedFirearmUniqueIdentifier)
		//{
            selectedFirearmUniqueIdentifier = newValue;
        //}

        Debug.Log("select firearm changed");

        if (firearm != null)
		{
            firearm.FireNetworkSafeHandler -= OnFireNetworkSafe;
		}
        
        firearm = Firearms.GetAbstractFirearmByUniqueIdentifier(selectedFirearmUniqueIdentifier);
        firearm.FireNetworkSafeHandler += OnFireNetworkSafe;

        if (FirearmSelectedHandler == null)
        {
            
        }

        FirearmSelectedHandler?.Invoke(
            firearm.GetRecoilProfile, 
            firearm.GetPrefab, 
            firearm.CharacterAnimationType, 
            firearm.GetCharacterRuntimeAnimatorController, 
            firearm.GetFirstPersonRuntimeAnimatorController);
    }

    public void NetworkSafe_SelectFirearm(FirearmUniqueIdentifier firearmUniqueIdentifier)
	{
        if (isServer)
		{
            selectedFirearmUniqueIdentifier = firearmUniqueIdentifier;
		}
        else
		{
            Cmd_SelectFirearm(firearmUniqueIdentifier);
		}
	}

    [Command]
    private void Cmd_SelectFirearm(FirearmUniqueIdentifier firearmUniqueIdentifier)
	{
        selectedFirearmUniqueIdentifier = firearmUniqueIdentifier;
	}

    public void Local_SelectFirearm(AbstractFirearm newFirearm)
	{
        
    }

    /*public void SelectFirearmNetworkSafe(AbstractFirearm newFirearm)
	{
        
	}*/


    public void Tick()
    {
        if (isServer)
		{
            Server_Tick();
		}
        if (isClientOnly)
		{
            Client_Tick();
		}

        if (isLocalPlayer)
        {
            Local_Tick();
            firearm?.Tick();
        }
    }

    private void Local_Tick()
	{
        if (Input.GetMouseButton(0))
		{
            firearm.FireButton();
		}
        if (Input.GetMouseButtonDown(0))
        {
            firearm.FireButtonDown();
        }
        if (Input.GetMouseButtonUp(0))
        {
            firearm.FireButtonUp();
        }
    }

    [Client]
    private void Client_Tick()
	{
        Local_IterateLiveBullets(SimulationType.Client);
	}

    [Server]
    private void Server_Tick()
	{
        Local_IterateLiveBullets(SimulationType.Server);
	}

    [SerializeField] private Vector3 playerRecoil = Vector3.zero;
    [SerializeField] private Vector3 cameraRecoil = Vector3.zero;

    private void Local_IterateLiveBullets(SimulationType simulationType)
	{
        /*
         * 
         * the indexes of bullets which have reached end of life, 
         * so they can be permenantly destroyed & removed from liveBullets after the for loop completes
         * 
         */
        List<int> liveBulletIndexesAtEndOfLife = new List<int>();

		#region Iteration Loop

		for (int i = 0; i < liveBullets.Count; i++)
		{
            if (liveBullets[i].AwaitingServerDestruction) //if the bullet has been psuedo-destroyed client side, then we don't need to iterate it, skip over
			{
                if (isServer)
				{
                    /*
                     *  save the indexes of the liveBullets which have reached end of life, 
                     *  without removing them from liveBullets just yet. 
                     *  they'll be removed after the for loop completes, to prevent index mismatching & errors
                     *  
                     *  only the server has the ability to destroy liveBullets completely, so we don't bother with this step if we're on a client
                     *  
                     */
                    liveBulletIndexesAtEndOfLife.Add(i);
				}
                i++; //whether the server or the client, skip over
			}
            if (i >= liveBullets.Count) //check again that the index is still valid
            {
                break;
            }

            RaycastHit hit;
            Ray ray = new Ray(liveBullets[i].NetworkSafeData.Position, liveBullets[i].Velocity.normalized);

            bool currentLiveBulletStillExists = true;

            if (Physics.Raycast(ray, out hit, liveBullets[i].NetworkSafeData.IterationDistance * Time.deltaTime, raycastExclusion))
            {
                GameObject hitGameObject = hit.collider.gameObject;
                /*
                 * 
                 *  if the simulationType is ServerAuthoritative, take real action and update the clients with RPCs
                 *  
                 *  if the simulationType is ClientPrediction, disable the bullet for the time being and wait for the server to destroy the bullet via RPC
                 *  
                 *  q: 
                 *      why not use a SyncList since Bullet is a struct?
                 *      
                 *  a:
                 *      SyncLists were OK for the player connection list because it's just a couple of structs.
                 *      with the possibility of 500+ Bullet structs at one time, each containing multiple Vector3's, floats, etc, that would be major network traffic
                 *      
                 *      we're also not using SyncList because it only updates every 1/10th of a second. we want to send minimal amount of data the moment it needs to be sent
                 *      
                 *      it's also easier this way to shoot fake simulation bullets on the client for visual effect & create immediate perceived player input response
                 *  
                 */
                switch (simulationType)
                {
                    case SimulationType.Server:

                        if (hitGameObject.CompareTag("Player"))
                        {
                            PlayerInfoHandler hitPlayerInfohandler = hitGameObject.GetComponent<PlayerInfoHandler>();

                            if (hitPlayerInfohandler.PlayerInfo.Team != mainController.InfoHandler.PlayerInfo.Team)
                            {
                                hitGameObject.GetComponent<PlayerHealth>().Server_TakeDamage(liveBullets[i].NetworkSafeData.Damage);
                            }
                        }
                            
                        Rpc_DestroyLiveBulletByIndex(i, hit.point);
                        currentLiveBulletStillExists = false;

                        break;

                    case SimulationType.Client:
                        liveBullets[i].DestroySelf(hit.point);
                        currentLiveBulletStillExists = false;

                        break;
                }
            }

            if (currentLiveBulletStillExists)
			{
                //Debug.Log();
                liveBullets[i].Iterate();
            }
        }

		#endregion

		if (isServer)
        {
            for (int i = 0; i < liveBulletIndexesAtEndOfLife.Count; i++)
            {
                Rpc_DestroyLiveBulletByIndex(i, liveBullets[liveBulletIndexesAtEndOfLife[i]].NetworkSafeData.Position);
            }
        }
    }

    [ClientRpc]
    private void Rpc_NonLocal_AddLiveBullet(BulletNetworkSafeData networkSafeData)
	{
        /*
         *  if this instance of the player owns this player, aka isLocalPlayer, then don't return and don't call Local_AddLiveBullet. 
         *  Local_AddLiveBullet is already called on the local instance via ShootBulletNetworkSafe
         */
        if (isLocalPlayer)
		{
            return;
		}

        Local_AddLiveBullet(networkSafeData);
    }

    private void Local_AddLiveBullet(BulletNetworkSafeData networkSafeData)
	{
        liveBullets.Add(new Bullet(networkSafeData, firearm.GetBulletProfile.dropCurve));
        AddRecoilHandler?.Invoke(firearm.GetRecoil);
    }

    [ClientRpc]
    private void Rpc_DestroyLiveBulletByIndex(int index, Vector3 destructionPosition)
	{
        if (index < liveBullets.Count)
        {
            Bullet liveBullet = liveBullets[index];
            if (liveBullet != null)
            {
                liveBullet.DestroySelf(destructionPosition);
                liveBullets.RemoveAt(index);
            }
        }
        else
		{
            Debug.LogError("Impossible index");
		}
    }

    public void ResetToDefaults()
	{
        //respawn logic 
	}

    /// <summary>
    /// Determines if this instance is a client or host and calls the correct methods
    /// </summary>
    private void OnFireNetworkSafe()
	{
        if (isLocalPlayer)
        {

            //Debug.Log("OnFireNetworkSafe");
            BulletNetworkSafeData bulletNetworkSafeData = new BulletNetworkSafeData(
                mainController.Look.MainCamera.position,
                mainController.Look.MainCamera.forward,
                firearm.GetBulletProfile.speed,
                firearm.GetDamage,
                firearm.GetBulletProfile.lifetime,
                mainController.InfoHandler.PlayerInfo.Id);

            /*
             *  note: normally you would not need Rpc_AddLiveBullet in addition to Local_AddLiveBullet, or Cmd_AddLiveBullet in addition to Local_AddLiveBullet. 
             *  
             *  Rpc is in fact called on the server-client as well as clients, & Cmd is just a method which can be called from a client which triggers Rpc to all other clients (including the one who originally sent Cmd)
             *  
             *  in this case, we want bullets to be simulated client-side with zero network delay, (even though the client side bullets don't functionally do anything) because delay when shooting feels awful
             * 
             */

            if (isServer)
            {
                Rpc_NonLocal_AddLiveBullet(bulletNetworkSafeData);
                Local_AddLiveBullet(bulletNetworkSafeData);
            }
            else
            {
                Cmd_NonLocal_AddLiveBullet(bulletNetworkSafeData);
                Local_AddLiveBullet(bulletNetworkSafeData);
            }
        }
	}

    [Command]
    private void Cmd_NonLocal_AddLiveBullet(BulletNetworkSafeData bulletNetworkSafeData) 
        /*
         * Cmd can only be called from non-server clients which have clientAuthority set to true on their NetworkIdentity component. when called, it is only executed on the server-client
	     *
	     * Cmd_ prefix is actually nessecary to function
	     *
         */
    {
        /*
         * 
         * q: why is fromPosition being passed from the client and not decided server side?
         * a: trying to reduce bullet & aim lag. if the server decided what position the shot would be shot from, it would be some # of miliseconds behind where the client actually shot the shot, causing a completely different aim trajectory
         * 
         * q: won't other players from the clients perspective still be behind where they actually are on the server?
         * a: yes, clientside player movement prediction will have to be implemented eventually in place of the NetworkTransform component
         * 
         */
        Rpc_NonLocal_AddLiveBullet(bulletNetworkSafeData);
    }
}
