using UnityEngine;
using System.Collections;

public class Resource : IStats
{

	public enum Type
	{
		Rock
	}
	
	public Type type;
	public int numberOfResources = 200;
	public int resistance = 5;
	public Worker worker {get; protected set;}
	
	public CapsuleCollider collider {get; protected set;}
	
	private float currentResistance;
	
	void Awake ()
	{
		currentResistance = resistance;
		collider = GetComponent<CapsuleCollider> ();
	}
	
	public void ExtractResource (int forceToExtract)
	{
		currentResistance = Mathf.Max (0, currentResistance - forceToExtract);
		if (currentResistance == 0f)
		{
			if (numberOfResources - worker.numberMaxGetResources < 0)
			{
				numberOfResources = Mathf.Max (0, numberOfResources - worker.numberMaxGetResources);
				
				worker.GetResource (numberOfResources);
				Destroy (gameObject);
			}
			else
			{
				numberOfResources = Mathf.Max (0, numberOfResources - worker.numberMaxGetResources);
				
				worker.GetResource ();
			}
			currentResistance = resistance;
		}
	}
	
	public bool SetWorker (Worker worker)
	{
		if (worker == null)
		{
			if (this.worker == null) return false;
		}
		else
		{
			if (this.worker != null) return false;
		}
		
		Debug.Log (worker);
		
		this.worker = worker;
		return true;
	}
}
