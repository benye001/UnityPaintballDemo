using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbedCharacter : AbstractCharacter
{
	public override CharacterName Name => CharacterName.Abed;
	protected override string PrefabFileName => "AbedCharacter";
	protected override string FirstPersonPrefabFileName => "FirstPerson AbedCharacter";
}
