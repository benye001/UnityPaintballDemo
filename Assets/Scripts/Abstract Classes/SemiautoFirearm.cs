using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SemiautoFirearm : AbstractFirearm
{
	public SemiautoFirearm() : base()
	{

	}
	
	public override void FireButton()
	{
		base.FireButton();
		FireIfCooldownFinished();
	}
}