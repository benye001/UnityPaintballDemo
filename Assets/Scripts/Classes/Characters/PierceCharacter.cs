using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierceCharacter : AbstractCharacter
{
	public override CharacterName Name => CharacterName.Pierce;
	protected override string PrefabFileName => "PierceCharacter";
	protected override string FirstPersonPrefabFileName => "FirstPerson PierceCharacter";
}
