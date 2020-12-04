using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterName
{
	Default,
	Abed,
	Pierce,
	Jeff,
	Annie,
	Shirley,
	Troy,
	Britta
}

public abstract class AbstractCharacter
{
	public virtual CharacterName Name => CharacterName.Default;
	
	protected virtual string PrefabFileName => "DefaultCharacter";
	protected GameObject prefab;
	public virtual GameObject GetPrefab => prefab;

	protected virtual string FirstPersonPrefabFileName => "FirstPerson DefaultCharacter";
	protected GameObject firstPersonPrefab;
	public virtual GameObject GetFirstPersonPrefab => firstPersonPrefab;

	public AbstractCharacter()
	{
		prefab = Resources.Load<GameObject>(ResourcePaths.CharacterPrefabs + "/" + PrefabFileName);
		firstPersonPrefab = Resources.Load<GameObject>(ResourcePaths.CharacterPrefabs + "/" + FirstPersonPrefabFileName);

		if (prefab == null)
		{
			Debug.LogError("an " + nameof(AbstractCharacter) + " has an invalid " + nameof(PrefabFileName) + ": " + PrefabFileName);
			prefab = Resources.Load<GameObject>(ResourcePaths.CharacterPrefabs + "/" + "DefaultCharacter");
		}

		if (firstPersonPrefab == null)
		{
			Debug.LogError("an " + nameof(AbstractCharacter) + " has an invalid " + nameof(FirstPersonPrefabFileName) + ": " + FirstPersonPrefabFileName);
			prefab = Resources.Load<GameObject>(ResourcePaths.CharacterPrefabs + "/" + "FirstPerson DefaultCharacter");
		}
	}
}
