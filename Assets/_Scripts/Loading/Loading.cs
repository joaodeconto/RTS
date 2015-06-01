using UnityEngine;
using System.Collections;

public class Loading : MonoBehaviour {

	private TweenAlpha loadAlpha;

	void OnEnable()
	{
		loadAlpha = GetComponent<TweenAlpha>();
		loadAlpha.enabled = false;
	}

	
	public void DisableLoad()
	{
		gameObject.SetActive(false);
	}
	
	public void reverseAlpha ()
	{
		loadAlpha.enabled = true;
		loadAlpha = GetComponent<TweenAlpha>();
		loadAlpha.PlayReverse();
		Invoke("DisableLoad",2f);
	}

	public void forwardAlpha ()
	{
		loadAlpha.enabled = true;
		loadAlpha = GetComponent<TweenAlpha>();
		loadAlpha.PlayForward();
	}
}
