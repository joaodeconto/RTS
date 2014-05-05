using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

[RequireComponent(typeof (MeshRenderer))]
public class SubstanceResourceBar : MonoBehaviour
{
			
	private ProceduralMaterial substance;
	private ProceduralPropertyDescription[] curProperties;
	private Resource refResource;
	private int actualResources; 
	private Transform refTransform;


	private MeshRenderer subMeshRenderer;


	void Awake ()
	{
		subMeshRenderer = GetComponent <MeshRenderer> ();
		subMeshRenderer.enabled = true;
		
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

	void Update ()
	{
		refTransform = GetComponent<ReferenceTransform>().referenceObject;
		refResource = refTransform.GetComponent<Resource>();
		UpdateResource (refResource.numberOfResources);


	}
	
	public void UpdateResource (int actualResource)
	{
		int maxResources = refResource.maxResources;

		float percentResource = (float)actualResource / maxResources;
		
		//Monkey patch: Min = 0.5 Max = 1.0
		percentResource = 0.5f + percentResource * 0.5f;
		
		foreach (ProceduralPropertyDescription curProperty in curProperties)
		{
			if (curProperty.type == ProceduralPropertyType.Float)
			{
				substance.SetProceduralFloat(curProperty.name, percentResource);
			}

			else
			{

				Color ResourceColor  = Color.yellow;
				if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
					substance.SetProceduralColor(curProperty.name, ResourceColor);
			}

		}

		substance.RebuildTextures ();
	}

	

}
