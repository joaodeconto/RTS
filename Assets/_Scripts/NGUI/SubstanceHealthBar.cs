﻿using UnityEngine;
using System.Collections;
using Visiorama;
using PathologicalGames;

public class SubstanceHealthBar : MonoBehaviour, IHealthObserver
{
	public IHealthObservable Target { private set; get; }
	private int TargetTeamID;
	private ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;
	private MeshRenderer subMeshRenderer;
	private bool initializedSub;

	void OnSpawned ()
	{
		if(!initializedSub){

			subMeshRenderer = GetComponent <MeshRenderer> ();
			subMeshRenderer.enabled = false;
			
			//		Material mMaterial = new Material (subMeshRenderer.sharedMaterial);
			//		substance 	  = subMeshRenderer.sharedMaterial as ProceduralMaterial;
	//		ProceduralMaterial mmMaterial = new ProceduralMaterial ();
	//		mmMaterial.CopyPropertiesFromMaterial (subMeshRenderer.sharedMaterial as ProceduralMaterial);

			ProceduralMaterial mMaterial = subMeshRenderer.material as ProceduralMaterial;

			subMeshRenderer.sharedMaterial = mMaterial;
			substance 	  = mMaterial;
			curProperties = substance.GetProceduralPropertyDescriptions();
		}
		
	}

	void OnDespawned ()
	{
		if (Target != null)
			Target.UnRegisterHealthObserver (this);
//		UpdateHealth(0);
	}

	public void SetTarget (IHealthObservable target, int teamID)
	{
		if (substance == null) OnSpawned ();
		
		this.TargetTeamID = teamID;
		this.Target = target;
		this.Target.RegisterHealthObserver (this);		//Forçando atualizaçao de vida atual
		this.Target.NotifyHealthChange ();		
		subMeshRenderer.enabled = true;
	}
	
#region IHealthObserver implementation
	
	public void UpdateHealth (int currentHealth)
	{		
		if (Target == null)
		{
		//	Debug.LogError ("Verifique se o metodo SetTarget foi chamado");
		}
			
		if (subMeshRenderer.enabled)
		{
			float percentHealth = (float)currentHealth * (3.14f/(float)Target.MaxHealth);
	
			//Monkey patch: Min = -3.14f Max = 0f
//			percentHealth = 0.5f + percentHealth * 0.5f;
	
			foreach (ProceduralPropertyDescription curProperty in curProperties)
			{
				if (curProperty.type == ProceduralPropertyType.Float)
				{
					substance.SetProceduralFloat(curProperty.name, (percentHealth-3.14f));
				}
//				if(!colored)
//				{
//					int teamID = this.TargetTeamID;
//					Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
//					if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
//						substance.SetProceduralColor(curProperty.name, teamColor);
//					colored = true;
//				}
			}

		}

		substance.RebuildTextures ();
	}

#endregion
}
