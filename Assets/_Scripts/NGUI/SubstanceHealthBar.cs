using UnityEngine;
using System.Collections;

using Visiorama;

[RequireComponent(typeof (MeshRenderer))]
public class SubstanceHealthBar : MonoBehaviour, IHealthObserver
{
	public IHealthObservable Target { private set; get; }
	private int TargetTeamID;
	
	private ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;
	
	private MeshRenderer subMeshRenderer;

	void Awake ()
	{
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
		
		//Começando sem vida, dae depois altera pro valor correto
//		UpdateHealth (0);
	}
	
	void OnDestroy ()
	{
		Close ();
	}
	
	public void SetTarget (IHealthObservable target, int teamID)
	{
		Debug.LogWarning ("SetTarget");

		if (substance == null)
			Awake ();
		
		this.TargetTeamID = teamID;
		this.Target = target;
		this.Target.RegisterHealthObserver (this);
		//Forçando atualizaçao de vida atual
		this.Target.NotifyHealthChange ();
		
		subMeshRenderer.enabled = true;
	}
	
	public void Close ()
	{
		if (Target != null)
			Target.UnRegisterHealthObserver (this);
	}
	
#region IHealthObserver implementation
	
	public void UpdateHealth (int currentHealth)
	{		
		if (Target == null)
		{
			Debug.LogError ("Verifique se o metodo SetTarget foi chamado");
		}
		
		//so mostra o submesh da substance healthbar se tiver vida, se nao nao
//		subMeshRenderer.enabled = (currentHealth != 0);
		
		if (subMeshRenderer.enabled)
		{
			float percentHealth = (float)currentHealth / (float)Target.MaxHealth;
	
			//Monkey patch: Min = 0.5 Max = 1.0
			percentHealth = 0.5f + percentHealth * 0.5f;
	
			foreach (ProceduralPropertyDescription curProperty in curProperties)
			{
				if (curProperty.type == ProceduralPropertyType.Float)
				{
					substance.SetProceduralFloat(curProperty.name, percentHealth);
				}
				else
				{
					int teamID = this.TargetTeamID;
					Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
					if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
						substance.SetProceduralColor(curProperty.name, teamColor);
				}
			}
			substance.RebuildTextures ();
		}
	}
	
#endregion
}
