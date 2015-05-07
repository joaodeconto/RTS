using UnityEngine;
using Visiorama;
using System.Collections;
using System.Collections.Generic;

public class AnimateInQueue : MonoBehaviour

{
	Animation mAnim;
	public List<AnimationClip> animList = new List<AnimationClip>();
		
	public void Init ()
	{
		mAnim = GetComponentInChildren<Animation>();
		
		if (mAnim == null)
		{
			Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has no Animation component");
			Destroy(this);
		}
		else
		{
			foreach (AnimationState state in mAnim)
			{
				state.wrapMode = WrapMode.Once; 

				if (state.name.Contains("Idle") || state.name.Contains("idle"))
				{
					animList.Add(state.clip);
					break;
				}
			}
			if(animList.Count>0) StartCoroutine ("PlayAnimsInOrder");
			else Debug.Log ("sem anima com esses nomes");
		}
	}

	public virtual IEnumerator PlayAnimsInOrder()
	{
		int i = 0;
		while( i < animList.Count)
		{
			AnimationClip clip = animList[i];
			Debug.Log(i + " " + clip.name);			   
			mAnim.CrossFade(clip.name);
			yield return new WaitForSeconds (clip.length);
			i++;
			if(i == animList.Count) i=0;

		}

	}
	
	/// <summary>
	/// If it's time to play a new idle break animation, do so.
	/// </summary>
	
	void Update ()
	{
	}
}
