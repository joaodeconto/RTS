using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {

	private Unit soldierTarget;
	private FactoryBase factoryTarget;
	
	protected UISlider slider;
	
	public delegate void UpdateHealthMethod ();
	protected UpdateHealthMethod CurrentTarget;
	
	void Awake ()
	{
		slider = GetComponent<UISlider> ();
	}
	
	public void SetTarget (Unit soldier)
	{
		soldierTarget = soldier;
		CurrentTarget = UnitHealth;
	}
	
	public void SetTarget (FactoryBase factory)
	{
		factoryTarget = factory;
		CurrentTarget = FactoryHealth;
	}
	
	public void Close ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (CurrentTarget != null) CurrentTarget ();
	}
	
	void UnitHealth ()
	{
		slider.sliderValue = (float)soldierTarget.Health / (float)soldierTarget.MaxHealth;
	}
	
	void FactoryHealth ()
	{
		slider.sliderValue = (float)factoryTarget.Health / (float)factoryTarget.MaxHealth;
	}
}
