using UnityEngine;
using System.Collections;

public class HelperColliderDetect : MonoBehaviour {
	
	public delegate void OnTriggerDelegate (Collider other);
	
	protected OnTriggerDelegate onTriggerEnterDelegate;
	protected OnTriggerDelegate onTriggerExitDelegate;
	
	public void Init (OnTriggerDelegate onTriggerEnter,
					  OnTriggerDelegate onTriggerExit)
	{
		onTriggerEnterDelegate = onTriggerEnter;
		onTriggerExitDelegate = onTriggerExit;
	}
	
	void OnTriggerEnter (Collider other)
	{
		onTriggerEnterDelegate (other);
	}
	
	void OnTriggerExit (Collider other)
	{
		onTriggerExitDelegate (other);
	}
}
