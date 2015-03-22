using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Visiorama.Utils;
using Visiorama;

[RequireComponent(typeof (MeshRenderer))]
public class SubstanceResourceBar : MonoBehaviour
{
			
	private ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;
	private MeshRenderer subMeshRenderer;
	public IStats refTarget;
	public FactoryBase refFactory;
	public bool noTimer = false;
	
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
		if (noTimer) InvokeRepeating ("UpdateInvokingConstruct",0, 0.2f);
		else InvokeRepeating ("UpdateInvokingTimer",0, 0.2f);
	}

	public void UpdateInvokingTimer ()
	{
		if(refFactory == null || !refFactory.inUpgrade) DestroyImmediate(gameObject);

		float percentResource = refFactory.timer * (3.14f/refFactory.timeToCreate);

		foreach (ProceduralPropertyDescription curProperty in curProperties)
		{
			if (curProperty.type == ProceduralPropertyType.Float)
			{
				substance.SetProceduralFloat(curProperty.name, (percentResource-3.14f));
			}

			else
			{
				int teamID = refFactory.team;
				Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
					substance.SetProceduralColor(curProperty.name, teamColor);

			}
		}
		substance.RebuildTextures ();
	}

	public void UpdateInvokingConstruct ()
	{
		if(refFactory == null || refFactory.wasBuilt) DestroyImmediate(gameObject);
		
		float percentResource = refFactory.Health * (3.14f/refFactory.MaxHealth);
		
		foreach (ProceduralPropertyDescription curProperty in curProperties)
		{
			if (curProperty.type == ProceduralPropertyType.Float)
			{
				substance.SetProceduralFloat(curProperty.name, (percentResource-3.14f));
			}
			
			else
			{
				int teamID = refFactory.team;
				Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
					substance.SetProceduralColor(curProperty.name, teamColor);
				
			}
		}
		substance.RebuildTextures ();
	}

	

}
