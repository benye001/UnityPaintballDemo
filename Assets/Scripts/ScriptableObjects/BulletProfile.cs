using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 *  q: 
 *      why have BulletProfiles in addition to XXX::AbstractWeapon classes? shouldn't all bullet attributes be determined by the XXX::AbstractWeapon classes?
 *  
 *  a: 
 *      this is to decrease player confusion and frustration with too many abstract and non-tangibal differences between weapons.
 *      with this system, we can have categories of weapons, so that the player doesn't have to re-learn how to lead their shots everytime they switch to a different weapon due to different speed and bullet drop, 
 *      which would be confusing and unmanageable for the player, not to mention they probably wouldn't even notice the differences and just think the game has bad hit detection
 * 
 */

[CreateAssetMenu(fileName = "New Bullet Profile", menuName = "SSAAG/New BulletProfile")]
public class BulletProfile : ScriptableObject
{
    public CurvedLerp dropCurve;
    public float speed;
    public float lifetime;
}
