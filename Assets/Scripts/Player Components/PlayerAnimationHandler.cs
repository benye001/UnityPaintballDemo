using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public delegate void CharacterSelected(CharacterUniqueReferenceHandler newHandler);

public class PlayerAnimationHandler : NetworkBehaviour, IControlledByMainController
{
	public event CharacterSelected CharacterSelectedHandler;
	
	private Animator animator;
	[SerializeField] private Animator firstPersonAnimator;

	[SerializeField] private float moveInputInterpolation = 0.5f;

	[SerializeField] private Vector2 moveInput = Vector2.zero;
	[SerializeField] private Vector2 moveInputInterpolated = Vector2.zero;

	[SerializeField] private Transform aimAtIK;
	[SerializeField] private Transform characterContainer;
	private Transform firstPersonCharacterContainer;

	[SerializeField] private CharacterUniqueReferenceHandler characterUniqueReferenceHandler;
	private CharacterUniqueReferenceHandler firstPersonCharacterUniqueReferenceHandler;

	private bool sprinting;


	[SyncVar(hook = nameof(OnSelectedCharacterNameChanged))] private CharacterName selectedCharacterName;
	public CharacterName SelectedCharacterName => selectedCharacterName;

	public void Initialize()
	{
		animator = characterUniqueReferenceHandler.Animator;

		firstPersonCharacterContainer = GameObject.FindGameObjectWithTag("FirstPersonContainer").transform;
		firstPersonCharacterUniqueReferenceHandler = firstPersonCharacterContainer.gameObject.GetComponentInChildren<CharacterUniqueReferenceHandler>();

		if (isLocalPlayer)
		{
			LocalPlayerInitialize();
		}

		//animator.gameObject.GetComponent<AnimatorIKProxy>().TriggerHandler += OnAnimatorIKTrigger;
	}

	private void LocalPlayerInitialize()
	{
		firstPersonAnimator = GameObject.FindGameObjectWithTag("FirstPersonAnimationController").GetComponent<Animator>();
	}

	public void SetMoveInput(Vector2 newMoveInput)
	{
		moveInput = newMoveInput;
	}

	private void SetFloat(string name, float value)
	{
		animator.SetFloat(name, value);
		if (firstPersonAnimator != null)
		{
			firstPersonAnimator.SetFloat(name, value);
		}
		

		Debug.Log("float set");
	}

	private void SetBool(string name, bool value)
	{
		animator.SetBool(name, value);
		if (firstPersonAnimator != null)
		{
			firstPersonAnimator.SetBool(name, value);
		}
	}

	public void Tick()
	{
		Vector2 currentInput = moveInput;

		SetBool("MoveInputMagnitudeGreaterThanZero", (moveInput.magnitude > 0f)); //transition in or out of the blend tree

		/*
		 *	
		 *		if we're transitioning out of the blend tree, 
		 *		don't change MoveInputX or MoveInputY (which are used to interpolate the blend tree)
		 *		because transitioning out of the blend tree should be handled by the Animator/AnimationController state machine
		 * 
		 */
		if (!Mathf.Approximately(moveInput.magnitude, 0f))
		{

			bool interpolate = true;
			if (Mathf.Approximately(currentInput.magnitude, 0f))
			{
				interpolate = false;
			}

			currentInput = moveInput;



			moveInputInterpolated = (interpolate) ? Vector2.Lerp(moveInputInterpolated, currentInput, moveInputInterpolation) : currentInput;

			SetFloat("MoveInputX", moveInputInterpolated.x);
			SetFloat("MoveInputY", moveInputInterpolated.y);
		}

		animator.SetBool("Aiming", (moveInput.y <= 0.5f));
		animator.SetFloat("AimVertical", aimAtIK.localPosition.y);
	}

	public void ResetToDefaults()
	{

	}

	public void SetAimAtPosition(Vector3 position)
	{
		aimAtIK.localPosition = position;
	}

