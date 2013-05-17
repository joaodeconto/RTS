using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	
	[System.NonSerialized]
	public IStats target;

	protected UISlider slider;

	public delegate void UpdateHealthMethod ();
	protected UpdateHealthMethod updateHealthMethod;

	void Awake ()
	{
		slider = GetComponent<UISlider> ();
	}

	public void SetTarget (IStats target)
	{
		this.target        = target;
		updateHealthMethod = DefaultMethod;
	}

	public void Close ()
	{
		updateHealthMethod = null;
	}

	void Update ()
	{
		if (updateHealthMethod != null) updateHealthMethod ();
	}

	void DefaultMethod ()
	{
		slider.sliderValue = (float)target.Health / (float)target.MaxHealth;
	}
}
