using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;

public delegate void FireNetworkSafe();

public abstract class AbstractFirearm
{
	public event FireNetworkSafe FireNetworkSafeHandler;

	/// <summary>
	/// 
	///		CharacterAnimationType determines which transform the firearm prefab will be instantiated into, 
	///		as well as which RuntimeAnimatorControllers will be loaded at runtime
	/// 
	/// 
	///		because of this, there should be a firearm prefab container for every CharacterAnimationType.
	///		firearm prefab containers are defined by CharacterUniqueReferenceHandler, & you can set those Transform references in each character prefab individually
	///		example of a character prefab would be the AbedCharacter prefab
	///		
	/// 
	///		there should also be two RuntimeAnimatorControllers for each CharacterAnimationType: "XXX CharacterController" & "XXX FirstPersonController" 
	///		They should override from the default RuntimeAnimatorControllers, "Handgun CharacterController" & "Handgun FirstPersonController"
	///		In order to be correctly loaded at runtime, their naming scheme needs to be as so:
	///		
	///		"CharacterAnimationType CharacterController"
	///		"CharacterAnimationType FirstPersonController"
	///		
	///		both of these files should be placed under Resources/Runtime Animator Controllers
	/// 
	/// </summary>
	public virtual CharacterAnimationType CharacterAnimationType => CharacterAnimationType.Handgun;

	/// <summary>
	/// Needs to be set uniquely for every non abstract implementation of the AbstractFirearm class,
	/// e.g. PistolFirearm & UziFirearm
	/// </summary>
	public virtual FirearmUniqueIdentifier FirearmUniqueIdentifier => FirearmUniqueIdentifier.Default;

	protected virtual float TimeBetweenShots => 0.25f;
	public virtual float GetTimeBetweenShots => TimeBetweenShots;


	protected virtual float Recoil => 1f;
	public virtual float GetRecoil => Recoil;


	protected virtual float SpreadIncreasePerShot => 0.25f;
	public virtual float GetSpreadIncreasePerShot => SpreadIncreasePerShot;


	protected virtual float Damage => 25f;
	public virtual float GetDamage => Damage;


	protected virtual string BulletProfileFileName => "Default";


	protected BulletProfile bulletProfile;
	public virtual BulletProfile GetBulletProfile => bulletProfile;


	protected virtual string RecoilProfileFileName => "Default";


	protected RecoilProfile recoilProfile;
	public virtual RecoilProfile GetRecoilProfile => recoilProfile;


	protected virtual string PreviewSpriteFileName => "Default";


	protected Sprite previewSprite;
	public virtual Sprite GetPreviewSprite => previewSprite;


	protected GameObject prefab;
	public virtual GameObject GetPrefab => prefab;


	protected virtual string PrefabFileName => "Default";


	protected RuntimeAnimatorController characterRuntimeAnimatorController;
	public virtual RuntimeAnimatorController GetCharacterRuntimeAnimatorController => characterRuntimeAnimatorController;


	protected RuntimeAnimatorController firstPersonRuntimeAnimatorController;
	public virtual RuntimeAnimatorController GetFirstPersonRuntimeAnimatorController => firstPersonRuntimeAnimatorController;


	protected float shotCooldown;
	public virtual float GetShotCooldown => shotCooldown;


	protected float currentSpread;
	public virtual float GetCurrentSpread => currentSpread;

	public AbstractFirearm()
	{
		bulletProfile = Resources.Load<BulletProfile>(ResourcePaths.BulletProfiles + "/" + BulletProfileFileName);
		recoilProfile = Resources.Load<RecoilProfile>(ResourcePaths.RecoilProfiles + "/" + RecoilProfileFileName);
		previewSprite = Resources.Load<Sprite>(ResourcePaths.FirearmPreviews + "/" + PreviewSpriteFileName);
		prefab = Resources.Load<GameObject>(ResourcePaths.FirearmPrefabs + "/" + PrefabFileName);

		characterRuntimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ResourcePaths.RuntimeAnimatorControllers + "/" + CharacterAnimationType.ToString() + " CharacterController");
		firstPersonRuntimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ResourcePaths.RuntimeAnimatorControllers + "/" + CharacterAnimationType.ToString() + " FirstPersonController");

		if (bulletProfile == null)
		{
			Debug.LogError("an " + nameof(AbstractFirearm) + " has an invalid " + nameof(BulletProfileFileName) + ": " + BulletProfileFileName);
			bulletProfile = Resources.Load<BulletProfile>(ResourcePaths.BulletProfiles + "/" + "Default");
		}

		if (recoilProfile == null)
		{
			Debug.LogError("an " + nameof(AbstractFirearm) + " has an invalid " + nameof(RecoilProfileFileName) + ": " + RecoilProfileFileName);
			recoilProfile = Resources.Load<RecoilProfile>(ResourcePaths.RecoilProfiles + "/" + "Default");
		}

		if (previewSprite == null)
		{
			Debug.LogError("an " + nameof(AbstractFirearm) + " has an invalid " + nameof(PreviewSpriteFileName) + ": " + PreviewSpriteFileName);
			previewSprite = Resources.Load<Sprite>(ResourcePaths.FirearmPreviews + "/" + "Default");
		}

		if (prefab == null)
		{
			Debug.LogError("an " + nameof(AbstractFirearm) + " has an invalid " + nameof(PrefabFileName) + ": " + PrefabFileName);
			prefab = Resources.Load<GameObject>(ResourcePaths.FirearmPrefabs + "/" + "Default");
		}
	}

	/// <summary>
	/// Called for every frame that the Fire button is held down
	/// </summary>
	public virtual void FireButton()
	{	

	}

	/// <summary>
	/// Called on the frame that the Fire button is pressed
	/// </summary>
	public virtual void FireButtonDown()
	{
		//Debug.Log("test");
	}

	/// <summary>
	/// Called on the frame that the Fire button is released
	/// </summary>
	public virtual void FireButtonUp()
	{

	}

	public virtual void Tick()
	{
		if (shotCooldown > 0f)
		{
			shotCooldown = Mathf.Clamp(shotCooldown - Time.deltaTime, 0f, Mathf.Infinity);
		}
	}

	protected virtual void FireIfCooldownFinished()
	{
		if (Mathf.Approximately(shotCooldown, 0f))
		{
			shotCooldown = TimeBetweenShots;
			Fire();
		}
	}

	/// <summary>
	/// Lets subscribers know that a bullet should be fired
	/// Generally that subscriber would be PlayerWeaponHandler
	/// </summary>
	protected virtual void Fire()
	{
		FireNetworkSafeHandler?.Invoke();
	}
}
