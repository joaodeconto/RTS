using UnityEngine;
using System.Collections;
using Visiorama;

public class HUDController : MonoBehaviour {
	
	public GameObject healthBar;
	public GameObject selectedObject;
	public Transform mainTranformSelectedObjects;
	public Transform transformMenu;
	public GameObject button;
	
	public HealthBar CreateHealthBar (Transform target, int maxHealth, string referenceChild)
	{
		if (HUDRoot.go == null || healthBar == null)
		{
			return null;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, healthBar);
		
		if (child.GetComponent<HealthBar> () == null) child.AddComponent <HealthBar> ();
		if (child.GetComponent<UISlider> () == null) child.AddComponent <UISlider> ();
		
		NGUIUtils.AdjustSlider (child.GetComponent<UISlider> (), new Vector2(maxHealth, 
			child.GetComponent<UISlider> ().fullSize.y), "Background");
		
		child.AddComponent<UIFollowTarget>().target = target.FindChild (referenceChild).transform;
		
		return child.GetComponent<HealthBar> ();
	}
	
	public void CreateSelected (Transform target, float size, Color color)
	{
		GameObject selectObj = Instantiate (selectedObject, target.position, Quaternion.identity) as GameObject;
		selectObj.transform.localScale = new Vector3(size * 0.3f, 0.1f, size * 0.3f);
		selectObj.AddComponent<ReferenceTransform>().inUpdate = true;
		ReferenceTransform refTransform = selectObj.GetComponent<ReferenceTransform> ();
		refTransform.referenceObject = target;
		refTransform.positionX = true;
		refTransform.positionY = true;
		refTransform.positionZ = true;
		refTransform.destroyObjectWhenLoseReference = true;
		refTransform.offsetPosition += Vector3.up * 0.4f;
		
		selectObj.renderer.sharedMaterial.color = color;
		
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
	
	public void CreateButtonInInspector (string buttonName, Vector3 position, Unit unit, FactoryBase factory)
	{
		GameObject newButton = NGUITools.AddChild (transformMenu.gameObject, button);
//		newButton.
		newButton.transform.localPosition = position;
		
		UnitCallbackButton ucb = newButton.AddComponent<UnitCallbackButton> ();
		ucb.Init (unit, factory);
	}
	
	public void DestroyInspector ()
	{
		foreach (Transform child in transformMenu)
		{
			DestroyObject (child.gameObject);
		}
	}
}
