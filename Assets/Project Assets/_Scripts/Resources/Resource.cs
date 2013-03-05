using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour {

	public enum Type
	{
		Rock
	}
	
	public Type type;
	public int numberOfResources = 200;
	public int resistance = 5;
	public Constructor constructor {get; protected set;}
	
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
		Debug.Log ("currentResistance: " + currentResistance);
		if (currentResistance == 0f)
		{
			if (numberOfResources - constructor.numberMaxGetResources < 0)
			{
				constructor.GetResource (numberOfResources);
			}
			else
			{
				constructor.GetResource ();
			}
			currentResistance = resistance;
		}
	}
	
	public void SetBuilder (Constructor constructor)
	{
		if (constructor == null)
		{
			if (this.constructor == null) return;
		}
		else
		{
			if (this.constructor != null) return;
		}
		
		this.constructor = constructor;
	}
}
