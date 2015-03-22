using UnityEngine;
using System.Collections;

public class Loading : MonoBehaviour {

	private TweenAlpha loadAlpha;

	void OnEnable()
	{
		loadAlpha = GetComponent<TweenAlpha>();
		loadAlpha.Play();
	}

	
	void DisableLoad()
	{
		gameObject.SetActive(false);
	}
	
	public void reverseAlpha ()
	{		 
		loadAlpha = GetComponent<TweenAlpha>();
		loadAlpha.PlayReverse();
		Invoke("DisableLoad",2f);
	}
}
