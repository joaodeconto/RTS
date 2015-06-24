using UnityEngine;
using System.Collections;
using Visiorama.Utils;

public class CameraBounds : MonoBehaviour
{
	// Editor
	public Vector3MinMax scenario;
	private float fieldOfViewInitial;
	private Vector3 boundsArea;
	public float zoomModifier = 0;
	
	void Start ()
	{
		fieldOfViewInitial = camera.GetComponent<CameraMovement>().zoom.max;	
		boundsArea = new Vector3((Mathf.Abs(scenario.x.min)+scenario.x.max)/3,60,(Mathf.Abs(scenario.z.min)+scenario.z.max)/3);
	}

	void Update () {

		transform.position = ClampScenario (transform.position);
	}
	
	public Vector3 ClampScenario (Vector3 position)
	{

		zoomModifier = 1f-(camera.fieldOfView/fieldOfViewInitial);

		return new Vector3 (Mathf.Clamp (position.x, scenario.x.min -(boundsArea.x*zoomModifier), scenario.x.max +(boundsArea.x*zoomModifier)), 
		                    Mathf.Clamp (position.y, scenario.y.min, scenario.y.max), 
		                    Mathf.Clamp (position.z, scenario.z.min -(boundsArea.z*zoomModifier), scenario.z.max + (boundsArea.z*zoomModifier)));		
	}
	
	void OnDrawGizmosSelected ()
	{
		// Colocando cor "cyan" em gizmo
		Gizmos.color = Color.cyan;
		
		// Desenhando um WireCube do scenario
		// (Cubo feito na mão pois o DrawWireCube ele escala só um eixo e não um mínimo e máximo)
		if (Application.isPlaying)
		{
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.min * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max * (camera.fieldOfView / fieldOfViewInitial)));
			Gizmos.DrawLine((Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)), (Vector3.right * scenario.x.max * (camera.fieldOfView / fieldOfViewInitial)) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min * (camera.fieldOfView / fieldOfViewInitial)));
		}
		else
		{
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max), (Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.min) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min));
			Gizmos.DrawLine((Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.max), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.max));
			Gizmos.DrawLine((Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.min) + (Vector3.forward * scenario.z.min), (Vector3.right * scenario.x.max) + (Vector3.up * scenario.y.max) + (Vector3.forward * scenario.z.min));
		}
	}
}
