using UnityEngine;
using System.Collections;

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
				Destroy (gameObject);
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
			if (positionX)
			{
				position.x = (referenceObject.localPosition.x + offsetPosition.x);
			}
			else
			{
				position.x = transform.localPosition.x;
			}
			
			if (positionY)
			{
				position.y = (referenceObject.localPosition.y + offsetPosition.y);
			}
			else
			{
				position.y = transform.localPosition.y;
			}
			
			if (positionZ)
			{
				position.z = (referenceObject.localPosition.z + offsetPosition.z);
			}
			else
			{
				position.z = transform.localPosition.z;
			}
			transform.localPosition = position;
			
			// Rotation
			Vector3 rotation = Vector3.zero;
			if (rotationX)
			{
				rotation.x = (referenceObject.localEulerAngles.x + offsetRotation.x);
			}
			else
			{
				rotation.x = transform.localEulerAngles.x;
			}
			
			if (rotationY)
			{
				rotation.y = (referenceObject.localEulerAngles.y + offsetRotation.y);
			}
			else
			{
				rotation.y = transform.localEulerAngles.y;
			}
			
			if (rotationZ)
			{
				rotation.z = (referenceObject.localEulerAngles.z + offsetRotation.z);
			}
			else
			{
				rotation.z = transform.localEulerAngles.z;
			}
			transform.localEulerAngles = rotation;
			
		}
		else
		{
			// Position
			Vector3 position = Vector3.zero;
			if (positionX)
			{
				position.x = (referenceObject.position.x + offsetPosition.x);
			}
			else
			{
				position.x = transform.position.x;
			}
			
			if (positionY)
			{
				position.y = (referenceObject.position.y + offsetPosition.y);
			}
			else
			{
				position.y = transform.position.y;
			}
			
			if (positionZ)
			{
				position.z = (referenceObject.position.z + offsetPosition.z);
			}
			else
			{
				position.z = transform.position.z;
			}
			transform.position = position;
			
			// Rotation
			Vector3 rotation = Vector3.zero;
			if (rotationX)
			{
				rotation.x = (referenceObject.eulerAngles.x + offsetRotation.x);
			}
			else
			{
				rotation.x = transform.eulerAngles.x;
			}
			
			if (rotationY)
			{
				rotation.y = (referenceObject.eulerAngles.y + offsetRotation.y);
			}
			else
			{
				rotation.y = transform.eulerAngles.y;
			}
			
			if (rotationZ)
			{
				rotation.z = (referenceObject.eulerAngles.z + offsetRotation.z);
			}
			else
			{
				rotation.z = transform.eulerAngles.z;
			}
			transform.eulerAngles = rotation;
		}
		
		// Scale
		// Don't have a not local Scale
		Vector3 scale = Vector3.zero;
		if (scaleX)
		{
			scale.x = (referenceObject.localScale.x + offsetScale.x);
		}
		else
		{
			scale.x = transform.localScale.x;
		}
		
		if (scaleY)
		{
			scale.y = (referenceObject.localScale.y + offsetScale.y);
		}
		else
		{
			scale.y = transform.localScale.y;
		}
		
		if (scaleZ)
		{
			scale.z = (referenceObject.localScale.z + offsetScale.z);
		}
		else
		{
			scale.z = transform.localScale.z;
		}
		transform.localScale = scale;
	}
}
