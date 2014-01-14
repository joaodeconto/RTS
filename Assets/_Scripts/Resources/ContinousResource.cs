using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContinousResource : Resource {

	public int x;


	public override void ExtractResource (Worker worker)
	{
		InvokeRepeating ("GetContinousResource", x,x);
		
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

	private void getContinousResource




}