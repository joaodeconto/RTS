using UnityEngine;
using System.Collections;

/// <summary>
/// Class Animation Extensions.
/// </summary>
public static class AnimationExtensions
{

	/// <summary>
	/// Play a clip to pass in param.
	/// </summary>
	public static void PlayClip(this Animation animation,
								AnimationClip animationClip)
	{
		animation.Play (animationClip.name);
	}
	
	public static void PlayClip(this Animation animation,
								AnimationClip animationClip, PlayMode playMode)
	{
		animation.Play (animationClip.name, playMode);
	}


	public static void PlayQueuedClip(this Animation animation,
								AnimationClip animationClip, QueueMode queueMode)
	{
		animation.PlayQueued (animationClip.name, queueMode);
	}

	/// <summary>
	/// Play a clip in Cross Fade.
	/// </summary>
    public static void PlayCrossFade(	this Animation animation,
									 	AnimationClip animationClip)
	{
		animation.CrossFade (animationClip.name);
	}

    public static void PlayCrossFade(	this Animation animation,
									 	AnimationClip animationClip, WrapMode wrapMode)
	{
		animation.CrossFade (animationClip.name);
		animation[animationClip.name].wrapMode = wrapMode;
	}
	
	public static void PlayCrossFade(	this Animation animation,
									 	AnimationClip animationClip, WrapMode wrapMode, PlayMode playMode)
	{
		animation.CrossFade (animationClip.name, 0.3f, playMode);
		animation[animationClip.name].wrapMode = wrapMode;
	}

	public static void PlayCrossFadeQueued(	this Animation animation,
									 	AnimationClip animationClip)
	{
		animation.CrossFadeQueued (animationClip.name);
	}

	public static void Rewind (	this Animation animation,
								AnimationClip animationClip)
	{
		animation.Rewind(animationClip.name);
	}

	public static void Stop (	this Animation animation,
								AnimationClip animationClip)
	{
		animation.Stop(animationClip.name);
	}


	public static bool IsPlayingClip(	this Animation animation,
									 	AnimationClip animationClip)
	{
		return animation.IsPlaying (animationClip.name);
	}

	public static bool IsPlayingClip(	this Animation animation,
									 	string animationName)
	{
		return animation.IsPlaying (animationName);
	}

	/// <summary>
	/// <para> While animating, doesn't run the remaining code. </para>
	/// <para> Using this when animation wrap should be "Once" or "PingPong". </para>
	/// <see cref="yield return StartCoroutine (animation.WhilePlaying ());"/>
	/// </summary>
    public static IEnumerator WhilePlaying( this Animation animation )
    {
        do
        {
            yield return null;
        }
		while ( animation.isPlaying );
    }

	/// <summary>
	/// <para> While animating, doesn't run the remaining code. </para>
	/// <para> Using this when animation wrap should be "Once" or "PingPong". </para>
	/// <see cref="yield return StartCoroutine (animation.WhilePlaying (animationClip));"/>
	/// </summary>
    public static IEnumerator WhilePlaying( this Animation animation,
									 		AnimationClip animationClip )
    {
        do
        {
            yield return null;
        }
		while ( animation.IsPlayingClip (animationClip) );
    }
	
	/// <summary>
	/// <para> While animating, doesn't run the remaining code. </para>
	/// <para> Using this when animation wrap should be "Once" or "PingPong". </para>
	/// <see cref="yield return StartCoroutine (animation.WhilePlaying (\"animationName\"));"/>
	/// </summary>
	public static IEnumerator WhilePlaying( this Animation animation,
									 		string animationName )
    {
        do
        {
            yield return null;
        }
		while ( animation.IsPlayingClip (animationName) );
    }

	public static IEnumerator WaitForAnimation ( this Animation animation,
									 		string animationName )
    {
        yield return new WaitForSeconds (animation[animationName].length);
    }

	public static IEnumerator WaitForAnimation ( this Animation animation,
									 		string animationName,
											float timePlus)
    {
        yield return new WaitForSeconds (animation[animationName].length + timePlus);
    }

	public static IEnumerator WaitForAnimation ( this Animation animation,
									 		AnimationClip animationClip )
    {
        yield return new WaitForSeconds (animationClip.length);
    }

	public static IEnumerator WaitForAnimation ( this Animation animation,
									 		AnimationClip animationClip,
											float timePlus)
    {
        yield return new WaitForSeconds (animationClip.length + timePlus);
    }

	public static void SetLayer ( this Animation animation,
								  AnimationClip animationClip,
								  int numberLayer)
	{
		animation[animationClip.name].layer = numberLayer;
	}

	public static void AddMixingTransform (	this Animation animation,
										   	AnimationClip animationClip,
											Transform mix)
	{
		animation[animationClip.name].AddMixingTransform(mix);
	}
}