	public void SetSprinting(bool value)
	{
		sprinting = value;
	}

	public void NetworkSafe_SetSelectedCharacterName(CharacterName characterName)
	{
		if (isServer)
		{
			selectedCharacterName = characterName;
		}
		else
		{
			Cmd_SetSelectedCharacterName(characterName);
		}
	}

	[Command]
	private void Cmd_SetSelectedCharacterName(CharacterName characterName)
	{
		selectedCharacterName = characterName;
	}

	private void OnSelectedCharacterNameChanged(CharacterName oldValue, CharacterName newValue)
	{
		selectedCharacterName = newValue;

		//create the new character
		AbstractCharacter newAbstractCharacter = Characters.GetAbstractCharacterByName(selectedCharacterName);
		Transform newCharacterTransform = Instantiate(newAbstractCharacter.GetPrefab, characterContainer).transform;
		newCharacterTransform.localPosition = Vector3.zero;
		newCharacterTransform.localRotation = Quaternion.identity;

		CharacterUniqueReferenceHandler newCharacterUniqueReferenceHandler = newCharacterTransform.gameObject.GetComponent<CharacterUniqueReferenceHandler>();

		//transfer firearm prefab to the new character
		//characterUniqueReferenceHandler.TransferFirearmToNewContainer(newCharacterUniqueReferenceHandler);

		//transfer whatever RuntimeAnimatorController is in use at the time
		RuntimeAnimatorController oldCharacterRuntimeAnimatorController = characterUniqueReferenceHandler.Animator.runtimeAnimatorController;
		newCharacterUniqueReferenceHandler.Animator.runtimeAnimatorController = oldCharacterRuntimeAnimatorController;

		//destroy the old character
		Destroy(characterUniqueReferenceHandler.gameObject);

		//set the new one as current
		characterUniqueReferenceHandler = newCharacterUniqueReferenceHandler;

		animator = characterUniqueReferenceHandler.Animator;

		if (isLocalPlayer)
		{
			Transform newFirstPersonCharacterTransform = Instantiate(newAbstractCharacter.GetFirstPersonPrefab, firstPersonCharacterContainer).transform;
			newFirstPersonCharacterTransform.localPosition = Vector3.zero;
			newCharacterTransform.localRotation = Quaternion.identity;

			CharacterUniqueReferenceHandler newFirstPersonCharacterUniqueReferenceHandler = newFirstPersonCharacterTransform.gameObject.GetComponent<CharacterUniqueReferenceHandler>();

			RuntimeAnimatorController oldFirstPersonCharacterRuntimeAnimatorController = firstPersonCharacterUniqueReferenceHandler.Animator.runtimeAnimatorController;
			newFirstPersonCharacterUniqueReferenceHandler.Animator.runtimeAnimatorController = oldFirstPersonCharacterRuntimeAnimatorController;

			Destroy(firstPersonCharacterUniqueReferenceHandler.gameObject);

			firstPersonCharacterUniqueReferenceHandler = newFirstPersonCharacterUniqueReferenceHandler;

			firstPersonAnimator = newFirstPersonCharacterUniqueReferenceHandler.Animator;

			firstPersonAnimator.SetTrigger(newAbstractCharacter.Name.ToString() + "PoseAdjustment");
		}

		StartCoroutine(WaitForSubscribers());
	}

	private IEnumerator WaitForSubscribers()
	{
		while (CharacterSelectedHandler == null)
		{
			yield return null;
		}
		CharacterSelectedHandler?.Invoke(characterUniqueReferenceHandler);
	}

	/*private void OnAnimatorIKTrigger(int layerIndex)
	{
		Debug.Log("test");
		
		animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);

		animator.SetIKPosition(AvatarIKGoal.RightHand, aimAtIK.position);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, aimAtIK.position);
	}*/

	private void LateUpdate()
	{
		//chest.LookAt(aimAtIK.position);
		//chest.rotation = chest.rotation * Quaternion.Euler(aimOffset);
	}
}
