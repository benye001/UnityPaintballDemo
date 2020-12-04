using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct RecoilProfileData
{
    public CurvedLerp horizontalRecovery;
    public CurvedLerp verticalRecovery;

    [Range(0.1f, 100f)]
    public float horizontalEasing;

    [Range(0.1f, 100f)]
    public float verticalEasing;

    public RecoilProfileData(RecoilProfileData recoilProfileData)
	{
        horizontalRecovery = new CurvedLerp(recoilProfileData.horizontalRecovery);
        verticalRecovery = new CurvedLerp(recoilProfileData.verticalRecovery);

        horizontalEasing = recoilProfileData.horizontalEasing;
        verticalEasing = recoilProfileData.verticalEasing;
    }
}

///
/// 
///     You should never change any values in here directly,
///     or reference it directly, you should copy it and then use it
///     When you need a copy of the profile, you should write something like 
/// 
///         RecoilProfileData test = new RecoilProfileData(Resources.Load<RecoilProfile>("Recoil Profiles/FileName").data);
///     
///     All instances of this ScriptableObject should exist in Assets/Resources/Recoil Profiles
/// 
///

[CreateAssetMenu(fileName = "New Recoil Profile", menuName = "SSAAG/New RecoilProfile")]
public class RecoilProfile : ScriptableObject
{
    public RecoilProfileData data;
}
