using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DeathChanged(bool newValue);

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(PlayerWeaponHandler))]
[RequireComponent(typeof(PlayerInfoHandler))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerAnimationHandler))]
public class PlayerMainController : NetworkBehaviour
{

	#region References

	private NetworkIdentity networkIdentity;
    public NetworkIdentity NetworkIdentity => networkIdentity;


    private PlayerMove move;
    public PlayerMove Move => move;


    private PlayerLook look;
    public PlayerLook Look => look;


    private PlayerWeaponHandler weaponHandler;
    public PlayerWeaponHandler WeaponHandler => weaponHandler;


    private PlayerHealth health;
    public PlayerHealth Health => health;


    private PlayerInfoHandler infoHandler;
    public PlayerInfoHandler InfoHandler => infoHandler;


    private PlayerAnimationHandler animationHandler;
    public PlayerAnimationHandler AnimationHandler => animationHandler;


    private IControlledByMainController[] componentsWithinControl;


    [SerializeField] private GameObject visuals;

    [SerializeField] private CharacterUniqueReferenceHandler characterUniqueReferenceHandler;
    public CharacterUniqueReferenceHandler CharacterUniqueReferenceHandler => characterUniqueReferenceHandler;

    #endregion

    private IEnumerator waitToSetFirearmPrefabContainer = null;

	private void Start()
    {
        FindReferences();

        // ... initialization

        /*
         * 
         *  PlayerMainController needs to be notified whenever the PlayerHealth::death value is changed,
         *  but for PlayerHealth to call a method on PlayerMainController would break top -> down design and create spaghetti code
         *  so instead we subscribe PlayerMainController to an event, this way the PlayerHealth component can still exist within a vacuum
         * 
         */
        health.DeathChangedHandler += OnDeathChanged;
        weaponHandler.AddRecoilHandler += OnAddRecoil;
        weaponHandler.FirearmSelectedHandler += OnFirearmSelected;
        move.MoveInputChangedHandler += OnMoveInputChanged;
        animationHandler.CharacterSelectedHandler += OnCharacterSelected;

        InitializePlayerComponents();
    }

    private void OnCharacterSelected(CharacterUniqueReferenceHandler newHandler)
	{
        characterUniqueReferenceHandler = newHandler;
        weaponHandler.ReselectCurrentFirearm();
    }

    private void OnMoveInputChanged(Vector2 moveInput)
	{
        animationHandler.SetMoveInput(moveInput);
	}

    /// <summary>
    /// Event method invoked by PlayerHealth::DeathChangedHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeathChanged(bool newValue)
    {

        if (newValue == true)
        {
            if (isLocalPlayer)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (isServer)
            {
                GameManager.Instance.Server_PlayerDied(infoHandler.PlayerInfo.Team);
            }
            SetVisualsAndColliders(false, false);
        }
        else
        {
            if (isLocalPlayer)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            SetVisualsAndColliders(!isLocalPlayer, true);
        }
    }

    private void OnAddRecoil(float recoil)
	{
        look.AddRecoil(recoil);
        move.InterruptSprinting();
	}

    private void OnFirearmSelected(
        RecoilProfile recoilProfile, 
        GameObject firearmPrefab, 
        CharacterAnimationType characterAnimationType, 
        RuntimeAnimatorController characterRuntimeAnimatorController,
        RuntimeAnimatorController firstPersonRuntimeAnimatorController)
	{
        look.SetRecoilProfile(recoilProfile);
        Debug.Log("OnFirearmSelected " + recoilProfile.name);

        if (waitToSetFirearmPrefabContainer != null)
		{
            StopCoroutine(waitToSetFirearmPrefabContainer);
        }
        waitToSetFirearmPrefabContainer = WaitToSetFirearmPrefab(firearmPrefab, characterAnimationType, characterRuntimeAnimatorController, firstPersonRuntimeAnimatorController);
        StartCoroutine(waitToSetFirearmPrefabContainer);
	}

    private void Update()
    {
        if (!health.Death) 
        {
            Tick();
            TickPlayerComponents();
        }
    }

    private void FindReferences()
    {
        move = GetComponent<PlayerMove>();
        look = GetComponent<PlayerLook>();
        weaponHandler = GetComponent<PlayerWeaponHandler>();
        health = GetComponent<PlayerHealth>();
        infoHandler = GetComponent<PlayerInfoHandler>();
        animationHandler = GetComponent<PlayerAnimationHandler>();

        networkIdentity = GetComponent<NetworkIdentity>();

        componentsWithinControl = GetComponents<IControlledByMainController>();
    }

    private void Tick()
	{
        if (isLocalPlayer)
		{
            LocalPlayerTick();
		}
	}

    private void LocalPlayerTick()
	{
        float cameraForwardY = look.MainCamera.forward.y;

        //animationHandler.SetAimAtPosition(new Vector3(0f, 1.25f * cameraForwardY, 1.5f * (1f - Mathf.Abs(cameraForwardY))) + new Vector3(0f, 1.5f, 0f)); circular IK motion

        animationHandler.SetAimAtPosition(new Vector3(0f, cameraForwardY, 0f)); //linear motion 
    }
  

    private void InitializePlayerComponents()
	{
        for (int i = 0; i < componentsWithinControl.Length; i++)
		{
            componentsWithinControl[i].Initialize();
		}

    }

    private void TickPlayerComponents()
	{
        for (int i = 0; i < componentsWithinControl.Length; i++)
		{
            componentsWithinControl[i].Tick();
		}
	}

