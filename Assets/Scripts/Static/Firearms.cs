using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum FirearmUniqueIdentifier
{
	Default,
	Pistol,
	Uzi
}

public static class Firearms
{
	private static bool initialized;
	private static List<AbstractFirearm> firearms;

	private static void Initialize()
	{
		firearms = new List<AbstractFirearm>();
		
		var assembly = Assembly.GetAssembly(typeof(AbstractFirearm));

		var allFirearmTypes = assembly.GetTypes()
			.Where(t => typeof(AbstractFirearm).IsAssignableFrom(t) && !t.IsAbstract);

		foreach (var firearmType in allFirearmTypes)
		{
			AbstractFirearm firearm = Activator.CreateInstance(firearmType) as AbstractFirearm;
			firearms.Add(firearm);
		}

		initialized = true;
	}

	public static List<AbstractFirearm> GetFirearms()
	{
		if (!initialized)
		{
			Initialize();
		}
		return firearms;
	}

	/*public static List<AbstractFirearm> GetAbstractFirearms()
	{
		List<AbstractFirearm> firearmList = new List<AbstractFirearm>();

		

		return firearmList;
	}*/

	public static AbstractFirearm GetAbstractFirearmByUniqueIdentifier(FirearmUniqueIdentifier uniqueIdentifier) 
	{
		if (!initialized)
		{
			Initialize();
		}
		foreach (AbstractFirearm firearm in firearms)
		{
			if (firearm.FirearmUniqueIdentifier == uniqueIdentifier)
			{
				return firearm;
			}
		}

		Debug.LogError("AbstractFirearm not found through FirearmUniqueIdentifier");

		return null;
	}
}
