using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	public bool CanBuy (ResourcesManager resourceCost, bool discount = true)
	{
		if (NumberOfRocks - resourceCost.NumberOfRocks < 0)
		{
			return false;
		}
		
		if (discount) NumberOfRocks -= resourceCost.NumberOfRocks;
		return true;
	}
	
	public void ReturnResources (ResourcesManager resourceCost)
	{
		NumberOfRocks += resourceCost.NumberOfRocks;
	}
}

public class Resource : Photon.MonoBehaviour
{

	public enum Type
	{
		None,
		Rock
	}
	
	public Type type;
	public int numberOfResources = 200;
	public int resistance = 5;
	public int limitWorkers = 20;
	
	// Passa o Worker, sendo o int a Resistência Atual
	public Dictionary<Worker, int> WorkersResistance {get; protected set;}
	
	public bool IsLimitWorkers {
		get
		{
			return WorkersResistance.Count >= limitWorkers;
		}
	}
	
	public CapsuleCollider collider {get; protected set;}
	
	void Awake ()
	{
		WorkersResistance = new Dictionary<Worker, int>();
		collider = GetComponent<CapsuleCollider> ();
	}
	
	public void ExtractResource (Worker worker)
	{
		WorkersResistance[worker] = Mathf.Max (0, WorkersResistance[worker] - worker.forceToExtract);
		if (WorkersResistance[worker] == 0f)
		{
			if (numberOfResources - worker.numberMaxGetResources <= 0)
			{
				DiscountResources (worker.numberMaxGetResources);
				if (!PhotonNetwork.offlineMode) photonView.RPC ("DiscountResources", PhotonTargets.OthersBuffered, worker.numberMaxGetResources);
				else Destroy (gameObject);
				worker.GetResource (numberOfResources);
			}
			else
			{
				DiscountResources (worker.numberMaxGetResources);
				if (!PhotonNetwork.offlineMode) photonView.RPC ("DiscountResources", PhotonTargets.OthersBuffered, worker.numberMaxGetResources);
				
				worker.GetResource ();
			}
			WorkersResistance[worker] = resistance;
		}
	}
	
	[RPC]
	void DiscountResources (int numberMaxGetResources)
	{
		numberOfResources = Mathf.Max (0, numberOfResources - numberMaxGetResources);
		if (numberOfResources == 0) Destroy (gameObject);
	}
	
	public bool AddWorker (Worker worker)
	{
		if (!WorkersResistance.ContainsKey (worker))
		{
			if (WorkersResistance.Count < limitWorkers)
			{
				WorkersResistance.Add (worker, resistance);
			}
			else
			{
				return false;
			}
		}
		
		return true;
	}
	
	public bool RemoveWorker (Worker worker)
	{
		if (WorkersResistance.ContainsKey (worker))
		{
			WorkersResistance.Remove (worker);
			return true;
		}
		return false;
	}

}
