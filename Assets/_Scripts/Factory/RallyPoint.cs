using UnityEngine;
using System.Collections;
using Visiorama;
using System.Collections.Generic;
using Visiorama.Utils;

public class RallyPoint : MonoBehaviour {
	
	public Texture2D lineTexture;
	
	protected TouchController touchController;
	protected LineRenderer lineRenderer;
	
	// Use this for initialization
	void Awake ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetColors (Color.white, Color.black);
		lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		lineRenderer.material.mainTexture = lineTexture;
		lineRenderer.material.color = ComponentGetter.Get<GameplayManager>().GetColorTeam ();
		lineRenderer.SetColors (lineRenderer.material.color, lineRenderer.material.color);
		transform.GetChild (0).renderer.material.color = lineRenderer.material.color;
		
		CalculateLine ();
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
		
		int layerMask = LayerMask.NameToLayer("Terrain");
		
		// Patch transform com hit.point
		Vector3 oldPosition = transform.position;
		transform.position = Vector3.zero;
		
		if (Physics.Raycast (ray, out hit))
		{
			if (hit.transform.gameObject.layer == layerMask)
				transform.position = hit.point;
			else
				transform.position = oldPosition;
		}
		else
		{
			transform.position = oldPosition;
		}
		
		CalculateLine ();
	}
	
	void CalculateLine ()
	{
		List<Vector3> nodes = new List<Vector3>();
		nodes.Add(transform.position);
		nodes.Add(transform.parent.position);
		
		Vector3 center = Math.CenterOfObjects (nodes.ToArray ());
		nodes.Insert (1, center + (Vector3.up * 1f));
	
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
