using UnityEngine;
using System.Collections;

[System.Serializable]
public class ResourcesManager
{
	public int NumberOfRocks;
	
	public ResourcesManager ()
	{
		NumberOfRocks = 50;
	}
	
	public void Set (Resource.Type resourceType, int numberOfResources)
	{
		if (resourceType == Resource.Type.Rock)
		{
			NumberOfRocks += numberOfResources;
		}
	}
	
	public bool CanBuy (ResourcesManager resourceCost)
	{
		if (NumberOfRocks - resourceCost.NumberOfRocks < 0)
		{
			return false;
		}
		
		NumberOfRocks -= resourceCost.NumberOfRocks;
		return true;
	}
}

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
	
	public bool HasWorker {
		get
		{
			return worker != null;
		}
	}
	
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
			if (numberOfResources - worker.numberMaxGetResources <= 0)
			{
				DiscountResources (worker.numberMaxGetResources);
				if (!PhotonNetwork.offlineMode) photonView.RPC ("DiscountResources", PhotonTargets.OthersBuffered, worker.numberMaxGetResources);
				
				worker.GetResource (numberOfResources);
			}
			else
			{
				DiscountResources (worker.numberMaxGetResources);
				if (!PhotonNetwork.offlineMode) photonView.RPC ("DiscountResources", PhotonTargets.OthersBuffered, worker.numberMaxGetResources);
				
				worker.GetResource ();
			}
			currentResistance = resistance;
		}
	}
	
	[RPC]
	public void DiscountResources (int numberMaxGetResources)
	{
		numberOfResources = Mathf.Max (0, numberOfResources - numberMaxGetResources);
		if (numberOfResources == 0) Destroy (gameObject);
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
		
		this.worker = worker;
		return true;
	}
	
	public override void SetVisible (bool visible)
	{
		throw new System.NotImplementedException ();
	}
	
	public override bool IsVisible {
		get {
			throw new System.NotImplementedException ();
		}
	}
}
