using UnityEngine;
using System.Collections;

public class HUDController : MonoBehaviour {
	
	public GameObject healthBar;
	
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
