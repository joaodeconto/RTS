using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

[System.Serializable]
public class ResourcesManager
{
	public int m_mana;
	public int m_rocks;

	private static int vezes = 0;
	public int Rocks {
		get { return m_rocks; }
		set { m_rocks = value;
		
			Model.Battle battle = GetCurrentBattle ();
			
			if (battle != null)
			{
//				Debug.LogError ("vezes: " + (++vezes));
				Score.SetScorePoints (DataScoreEnum.ResourcesGathered, m_rocks);
				Score.SetScorePoints (DataScoreEnum.ResourcesGathered, m_rocks, battle.IdBattle);
			}
		}
	}

	public int Mana {
		get { return m_mana; }
		set { m_mana = value;
			
			Model.Battle battle = GetCurrentBattle ();
			
			if (battle != null)
			{
				Score.SetScorePoints (DataScoreEnum.CurrentCrystals, m_mana);
				Score.SetScorePoints (DataScoreEnum.CurrentCrystals, m_mana, battle.IdBattle);
			}
		}
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
		if (resourceType == Resource.Type.Rock)
		{
			Rocks += numberOfResources;
		}

		if (resourceType == Resource.Type.Mana)
		{
			Mana += numberOfResources;
		}
	}

	public bool CanBuy (ResourcesManager resourceCost, bool discount = true)
	{
		if (Rocks - resourceCost.Rocks < 0)
		{
			return false;
		}

		if (discount) Rocks -= resourceCost.Rocks;
		return true;

		if (Mana - resourceCost.Mana < 0)
		{
			return false;
		}
		
		if (discount) Mana -= resourceCost.Mana;
		return true;
	}

	public void ReturnResources (ResourcesManager resourceCost, float percent = 1f)
	{
		Rocks += Mathf.FloorToInt((float)resourceCost.Rocks * percent);

		Mana += Mathf.FloorToInt((float)resourceCost.Mana * percent);
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

	public CapsuleCollider capsuleCollider { get; protected set; }

	void Awake ()
	{
		WorkersResistance = new Dictionary<Worker, int>();
		capsuleCollider   = GetComponent<CapsuleCollider> ();
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
