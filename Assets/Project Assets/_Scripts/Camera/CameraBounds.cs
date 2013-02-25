using UnityEngine;
using System.Collections;

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
	
	private Vector3 positionInitial;
	private bool wasInitialized;
	
	void Start () {
		// Pegando posição inicial
		positionInitial = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		
		// Calcular posicao da camera inicial
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, scenario.x.min, scenario.x.max), 
		                                 height, 
		                                 Mathf.Clamp(transform.position.z, scenario.z.min, scenario.z.max));
		
		wasInitialized = true;
	}
	
	// Update is called once per frame
	void Update () {
		// Cálculo em torno da área que está no cubo ciano
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, scenario.x.min, scenario.x.max), 
		                                 height, 
		                                 Mathf.Clamp(transform.position.z, scenario.z.min, scenario.z.max));
	}
	
	void OnDrawGizmosSelected ()
	{
		Vector3 centerPosition;
		if (!wasInitialized)
		{
			centerPosition = transform.position;
		}
		else
		{
			centerPosition = positionInitial;
		}
		
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
