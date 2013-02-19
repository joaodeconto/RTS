using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {

	public Unit soldier;
	
	protected UISlider slider;
	
	void Awake ()
	{
		slider = GetComponent<UISlider> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		slider.sliderValue = (float)soldier.Health / (float)soldier.MaxHealth;
	}
}
