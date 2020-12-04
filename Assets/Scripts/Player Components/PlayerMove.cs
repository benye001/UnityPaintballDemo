using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public delegate void MoveInputChanged(Vector2 moveInput);

public class PlayerMove : NetworkBehaviour, IControlledByMainController
{

    public event MoveInputChanged MoveInputChangedHandler;

	#region References

	private CharacterController characterController;

	#endregion


	#region Inspector Parameters

	[SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5.5f;

    [SerializeField] [Range(0f, 1f)] private float movementInterpolation = 0.5f;

    [SerializeField] private float gravity = 3f;

    #endregion

    [SerializeField] private bool sprinting = false;

    private float sprintCooldown = 0f;

    private Vector3 targetMovementVelocity = Vector3.zero;
    private Vector3 interpolatedMovementVelocity = Vector3.zero;

    private Vector2 input;

    public void Initialize()
    {
        FindReferences();

        if (isLocalPlayer)
		{
            LocalPlayerStart();
		}
    }

    public void ResetToDefaults()
	{

	}

    private void LocalPlayerStart()
	{
        //nothing yet
	}

    private void FindReferences()
	{
        characterController = GetComponent<CharacterController>();
    }

    public void Tick()
    {
        if (isLocalPlayer)
		{
            LocalPlayerUpdate();
		}
    }

    

    private void LocalPlayerUpdate()
	{
        Move();
        if (sprintCooldown > 0f)
        {
            sprintCooldown -= Time.deltaTime;
        }
    }

    private void Move()
	{
        Vector2 previousInput = input;
        input = GetWASDInputAsVector2();

        bool previousSprinting = sprinting;

        if (
            //if any of these
            (input.magnitude == 0f || 
            input.y <= 0f || 
            (Mathf.Abs(input.x) > Mathf.Abs(input.y)) ||
            sprintCooldown > 0f)
            
            //are in combination with this
            )
		{
            //then this
            sprinting = false;
		}
        else if (Input.GetKey(KeyCode.LeftShift))
		{
            sprinting = true;
		}

        targetMovementVelocity = transform.forward * input.y;
        targetMovementVelocity += transform.right * input.x;
        targetMovementVelocity = targetMovementVelocity.normalized * ((sprinting) ? sprintSpeed : walkSpeed) * Time.deltaTime;

        interpolatedMovementVelocity = Vector3.Lerp(interpolatedMovementVelocity, targetMovementVelocity, movementInterpolation);

        if (characterController.isGrounded)
        {
            interpolatedMovementVelocity.y = 0f;
        }
        else
		{
            interpolatedMovementVelocity.y -= gravity;
        }

        //if there has been a change in the movement of the character, sync across clients
        if (input != previousInput || sprinting != previousSprinting)
        {
            NetworkSafe_Move(input, sprinting);
        }

        characterController.Move(interpolatedMovementVelocity);
    }

    /// <summary>
    ///     
    ///     TEMPORARY IMPLEMENTATION:
    ///         Only syncs animation across clients, 
    ///         movement synchronization is handled by the NetworkTransform component 
    /// 
    ///     PLANNED IMPLEMENTATION:
    ///         Sync animation & movement
    ///         with LocalPlayer client side simulation
    ///         as well as non-LocalPlayer client side simulation & prediction 
    /// 
    /// </summary>
    /// <param name="rawInput">Raw WASD input</param>
    private void NetworkSafe_Move(Vector2 rawInput, bool sprinting)
	{
        if (isServer)
		{
            Local_Move(rawInput, sprinting);
            Rpc_Move(rawInput, sprinting);
		}
        else
		{
            Local_Move(rawInput, sprinting);
            Cmd_Move(rawInput, sprinting);
		}
	}

    private void Local_Move(Vector2 rawInput, bool sprinting)
	{
        float blendTreeMultiplier = (sprinting) ? 1f : 0.5f;

        MoveInputChangedHandler?.Invoke(rawInput * blendTreeMultiplier);
    }

    [Command]
    private void Cmd_Move(Vector2 rawInput, bool sprinting)
	{
        Rpc_Move(rawInput, sprinting);
	}

    [ClientRpc]
    private void Rpc_Move(Vector2 rawInput, bool sprinting)
	{
        //prevents the Rpc from coming back to the original caller, thus preventing Local_Move from being called twice for no reason
        if (!isLocalPlayer)
		{
            Local_Move(rawInput, sprinting);
        }
	}

    /// <summary>
    /// stops sprinting for X amount of time
    /// e.g. when shooting
    /// </summary>
    public void InterruptSprinting()
	{
        sprintCooldown = 0.45f;
	}

    private Vector2 GetWASDInputAsVector2()
	{
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.A))
        {
            input.x -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            input.x += 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            input.y += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input.y -= 1f;
        }

        return input;
    }
}
