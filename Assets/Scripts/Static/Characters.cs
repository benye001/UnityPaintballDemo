using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Characters
{
	private static bool initialized;
	private static List<AbstractCharacter> characters;

	private static void Initialize()
	{
		characters = new List<AbstractCharacter>();

		var assembly = Assembly.GetAssembly(typeof(AbstractCharacter));

		var allFirearmTypes = assembly.GetTypes()
			.Where(t => typeof(AbstractCharacter).IsAssignableFrom(t) && !t.IsAbstract);

		foreach (var firearmType in allFirearmTypes)
		{
			AbstractCharacter character = Activator.CreateInstance(firearmType) as AbstractCharacter;
			characters.Add(character);
		}

		initialized = true;
	}

	public static List<AbstractCharacter> GetCharacters()
	{
		if (!initialized)
		{
			Initialize();
		}
		return characters;
	}

	public static AbstractCharacter GetAbstractCharacterByName(CharacterName name)
	{
		if (!initialized)
		{
			Initialize();
		}
		foreach (AbstractCharacter character in characters)
		{
			if (character.Name == name)
			{
				return character;
			}
		}

		Debug.LogError("AbstractCharacter not found through CharacterName");

		return null;
	}
}