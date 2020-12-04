using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RevolverFirearm : ManualFirearm
{
	public override CharacterAnimationType CharacterAnimationType => CharacterAnimationType.Handgun;

	public override FirearmUniqueIdentifier FirearmUniqueIdentifier => FirearmUniqueIdentifier.Pistol;

	protected override string BulletProfileFileName => "Handgun";
	protected override string RecoilProfileFileName => "Handgun";
	protected override string PreviewSpriteFileName => "Pistol";
	protected override string PrefabFileName => "Pistol";


	protected override float Damage => 40f;
	protected override float SpreadIncreasePerShot => 1f;
	protected override float TimeBetweenShots => 0.2f;
	protected override float Recoil => 1.75f;

	public RevolverFirearm() : base()
	{
		
	}
}
