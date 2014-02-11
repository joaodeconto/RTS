using UnityEngine;
using System.Collections;

//[RequireComponent(typeof (MeshRenderer))]
public class SubstanceHealthBar : MonoBehaviour, IHealthObserver
{
	public IHealthObservable Target { private set; get; }

	public ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;

	void Awake ()
	{
		MeshRenderer subMeshRenderer = GetComponent <MeshRenderer> ();
		//		Material mMaterial = new Material (subMeshRenderer.sharedMaterial);
		//		substance 	  = subMeshRenderer.sharedMaterial as ProceduralMaterial;
//		ProceduralMaterial mmMaterial = new ProceduralMaterial ();
//		mmMaterial.CopyPropertiesFromMaterial (subMeshRenderer.sharedMaterial as ProceduralMaterial);

		ProceduralMaterial mMaterial = subMeshRenderer.material as ProceduralMaterial;

		subMeshRenderer.sharedMaterial = mMaterial;
		substance 	  = mMaterial;
		curProperties = substance.GetProceduralPropertyDescriptions();
	}
	
	void OnDestroy ()
	{
		Close ();
	}
	
	public void SetTarget (IHealthObservable target)
	{
		Debug.LogWarning ("SetTarget");

		if (substance == null)
			Awake ();
		
		this.Target = target;
		this.Target.RegisterHealthObserver (this);
		//Forçando atualizaçao de vida atual
		this.Target.NotifyHealthChange ();
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
			Debug.LogError ("Verifique se o metodo SetTarget foi chamado");
		
		float percentHealth = (float)currentHealth / (float)Target.MaxHealth;

		//Monkey patch: Min = 0.5 Max = 1.0
		percentHealth = 0.5f + percentHealth * 0.5f;

		foreach (ProceduralPropertyDescription curProperty in curProperties)
		{
//			Debug.Log ("curProperty: " + curProperty.name + " - " + curProperty.type);

			if (curProperty.type == ProceduralPropertyType.Float)
			{
//				Debug.Log ("curProperty.name: " + curProperty.name + " - " + percentHealth);
				substance.SetProceduralFloat(curProperty.name, percentHealth);
			}
		}
		substance.RebuildTextures ();
	}
	
#endregion
}
