using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;
using Visiorama;
using PathologicalGames;


[RequireComponent(typeof (MeshRenderer))]
public class SubstanceResourceBar : MonoBehaviour
{
			
	private ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;
	private MeshRenderer subMeshRenderer;
	public IStats refTarget;
	public FactoryBase refFactory;
	public bool noTimer = false;
	public float refreshRate = 0.3f;
	
	public void Init ()
	{
		subMeshRenderer = GetComponent <MeshRenderer> ();
		subMeshRenderer.enabled = true;
		ProceduralMaterial mMaterial = subMeshRenderer.material as ProceduralMaterial;
		subMeshRenderer.sharedMaterial = mMaterial;
		substance 	  = mMaterial;
		curProperties = substance.GetProceduralPropertyDescriptions();
		refFactory = refTarget as FactoryBase;
        CheckInvokingTimer ();
	}


	public void CheckInvokingTimer ()
	{ 

		if (noTimer) 
		{
			Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (refFactory.team, 1);
			substance.SetProceduralColor("outputcolor", teamColor);
			InvokeRepeating ("UpdateInvokingConstruct",0, refreshRate);
		}

		else
		{
			Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (refFactory.team, 1);
			substance.SetProceduralColor("outputcolor", teamColor);
			InvokeRepeating ("UpdateInvokingTimer",0, refreshRate);
		}

	}

	public void UpdateInvokingTimer ()
	{
		if(refFactory == null || !refFactory.inUpgrade)
		{
			CancelInvoke("UpdateInvokingTimer");
			PoolManager.Pools["Selection"].Despawn(transform);
			return;
		}
		float percentResource = refFactory.timer * (3.14f/refFactory.timeToCreate);
		substance.SetProceduralFloat("Rotate", (percentResource-3.14f));
		substance.RebuildTextures ();
	}

	public void UpdateInvokingConstruct ()
	{
		if(refFactory == null || refFactory.wasBuilt) 
		{			
			CancelInvoke("UpdateInvokingConstruct");
			PoolManager.Pools["Selection"].Despawn(transform);
			return;
		}
		float percentResource = refFactory.Health * (3.14f/refFactory.MaxHealth);
		substance.SetProceduralFloat("Rotate", (percentResource-3.14f));
		substance.RebuildTextures ();

	}

	

}
