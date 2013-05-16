using UnityEngine;
using System.Collections;
using Visiorama.Utils;

public class CameraBounds : MonoBehaviour {
	
	// Classe Serializada de Vector3 com MinMaxFloat
	[System.Serializable]
	public class Vector3MinMax
	{
		public MinMaxFloat x, y, z;
	}
	
	// Editor
	public Vector3MinMax scenario;
	public float height = 15f;
	public float relativeSize = 15f;
	
	private Vector3 positionInitial;
	private bool wasInitialized;
	
	void Start () {
		// Pegando posição inicial
		positionInitial = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		
		wasInitialized = true;
	}
	
	// Update is called once per frame
	void Update () {
		// Cálculo em torno da área que está no cubo ciano
		transform.position = ClampScenario (transform.position);
	}
	
	public Vector3 ClampScenario (Vector3 position)
	{
//		Debug.Log (relativeSize/camera.orthographicSize);
//		return new Vector3 (Mathf.Clamp (position.x, scenario.x.min + (relativeSize/camera.orthographicSize), scenario.x.max + (relativeSize/camera.orthographicSize)), 
//		                    height, 
//		                    Mathf.Clamp (position.z, scenario.z.min + (relativeSize/camera.orthographicSize), scenario.z.max + (relativeSize/camera.orthographicSize)));
		return new Vector3 (Mathf.Clamp (position.x, scenario.x.min, scenario.x.max), 
		                    height, 
		                    Mathf.Clamp (position.z, scenario.z.min, scenario.z.max));
		
	}
	
	void OnDrawGizmosSelected ()
	{
		// Colocando cor "cyan" em gizmo
		Gizmos.color = Color.cyan;
		
		// Desenhando um WireCube do scenario
		// (Cubo feito na mão pois o DrawWireCube ele escala só um eixo e não um mínimo e máximo)
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
