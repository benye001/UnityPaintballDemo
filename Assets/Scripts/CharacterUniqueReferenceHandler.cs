using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// References to in-character transforms, meant to be set in each Character's prefab, (e.g. the AbedCharacter prefab) via the inspector
/// </summary>
public class CharacterUniqueReferenceHandler : MonoBehaviour
{
    [SerializeField] private Transform handgunFirearmContainer;
    public Transform HandgunFirearmContainer => handgunFirearmContainer;


    [SerializeField] private Transform rifleFirearmContainer;
    public Transform RifleFirearmContainer => rifleFirearmContainer;


    [SerializeField] private Animator animator;
    public Animator Animator => animator;


    private Dictionary<CharacterAnimationType, Transform> containerDictionary;


    private bool initialized;


	private void Awake()
	{
        if (!initialized)
		{
            Initialize();
		}
    }

	private void Initialize()
	{
        containerDictionary = new Dictionary<CharacterAnimationType, Transform>()
        {
            {CharacterAnimationType.Handgun, handgunFirearmContainer},
            {CharacterAnimationType.Rifle, rifleFirearmContainer}
        };
        initialized = true;
    }

	public Transform GetFirearmContainerByCharacterAnimationType(CharacterAnimationType type)
	{
        if (!initialized)
        {
            Initialize();
        }
        return containerDictionary[type];
	}

    public void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeAnimatorController) 
	{
        animator.runtimeAnimatorController = runtimeAnimatorController;
	}

    public void DestroyFirearmContainerChildren()
	{
        if (!initialized)
        {
            Initialize();
        }
        if (containerDictionary != null)
        {
            foreach (KeyValuePair<CharacterAnimationType, Transform> keyValuePair in containerDictionary)
            {
                FirearmUniqueReferenceHandler[] childrenOfCurrent = keyValuePair.Value.gameObject.GetComponentsInChildren<FirearmUniqueReferenceHandler>();
                foreach (FirearmUniqueReferenceHandler child in childrenOfCurrent)
                {
                    Destroy(child.gameObject);
                }
            }
        }
	}

    /*public void TransferFirearmToNewContainer(CharacterUniqueReferenceHandler newContainer)
	{
        if (containerDictionary == null)
		{
            Debug.LogError("CONTAINER DICTIONARY");
            return;
		}
        
        foreach (KeyValuePair<CharacterAnimationType, Transform> keyValuePair in containerDictionary)
		{
            if (keyValuePair.Value == null)
            {
                Debug.LogError("CONTAINER DICTIONARY");
                return;
            }

            Transform firearm = keyValuePair.Value.gameObject.GetComponentInChildren<Transform>();

            if (firearm != null)
            {
                firearm.SetParent(newContainer.containerDictionary[keyValuePair.Key]);

                firearm.localPosition = Vector3.zero;
                firearm.localRotation = Quaternion.identity;
            }
            else
			{
                Debug.LogError("FIREARM");
			}
		}
	}*/
}
