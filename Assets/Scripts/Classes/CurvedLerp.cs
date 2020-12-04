using System;
using System.Linq;
using UnityEngine;

/*
 * 
 *	NOTE:
 *	I pulled this class from my previous project, 
 *	it's a little messy, but all you need to know is:
 *	
 *	1.	it's meant to be a [SerializeField] editor variable
 *		
 *		so you can set the AnimationCurve in the inspector
 *	
 *	
 *	
 *	2.	the easiest way to use it is to just write something like 
 *		
 *		float multiplier = exampleCurvedLerp.LerpAlongCurve(); //returns a value from the lowest animation curve value to the highest animation curve value, depends how you set up the curve in the inspector
 *		
 *		
 *		
 *	3.	if you need to access the current evaluated value without increasing the lerp value (lerp value being CurrentTime), you can do
 *		
 *		float multiplier = exampleCurvedLerp.EvaluateAgainstCurve(exampleCurvedLerp.CurrentTime);
 *		
 *		
 *		
 *	4.	if you need the current lerp value from 0f-1f without evaluation against the animation curve, you can do
 *		
 *		float currentLerpTime01 = exampleCurvedLerp.CurrentTime;
 *		
 *		
 *		
 *	5.	you can subscribe to onCompletion.ContainedEventProperty, which is invoked when CurrentTime reaches 1f
 *		
 *		exampleCurvedLerp.onCompletion.ContainedEventProperty += OnExampleCurvedLerpCompletion;
 *		
 *		private void OnExampleCurvedLerpCompletion(object sender, EventArgs e)
 *		{
 *			
 *		}
 *		
 */

/// <summary>
/// Easy lerping with animation curves
/// </summary>
[Serializable]
public class CurvedLerp
{
	[SerializeField] private AnimationCurve[] alternativeAnimationCurves;

	private AnimationCurve currentAnimationCurve;
	public AnimationCurve CurrentAnimationCurve
	{
		get
		{
			if (currentAnimationCurve == null)
			{
				if (!SwitchToAlternativeAnimationCurveByIndex(0))
				{
					return AnimationCurve.Linear(0f, 0f, 1f, 1f);
				}
			}
			return currentAnimationCurve;
		}
		set => currentAnimationCurve = value;
	}

	public float totalTimeToCompletion;
	public float startingLerpCompletion;

	public bool useNegatives;

	[SerializeField] private float currentTime;
	public float CurrentTime
	{
		get
		{
			CurrentTimeWasAccessed();
			return currentTime;
		}
		set
		{
			currentTime = value;
			CurrentTimeWasAccessed();
		}
	}

	private float evaluatedCurrentLerpCompletion;

	public SafeEventHandlerShell<EventArgs> onCompletion;
	private bool alreadyFiredOnCompletion;

	public CurvedLerp(AnimationCurve[] alternativeAnimationCurves, int currentAnimationCurveByIndex, float totalTimeToCompletion, float startingLerpCompletion)
	{
		this.alternativeAnimationCurves = alternativeAnimationCurves;
		currentAnimationCurve = this.alternativeAnimationCurves[currentAnimationCurveByIndex];
		this.totalTimeToCompletion = totalTimeToCompletion;

		this.startingLerpCompletion = startingLerpCompletion;

		currentTime = startingLerpCompletion;
		evaluatedCurrentLerpCompletion = currentAnimationCurve.Evaluate(currentTime);
	}

	public CurvedLerp(CurvedLerp toCopy)
	{
		alternativeAnimationCurves = new AnimationCurve[toCopy.alternativeAnimationCurves.Length];
		for (int i = 0; i < alternativeAnimationCurves.Length; i++)
		{
			alternativeAnimationCurves[i] = toCopy.alternativeAnimationCurves[i];
		}

		currentAnimationCurve = alternativeAnimationCurves[0];

		totalTimeToCompletion = toCopy.totalTimeToCompletion;
		startingLerpCompletion = toCopy.startingLerpCompletion;

		currentTime = startingLerpCompletion;
		evaluatedCurrentLerpCompletion = currentAnimationCurve.Evaluate(currentTime);
	}

	/// <summary>
	/// Lerps currentLerpCompletion from 0f to 1f based on lerpSpeed
	/// </summary>
	/// <returns>currentLerpCompletion evaluated by animationCurve</returns>
	public float LerpAlongCurve()
	{
		evaluatedCurrentLerpCompletion = CurrentAnimationCurve.Evaluate(CurrentTime);

		currentTime = Mathf.Clamp(currentTime + (Time.deltaTime / totalTimeToCompletion), startingLerpCompletion, 1f);

		currentTime = Mathf.Lerp(0f, 1f, currentTime);

		return evaluatedCurrentLerpCompletion;
	}

	public float EvaluateAgainstCurve(float floaty)
	{
		return CurrentAnimationCurve.Evaluate(floaty);
	}

	/// <summary>
	/// Sets currentLerpCompletion to 0f
	/// </summary>
	public void ResetCurrentLerpCompletion()
	{
		currentTime = startingLerpCompletion;
	}

	public void CurrentTimeWasAccessed()
	{
		currentTime = Mathf.Clamp(currentTime, startingLerpCompletion, 1f);
		if (Mathf.Approximately(currentTime, 1f))
		{
			if (!alreadyFiredOnCompletion)
			{
				onCompletion.Invoke(this, EventArgs.Empty);
			}
			alreadyFiredOnCompletion = true;
		}
		else
		{
			alreadyFiredOnCompletion = false;
		}
	}

	public void AddToCurrentTimeWithClamp(float additive, float clamp, bool canReduceTime = false)
	{
		float calculated = Mathf.Clamp(currentTime + additive, 0f, clamp);

		if (!canReduceTime && calculated < currentTime)
		{
			//nothing
		}
		else
		{
			CurrentTime = calculated;
		}

	}

	public bool IsComplete => (Mathf.Approximately(CurrentTime, 1f));

	/// <summary>
	/// 
	/// </summary>
	/// <param name="index"></param>
	/// <returns>success</returns>
	public bool SwitchToAlternativeAnimationCurveByIndex(int index)
	{
		if (alternativeAnimationCurves != null && index < alternativeAnimationCurves.Length && index >= 0)
		{
			CurrentAnimationCurve = alternativeAnimationCurves[index];
			return true;
		}

		Debug.LogWarning("CurvedLerp.SwitchToAlternativeAnimationCurveByIndex was called with an invalid index");
		return false;
	}

	public float EvaluatedCurrentLerpCompletion
	{
		get
		{
			evaluatedCurrentLerpCompletion = CurrentAnimationCurve.Evaluate(CurrentTime);
			return evaluatedCurrentLerpCompletion;
		}
	}
}


