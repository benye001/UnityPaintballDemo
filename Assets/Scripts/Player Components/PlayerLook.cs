using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : NetworkBehaviour, IControlledByMainController
{
	#region References

	private Transform mainCamera;
    public Transform MainCamera
	{
        get
		{
            return mainCamera;
		}
	}

	#endregion

	#region Inspector Parameters

	[SerializeField] private Vector2 mouseSensitivity = new Vector2(1f, 1f);

    [SerializeField] private Vector3 cameraLocalPosition;

    #endregion

    #region Internal Values

    private Vector3 localCameraRotation;
    private Vector3 cameraRecoil;
    private Vector3 cameraRoll;
    private Vector3 localCameraOffsetEased;

    private Vector3 globalPlayerRotation;
    private Vector3 playerRecoil;
    private Vector3 globalPlayerOffsetEased;

    #endregion

    [Header("Local Camera Rotation")]

    [SerializeField] private AnimationCurve verticalRecoilByVerticalAim;

    [Header("Global Player Rotation")]

    [SerializeField] private AnimationCurve horizontalRecoilByVerticalAim;

    private RecoilProfileData recoilProfileData;

    public void Initialize()
    {
        FindReferences();

        if (isLocalPlayer)
		{
            LocalPlayerStart();
		}
    }

    public void SetRecoilProfile(RecoilProfile recoilProfile)
	{
        recoilProfileData = new RecoilProfileData(recoilProfile.data);
	}

    public void ResetToDefaults()
	{

	}

    private void LocalPlayerStart()
	{
        SetCameraParentToTransform();
	}

    private void FindReferences()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void SetCameraParentToTransform()
    {
        mainCamera.SetParent(transform);
    }

    public void Tick()
    {
        if (isLocalPlayer)
		{
            LocalPlayerTick();
		}
    }

    private void LocalPlayerTick()
	{
        Look();
        mainCamera.localPosition = cameraLocalPosition;
    }

    public void AddRecoil(float recoil)
	{
        float multiplierByCameraLocalX = mainCamera.forward.y;

        //Debug.Log(multiplierByCameraLocalX);

        System.Random random = new System.Random();
        float horizontalMultiplierNeg1ToPos1 = (random.Next(0, 2) == 0) ? -1f : 1f;

        playerRecoil.y = (playerRecoil.y * (1f - recoilProfileData.horizontalRecovery.EvaluatedCurrentLerpCompletion)) + (recoil * horizontalRecoilByVerticalAim.Evaluate(multiplierByCameraLocalX)) * horizontalMultiplierNeg1ToPos1;
        cameraRecoil.x = (cameraRecoil.x * (1f - recoilProfileData.verticalRecovery.EvaluatedCurrentLerpCompletion)) - (recoil * verticalRecoilByVerticalAim.Evaluate(multiplierByCameraLocalX));

        recoilProfileData.verticalRecovery.ResetCurrentLerpCompletion();
        recoilProfileData.horizontalRecovery.ResetCurrentLerpCompletion();
    }

	private void Look()
    {
       /*if (recoilProfileData == null)
		{
            return;
		}*/
        
        Vector2 mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector2 lookIncrement = new Vector2(mouseAxis.x * mouseSensitivity.x, mouseAxis.y * mouseSensitivity.y);

        if (lookIncrement.y > 0f && cameraRecoil.x < 0f)
		{
            cameraRecoil.x += lookIncrement.y;
            if (cameraRecoil.x > 0f)
			{
                localCameraRotation.x += -cameraRecoil.x;
                cameraRecoil.x = 0f;
			}
        }
        else 
        {
            localCameraRotation.x += lookIncrement.y;
		}

        Vector3 horizontal = Vector3.Lerp(playerRecoil, Vector3.zero, recoilProfileData.horizontalRecovery.EvaluatedCurrentLerpCompletion);
        Vector3 vertical = Vector3.Lerp(cameraRecoil, Vector3.zero, recoilProfileData.verticalRecovery.EvaluatedCurrentLerpCompletion);

        globalPlayerOffsetEased = Vector3.Lerp(globalPlayerOffsetEased, horizontal, recoilProfileData.horizontalEasing * Time.deltaTime);
        localCameraOffsetEased = Vector3.Lerp(localCameraOffsetEased, vertical, recoilProfileData.verticalEasing * Time.deltaTime);

        localCameraRotation.x = Mathf.Clamp(localCameraRotation.x, -90f, 90f);
        globalPlayerRotation.y += (mouseAxis.x * mouseSensitivity.x);

        transform.eulerAngles = globalPlayerRotation + globalPlayerOffsetEased;
        mainCamera.localEulerAngles = localCameraRotation + localCameraOffsetEased;

        recoilProfileData.horizontalRecovery.LerpAlongCurve();
        recoilProfileData.verticalRecovery.LerpAlongCurve();
    }

    
}
