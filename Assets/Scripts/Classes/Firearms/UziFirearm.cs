using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UziFirearm : SemiautoFirearm
{
	public override CharacterAnimationType CharacterAnimationType => CharacterAnimationType.Rifle;

	public override FirearmUniqueIdentifier FirearmUniqueIdentifier => FirearmUniqueIdentifier.Uzi;

	protected override string BulletProfileFileName => "Semiauto";
	protected override string RecoilProfileFileName => "Semiauto";
	protected override string PreviewSpriteFileName => "Uzi";
	protected override string PrefabFileName => "Uzi";


	protected override float Damage => 10f;
	protected override float SpreadIncreasePerShot => 1f;
	protected override float TimeBetweenShots => 0.09f;
	protected override float Recoil => 0.75f;

	public UziFirearm() : base()
	{
		
	}
}
