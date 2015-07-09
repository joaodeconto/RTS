using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama;


[System.Serializable]
public class ResourcesManager
{
	public int m_mana;
	public int m_rocks;

//	private static int vezes = 0;
	public int Rocks {
		get { return m_rocks; }
		set { m_rocks = value;}
	}

	public int Mana {
		get { return m_mana; }
		set { m_mana = value;}
	}
	
	private Model.Battle GetCurrentBattle  ()
	{
		PhotonWrapper pw = ComponentGetter.Get<PhotonWrapper> ();
		Model.Battle battle = null;
		if (pw.GetPropertyOnRoom ("battle") != null)
		{
			battle = (new Model.Battle((string)pw.GetPropertyOnRoom ("battle")));
		}
		return battle;
	}
	
	public void DeliverResources (Resource.Type resourceType, int numberOfResources)
	{
		Model.Battle battle = GetCurrentBattle ();
		
		if (resourceType == Resource.Type.Rock)
		{
			Rocks += numberOfResources;
			
			if (!PhotonNetwork.offlineMode)	 
				Score.AddScorePoints (DataScoreEnum.GoldGathered, numberOfResources, battle.IdBattle);
			else
			{
				OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
				oScore.oPlayers[0].AddScorePlayer (DataScoreEnum.GoldGathered, numberOfResources);			
			}
		}

		if (resourceType == Resource.Type.Mana)
		{
			Mana += numberOfResources;
			
			if (!PhotonNetwork.offlineMode)
				Score.AddScorePoints (DataScoreEnum.ManaGathered, numberOfResources, battle.IdBattle);
			else
			{
				OfflineScore oScore = ComponentGetter.Get<OfflineScore>();
				oScore.oPlayers[0].AddScorePlayer (DataScoreEnum.ManaGathered, numberOfResources);			
			}
		}
	}
	
	public void UseResources (ResourcesManager resourceCost)
	{				
		Rocks -= resourceCost.Rocks;
		Mana  -= resourceCost.Mana;		
	}

	public bool CanBuy (ResourcesManager resourceCost)
	{
//		Debug.Log ("Rocks: " + Rocks + " - resourceCost.Rocks: " + resourceCost.Rocks);
//		Debug.Log ("Mana: " + Mana + " - resourceCost.Mana: " + resourceCost.Mana);
		
		return ((Rocks - resourceCost.Rocks) >= 0 &&
				(Mana  - resourceCost.Mana)  >= 0);
	}

	public void ReturnResources (ResourcesManager resourceCost, float percent = 1f)
	{
		DeliverResources(Resource.Type.Rock, Mathf.FloorToInt((float)resourceCost.Rocks * percent) );
		DeliverResources(Resource.Type.Mana, Mathf.FloorToInt((float)resourceCost.Mana * percent) );
	}
}

public class Resource : Photon.MonoBehaviour
{
	public enum Type
	{
		None,
		Rock,
		Mana,
	}

	public Type type;
	public int maxResources;
	public int numberOfResources;

	
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

	public CapsuleCollider capsuleCollider { get; protected set; }

	void Awake ()
	{	
		WorkersResistance	  = new Dictionary<Worker, int>();
		capsuleCollider  	  = GetComponent<CapsuleCollider> ();
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

	public void NotifyResourceChange ()
	{
				
	}

}

