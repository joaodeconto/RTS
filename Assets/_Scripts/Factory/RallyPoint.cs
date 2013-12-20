
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Visiorama;
using Visiorama.Utils;

public class RallyPoint : MonoBehaviour, IMovementObserver {
	
	public Texture2D lineTexture;
	public float start = 1.0f; 
	public float end = 1.0f;
	public string[] AllowedLayersToFollow = new string[2] { "Terrain", "Unit" };//Layers de objetos que podem ser seguidos 

	protected TouchController touchController;
	protected LineRenderer lineRenderer;

	public IMovementObservable observedUnit { get; private set; }
	private Vector3 lastObservedPosition = Vector3.zero;

	
	// Use this for initialization
	void Start ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetColors (Color.black, Color.black);
		lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		lineRenderer.material.mainTexture = lineTexture;
		lineRenderer.material.color = ComponentGetter.Get<GameplayManager>().GetColorTeam ();
		lineRenderer.SetColors (lineRenderer.material.color, lineRenderer.material.color);
		lineRenderer.SetWidth (start, end);
		transform.GetChild (0).renderer.material.color = lineRenderer.material.color;

		//Atualizar a primeira vez
		UpdatePosition (this.transform.position);
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

//			Debug.Log ("hit: " + LayerMask.LayerToName(goHit.layer) + " - " + goHit.name);

			if ((goHit.layer & layerMask) != 0)
			{
				MonoBehaviour[] scripts = goHit.GetComponents<MonoBehaviour> ();

				foreach (MonoBehaviour script in scripts)
				{
					IMovementObservable observable = script as IMovementObservable;

					if (observable != null)
					{
						observedUnit = observable;
						observedUnit.RegisterMovementObserver (this);
						break;
					}
				}
				
				UpdatePosition (hit.point);
			}
		}
		
		CalculateLine ();
	}

	#region IMovementObserver implementation

	public void UpdatePosition (Vector3 newPosition)
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

	public void OnUnRegisterObserver ()
	{
		transform.position = lastObservedPosition;
	}

	#endregion

	void CalculateLine ()
	{
		List<Vector3> nodes = new List<Vector3>();
		nodes.Add(transform.position);
		nodes.Add(transform.parent.position);
		
//		Vector3 center = Math.CenterOfObjects (nodes.ToArray ());
//		nodes.Insert (1, center);
		
		IEnumerable<Vector3> sequence = Interpolate.NewCatmullRom (nodes.ToArray(), 10, false);
		
		int i = 0;
		foreach (Vector3 segment in sequence)
		{
			lineRenderer.SetVertexCount (i+1);
			lineRenderer.SetPosition(i, segment);
			i++;
		}
	}
}
