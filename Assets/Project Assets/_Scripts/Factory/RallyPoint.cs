using UnityEngine;
using System.Collections;
using Visiorama;

public class RallyPoint : MonoBehaviour {
	
	public Texture2D lineTexture;
	
	protected TouchController touchController;
	protected LineRenderer lineRenderer;
	
	// Use this for initialization
	void Awake ()
	{
		touchController = ComponentGetter.Get<TouchController> ();
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetColors (Color.black, Color.black);
		lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		lineRenderer.material.mainTexture = lineTexture;
		lineRenderer.material.color = ComponentGetter.Get<GameplayManager>().GetColorTeam ();
		lineRenderer.SetColors (lineRenderer.material.color, lineRenderer.material.color);
		transform.GetChild (0).renderer.material.color = lineRenderer.material.color;
		active = false;
		
		lineRenderer.SetPosition (0, transform.position);
		lineRenderer.SetPosition (1, transform.parent.position);
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
//		if (touchController.touchType == TouchController.TouchType.First)
//		{
//			if (touchController.idTouch == TouchController.IdTouch.Id0)
//			{
//				UpdateRallyPoint ();
//			}
//		}
#endif
	}
	
	public void UpdateRallyPoint ()
	{
		Ray ray = touchController.mainCamera.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		int layerMask = 1 << LayerMask.NameToLayer("Terrain");
		
		// Patch transform com hit.point
		transform.position = Vector3.zero;
		
		if (Physics.Raycast (ray, out hit, layerMask))
		{
			transform.position = hit.point;
		}
		
		lineRenderer.SetPosition (0, transform.position);
		lineRenderer.SetPosition (1, transform.parent.position);
	}
}
