using UnityEngine;
using System.Collections;

public class HUDController : MonoBehaviour {
	
	public GameObject healthBar;
	public GameObject selectedObject;
	public Transform mainTranformSelectedObjects;
	
	public HealthBar CreateHealthBar (Transform target, int maxHealth, string referenceChild)
	{
		if (HUDRoot.go == null || healthBar == null)
		{
			return null;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, healthBar);
		
		if (child.GetComponent<HealthBar> ()) child.AddComponent <HealthBar> ();
		if (child.GetComponent<UISlider> ()) child.AddComponent <UISlider> ();
		
		AdjustSlider (child.GetComponent<UISlider> (), new Vector2(maxHealth, 
			child.GetComponent<UISlider> ().fullSize.y));
		
		child.AddComponent<UIFollowTarget>().target = target.FindChild (referenceChild).transform;
		
		return child.GetComponent<HealthBar> ();
	}
	
	public void CreateSelected (Transform target, float size)
	{
		GameObject selectObj = Instantiate (selectedObject, target.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 0.4f, 0.1f, size * 0.4f);
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.2f;
		
		selectObj.transform.parent = mainTranformSelectedObjects;
	}
	
	public void DestroySelected (Transform target)
	{
		foreach (Transform child in mainTranformSelectedObjects)
		{
			if (child.GetComponent<ReferenceTransform>().referenceObject == target)
			{
				DestroyObject (child.gameObject);
			}
		}
	}
	
	// Add Códigos na Framework
	
	void AdjustSlider (UISlider slider, Vector2 newSize)
	{
		slider.fullSize = newSize;
		
		Transform background = slider.transform.Find("Background");
		background.localScale = new Vector3(newSize.x, newSize.y, 1f);
		
		Vector3 newPosition = new Vector3(-newSize.x/2, background.localPosition.y, background.localPosition.z);
		
		background.localPosition = newPosition;
		slider.foreground.localPosition = newPosition;
	}
}
