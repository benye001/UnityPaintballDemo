using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ManualFirearm : AbstractFirearm
{	
	public ManualFirearm() : base()
	{

	}

	public override void FireButtonDown()
	{
		base.FireButtonDown();
		FireIfCooldownFinished();
	}
}
