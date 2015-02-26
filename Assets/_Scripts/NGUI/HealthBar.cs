using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UISlider))]
public class HealthBar : MonoBehaviour, IHealthObserver
{
	public IHealthObservable Target { private set; get; }

	protected UISlider slider;

	void Awake ()
	{
		slider = GetComponent<UISlider> ();
	}
	
	void OnDestroy ()
	{
		Close ();
	}

	public void SetTarget (IHealthObservable target)
	{
		if (slider == null)
			Awake ();

		this.Target = target;
		this.Target.RegisterHealthObserver (this);
		//Forçando atualizaçao de vida atual
		this.Target.NotifyHealthChange ();
	}

	public void Close ()
	{
		if (Target != null)
			Target.UnRegisterHealthObserver (this);
	}

	#region IHealthObserver implementation

	public void UpdateHealth (int currentHealth)
	{
		if (Target == null)
			Debug.LogError ("Verifique se o metodo SetTarget foi chamado");

		slider.value = (float)currentHealth / (float)Target.MaxHealth;
	}

	#endregion
}
