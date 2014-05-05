
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Visiorama;
using Visiorama.Utils;

public class RallyPoint : MonoBehaviour, IMovementObserver
{
	public Texture2D lineTexture;
	public float start = 1.0f; 
	public float end = 1.0f;
	public string[] AllowedLayersToFollow = new string[3] { "Terrain", "Unit","Resources" };//Layers de objetos que podem ser seguidos 

	public Transform subMesh;

	protected TouchController touchController;
	protected LineRenderer lineRenderer;
	protected HUDController hudController;
	protected GameplayManager gameplayManager;
	protected InteractionController interactionController;

	public Unit observedUnit { get; private set; }
	private IMovementObservable observed { get; set; }
	private Vector3 lastObservedPosition = Vector3.zero;

	private Vector3 lastSavedPosition;
	
	// Use this for initialization
	public void Init (Vector3 initialPosition, int team)
	{
		touchController = ComponentGetter.Get<TouchController> ();
		gameplayManager = ComponentGetter.Get<GameplayManager> ();
		hudController   = ComponentGetter.Get<HUDController> ();
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		//lineRenderer.SetColors (Color.white, Color.white);
		lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		lineRenderer.material.mainTexture = lineTexture;
		lineRenderer.material.color = ComponentGetter.Get<GameplayManager>().GetColorTeam ();
		lineRenderer.SetColors (lineRenderer.material.color, lineRenderer.material.color);
		lineRenderer.SetWidth (start, end);
		transform.GetChild (0).renderer.material.color = lineRenderer.material.color;

		ChangeColor (team);

		//Atualizar a primeira vez
		SavePosition (initialPosition);
		UpdatePosition (initialPosition);
	}
	
	private static Dictionary<string, ProceduralMaterial[]> rallypointMaterials = new Dictionary<string, ProceduralMaterial[]> ();	
	
	//Caso esse metodo for modificado eh necessario modificar no IStats tbm
	private void ChangeColor (int teamID)
	{
		Color teamColor  = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 0);
		Color teamColor1 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 1);
		Color teamColor2 = Visiorama.ComponentGetter.Get<GameplayManager>().GetColorTeam (teamID, 2);
		
		string keyRallypointMaterial = "rallypoint - " + teamID;
		
		//Inicializando unitTeamMaterials com materiais compartilhado entre as unidades iguais de cada time
		if (!rallypointMaterials.ContainsKey (keyRallypointMaterial))
		{
			int nMaterials = subMesh.renderer.materials.Length;
			rallypointMaterials.Add (keyRallypointMaterial, new ProceduralMaterial[nMaterials]);
			
			for (int i = 0, iMax = subMesh.renderer.materials.Length; i != iMax; ++i)
			{
				ProceduralMaterial substance 				  = subMesh.renderer.materials[i] as ProceduralMaterial;
				ProceduralPropertyDescription[] curProperties = substance.GetProceduralPropertyDescriptions();
				
				//Setando os valores corretos de cor
				foreach (ProceduralPropertyDescription curProperty in curProperties)
				{
					if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor"))
						substance.SetProceduralColor(curProperty.name, teamColor);
					if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor1"))
						substance.SetProceduralColor(curProperty.name, teamColor1);
					if (curProperty.type == ProceduralPropertyType.Color4 && curProperty.name.Equals ("outputcolor2"))
						substance.SetProceduralColor(curProperty.name, teamColor2);
				}
				
				substance.RebuildTextures ();
				
				rallypointMaterials[keyRallypointMaterial][i] = substance;
			}
		}
		
		//Associando na unidade os materiais corretos
		ProceduralMaterial[] pms = rallypointMaterials[keyRallypointMaterial];

		subMesh.renderer.sharedMaterials = pms as Material[];
	}
	
	// Update is called once per frame
	void Update ()
	{
		lineRenderer.enabled = enabled;

		#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
		if (touchController.touchType == TouchController.TouchType.First)
		{
			if (touchController.idTouch == TouchController.IdTouch.Id1)
			{
				UpdateRallyPoint ();
			}
		}
		#else
		if (touchController.touchType == TouchController.TouchType.Ended)
		{
			if (!touchController.DragOn)
			{
				if (touchController.idTouch == TouchController.IdTouch.Id0)
				{
					UpdateRallyPoint ();
				}
			}
		}
		#endif
	}
	
	public void UpdateRallyPoint ()
	{
		Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		int layerMask = 0;

		foreach (string layerName in AllowedLayersToFollow)
		{
			layerMask = layerMask | LayerMask.NameToLayer(layerName);
		}

		if (Physics.Raycast (ray, out hit))
		{
			GameObject goHit = hit.transform.gameObject;

			//Testar se a layer do hit esta disponivel para ser usada como referencia para o rallypoint
			if ((goHit.layer & layerMask) != 0)
			{
				UpdatePosition (hit.point);

				if (goHit.name == "Resource")
				{
					Resource resourceStats = goHit.GetComponent<Resource>();
					hudController.CreateSubstanceResourceBar (resourceStats, resourceStats.sizeOfSelectedHealthBar, resourceStats.maxResources);
					hudController.CreateFeedback (HUDController.Feedbacks.Move,hit.transform.localPosition,
					                              1f,
					                              gameplayManager.GetColorTeam ());
				}

				if (observed != null)
				{
					observed.UnRegisterMovementObserver (this);
					observed = null;
				}

				Unit unit = goHit.GetComponent<Unit> ();



				if (unit == null)
				{
					SavePosition (hit.point);
				}
				else
				{
					GameplayManager gm = ComponentGetter.Get<GameplayManager> ();
					
					if (gm.IsSameTeam (unit) || gm.IsAlly (unit))
					{
						MonoBehaviour[] scripts = goHit.GetComponents<MonoBehaviour> ();
						
						foreach (MonoBehaviour script in scripts)
						{
							IMovementObservable o = script as IMovementObservable;
							Unit u 				  = script as Unit;
							
							if (o != null && u != null)
							{
								observedUnit = u;
								observed     = o;
								observed.RegisterMovementObserver (this);
								break;
							}
						}
					}
				}
			}
		}
		
		CalculateLine ();
	}



	#region IMovementObserver implementation

	public void UpdatePosition (Vector3 newPosition, GameObject go = null)
	{
		lastObservedPosition = transform.position;
		transform.position   = newPosition;

		CalculateLine ();
	}

	public Vector3 LastPosition {
		get {
			return lastObservedPosition;
		}
	}

	public void OnUnRegisterMovementObserver ()
	{
		UpdatePosition (lastSavedPosition);
	}

	#endregion

	void SavePosition (Vector3 position)
	{
		lastSavedPosition = position;
	}

	void CalculateLine ()
	{
		List<Vector3> nodes = new List<Vector3>();
		nodes.Add(transform.position);
		nodes.Add(transform.parent.position);
		
//		Vector3 center = Math.CenterOfObjects (nodes.ToArray ());
//		nodes.Insert (1, center);
		
		IEnumerable<Vector3> sequence = Interpolate.NewCatmullRom (nodes.ToArray(), 5, false);
		
		int i = 0;
		foreach (Vector3 segment in sequence)
		{
			lineRenderer.SetVertexCount (i+1);
			lineRenderer.SetPosition(i, segment);
			i++;
		}
	}
}