    private void ResetPlayerComponents()
	{
        for (int i = 0; i < componentsWithinControl.Length; i++)
        {
            componentsWithinControl[i].ResetToDefaults();
        }
    }

    /// <summary>
    /// Waits for the visual Character prefab (e.g. the AbedCharacter prefab) to be set before instantiating the firearm prefab
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitToSetFirearmPrefab(
        GameObject firearmPrefab, 
        CharacterAnimationType characterAnimationType, 
        RuntimeAnimatorController characterRuntimeAnimatorController, 
        RuntimeAnimatorController firstPersonRuntimeAnimatorController)
	{
        while (characterUniqueReferenceHandler == null)
		{
            Debug.Log(infoHandler.PlayerInfo.Id + "waiting");
            yield return null;
		}

        characterUniqueReferenceHandler.DestroyFirearmContainerChildren();

        Transform characterFirearm = Instantiate(firearmPrefab, characterUniqueReferenceHandler.GetFirearmContainerByCharacterAnimationType(characterAnimationType)).transform;
        /*
         * 
         *  the firearm prefab container, which reference to is defined by CharacterUniqueReferenceHandler, decides the position offset and rotation offset of the firearm prefab
         *  because of this, we can set the parent to the firearm prefab container & just set the local position & rotation of the actual firearm prefab to zero
         *  
         */
        characterFirearm.localPosition = Vector3.zero;
        characterFirearm.localRotation = Quaternion.identity;

        characterUniqueReferenceHandler.SetRuntimeAnimatorController(characterRuntimeAnimatorController);

        if (isLocalPlayer)
		{

            // even though Destroy is called on the original first person prefab before GetComponentsInChildren is ever called, 
            // it still sometimes will return two references (one of which is null)
            // the in-line if statement accounts for the Unity bug
            CharacterUniqueReferenceHandler[] firstPersonCharacterUniqueReferenceHandlers = look.MainCamera.GetComponentsInChildren<CharacterUniqueReferenceHandler>();
            CharacterUniqueReferenceHandler firstPersonCharacterUniqueReferenceHandler = (firstPersonCharacterUniqueReferenceHandlers.Length == 1) ? firstPersonCharacterUniqueReferenceHandlers[0] : firstPersonCharacterUniqueReferenceHandlers[1]; 

            firstPersonCharacterUniqueReferenceHandler.DestroyFirearmContainerChildren();

            Transform firearmContainer = firstPersonCharacterUniqueReferenceHandler.GetFirearmContainerByCharacterAnimationType(characterAnimationType);

            Transform firstPersonFirearm = Instantiate(firearmPrefab, firearmContainer).transform;
            firstPersonFirearm.localPosition = Vector3.zero;
            firstPersonFirearm.localRotation = Quaternion.identity;

            LayerTools.SetLayerRecursively(firearmContainer.gameObject, 9); //9 == FirstPerson layer

            firstPersonCharacterUniqueReferenceHandler.SetRuntimeAnimatorController(firstPersonRuntimeAnimatorController);
            if (animationHandler.SelectedAbstractCharacter != null)
            {
                firstPersonCharacterUniqueReferenceHandler.Animator.SetTrigger(animationHandler.SelectedAbstractCharacter.Name.ToString() + "PoseAdjustment");
            }
		}

        waitToSetFirearmPrefabContainer = null;
	}

    [Server]
    public void Server_ResetToDefaults()
	{
        Rpc_ResetPosition();
        ResetPlayerComponents();
	}

    [ClientRpc]
    private void Rpc_ResetPosition()
	{
        transform.position = Vector3.zero;
	}

    /// <summary>
    /// Determines if this instance is a server-client or just a client, then calls the appropriate respawn method
    /// </summary>
    public void RespawnPlayerNetworkSafe()
	{
        if (animationHandler.SelectedCharacterName == CharacterName.Default)
		{
            //Debug.LogError("");
        }
        else
		{
            int localPlayerId = infoHandler.PlayerInfo.Id;

            if (networkIdentity.isServer)
            {
                GameManager.Instance.Server_RespawnPlayerById(localPlayerId);
            }
            else
            {
                Cmd_RespawnPlayer(localPlayerId);
            }
        }
        
	}

    [Command]
    private void Cmd_RespawnPlayer(int playerId)
        /*
         * 
         *  q:
         *      why is there a Cmd on the player, of which the only purpose is to call a server-only method on the GameManager? why not a Cmd on the GameManager?
         *      
         *  a:
         *      players only have authority over the NetworkBehaviours on their own player, 
         *      due to the NetworkIdentity component which has clientAuthority set to true. 
         *      
         *      Cmds invoked within the GameManager (which was spawned by the server, with authority given to the server-client) would not execute 
         * 
         */
	{
        GameManager.Instance.Server_RespawnPlayerById(playerId);
    }

    /// <summary>
    /// Calls SetLocalPlayer(gameObject) on the LOCAL instance of this player's GameManager
    /// </summary>
    /// <param name="target"></param>
    [TargetRpc] //sends an RPC to a specific NetworkConnection / player
    public void TargetRpc_SetLocalGameManagerLocalPlayerReferenceToSelf(NetworkConnection target)
	{
        GameManager.Instance.SetLocalPlayer(gameObject);
	}

    /// <summary>
    /// Enables or disables the collider component & the visuals GameObject, depending on the given parameter
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisualsAndColliders(bool visible, bool collision)
    {
        GetComponent<CharacterController>().enabled = collision;
        visuals.SetActive(visible);
    }
}
