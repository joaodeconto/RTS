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
	private bool colored = false;
	public IStats refTarget;
	public FactoryBase refFactory;
	
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
		InvokeRepeating ("UpdateInvokingTimer",0, 0.1f);
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

			if (!colored)
				if(!colored)
			{
				int teamID = refFactory.team;
				Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
					substance.SetProceduralColor(curProperty.name, teamColor);
				colored = true;
			}
		}
		substance.RebuildTextures ();
	}

	

}
