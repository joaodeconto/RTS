using UnityEngine;
using System.Collections;
using PathologicalGames;

public class ReferenceTransform : MonoBehaviour
{
	public Transform referenceObject;
	public bool local;
	public bool positionX, positionY, positionZ;
	public Vector3 offsetPosition;
	public bool rotationX, rotationY, rotationZ;
	public Vector3 offsetRotation;
	public bool scaleX, scaleY, scaleZ;
	public Vector3 offsetScale;
	
	public bool inUpdate;
	public bool destroyObjectWhenLoseReference;
	
	void Start ()
	{
		CalculeReference ();
		enabled = inUpdate;
	}
	
	void Update ()
	{
		CalculeReference ();
	}
	
	void CalculeReference ()
	{
		if (referenceObject == null)
		{
			if (destroyObjectWhenLoseReference)
			{
				PoolManager.Pools["Selection"].Despawn(transform);
			}
			else
			{
				return;
			}
		}
		
		if (local)
		{
			
			// Position
			Vector3 position = Vector3.zero;
			bool positionChange = false;
			if (positionX)
			{
				positionChange = true;
				position.x = (referenceObject.localPosition.x + offsetPosition.x);
			}
			
			if (positionY)
			{
				positionChange = true;
				position.y = (referenceObject.localPosition.y + offsetPosition.y);
			}
			
			if (positionZ)
			{
				positionChange = true;
				position.z = (referenceObject.localPosition.z + offsetPosition.z);
			}
			
			if (positionChange) transform.localPosition = position;
			
			// Rotation
			Vector3 rotation = Vector3.zero;
			bool rotationChange = false;
			if (rotationX)
			{
				rotationChange = true;
				rotation.x = (referenceObject.localEulerAngles.x + offsetRotation.x);
			}
			
			if (rotationY)
			{
				rotationChange = true;
				rotation.y = (referenceObject.localEulerAngles.y + offsetRotation.y);
			}
			
			if (rotationZ)
			{
				rotationChange = true;
				rotation.z = (referenceObject.localEulerAngles.z + offsetRotation.z);
			}
			
			if (rotationChange) transform.localEulerAngles = rotation;
			
		}
		else
		{
			// Position
			Vector3 position = Vector3.zero;
			bool positionChange = false;
			if (positionX)
			{
				positionChange = true;
				position.x = (referenceObject.position.x + offsetPosition.x);
			}
			
			if (positionY)
			{
				positionChange = true;
				position.y = (referenceObject.position.y + offsetPosition.y);
			}
			
			if (positionZ)
			{
				positionChange = true;
				position.z = (referenceObject.position.z + offsetPosition.z);
			}
			
			if (positionChange) transform.position = position;
			
			// Rotation
			Vector3 rotation = Vector3.zero;
			bool rotationChange = false;
			if (rotationX)
			{
				rotationChange = true;
				rotation.x = (referenceObject.eulerAngles.x + offsetRotation.x);
			}
			
			if (rotationY)
			{
				rotationChange = true;
				rotation.y = (referenceObject.eulerAngles.y + offsetRotation.y);
			}
			
			if (rotationZ)
			{
				rotationChange = true;
				rotation.z = (referenceObject.eulerAngles.z + offsetRotation.z);
			}
			
			if (rotationChange) transform.eulerAngles = rotation;
		}
		
		// Scale
		// Don't have a not local Scale
		Vector3 scale = Vector3.zero;
		bool scaleChange = false;
		if (scaleX)
		{
			scaleChange = true;
			scale.x = (referenceObject.localScale.x + offsetScale.x);
		}
		
		if (scaleY)
		{
			scaleChange = true;
			scale.y = (referenceObject.localScale.y + offsetScale.y);
		}
		
		if (scaleZ)
		{
			scaleChange = true;
			scale.z = (referenceObject.localScale.z + offsetScale.z);
		}
		if (scaleChange) transform.localScale = scale;
	}
}
