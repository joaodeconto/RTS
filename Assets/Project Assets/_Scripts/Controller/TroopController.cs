using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TroopController : MonoBehaviour
{
	public GameObject healthBar;
	
	public bool keepFormation {get; set;}
	
	internal List<Unit> soldiers = new List<Unit> ();
	internal List<Unit> selectedSoldiers;
	
	protected bool enemySelected = false;
	protected Vector3 centerOfTroop;
		
	public void Init ()
	{
		selectedSoldiers = new List<Unit> ();
		//InvokeRepeating("OrganizeUnits",1.0f,1.0f);
	}
	
	public void MoveTroop (Vector3 destination)
	{
		if (enemySelected) return;
		
		if (keepFormation)
		{
			centerOfTroop = CenterOfObjects (selectedSoldiers.ToArray ());
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier != null)
				{
					soldier.TargetingEnemy (null);
					
					Vector3 t = centerOfTroop - soldier.transform.position;
					soldier.Move (destination - t);
				}
			}
		}
		else
		{
			foreach (Unit soldier in selectedSoldiers)
			{
				if (soldier != null)
				{
					soldier.TargetingEnemy (null);
					
//					Vector3 t = destination - soldier.transform.position;
					Vector3 newDestination = destination + (Random.insideUnitSphere * soldier.pathfind.radius * selectedSoldiers.Count);
					
					soldier.Move (newDestination);
				}
			}
		}
	}
	
	public void AttackTroop (GameObject enemy)
	{
		if (enemy == null) return;
		
		foreach (Unit soldier in selectedSoldiers)
		{
			soldier.TargetingEnemy (enemy);
		}
	}
	
	public void AddSoldier (Unit soldier)
	{
		soldiers.Add (soldier);
	}
	
	public void RemoveSoldier (Unit soldier)
	{
		if (selectedSoldiers.Contains (soldier))
		{
			selectedSoldiers.Remove (soldier);
		}
		soldiers.Remove (soldier);
	}
	
	public void SelectSoldier (Unit soldier, bool select)
	{
		if (select)
		{
			if (soldier.CompareTag ("Enemy")) enemySelected = true;
			else enemySelected = false;
			
			selectedSoldiers.Add (soldier);
			
			if (HUDRoot.go == null || healthBar == null)
			{
				return;
			}
	
			GameObject child = NGUITools.AddChild(HUDRoot.go, healthBar);
			
			AdjustSlider (child.GetComponent<UISlider> (), new Vector2(soldier.MaxHealth, 
				child.GetComponent<UISlider> ().fullSize.y));
			
			child.AddComponent<UIFollowTarget>().target = soldier.transform.FindChild ("Health Reference").transform;
			child.GetComponent<HealthBar> ().soldier = soldier;
			
			soldier.Active ();
		}
		else
		{
			selectedSoldiers.Remove (soldier);
			soldier.Deactive ();
		}
	}
	
	public void DeselectAllSoldiers ()
	{
		foreach (Unit soldier in selectedSoldiers)
		{
			if (soldier != null)
			{
				soldier.Deactive ();
			}
		}
		
		selectedSoldiers.Clear ();
		
		foreach (Transform child in HUDRoot.go.transform)
		{
			Destroy (child.gameObject);
		}
	}
	
	//TODO Só para testes
	void OnGUI ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Space (10f);
		GUI.color = Color.red;
		keepFormation = GUILayout.Toggle (keepFormation, "Keep Formation");
		GUILayout.EndHorizontal ();
	}
	
	void OrganizeUnits()
	{
		soldiers.Sort((unit1, unit2) =>
		{
			return unit1.transform.position.x.CompareTo(unit2.transform.position.x);
		});
	}
	
	// Códigos a adicionar no Framework
	
	Vector3 CenterOfObjects (GameObject[] objects)
	{
		int total = objects.Length;
		
		Vector3 position = Vector3.zero;
		foreach (GameObject obj in objects)
		{
			position += obj.transform.localPosition;
		}
		
		return position /= total;
	}
	
	Vector3 CenterOfObjects (MonoBehaviour[] objects)
	{
		int total = objects.Length;
		
		Vector3 position = Vector3.zero;
		foreach (MonoBehaviour obj in objects)
		{
			GameObject go = obj.gameObject;
			position += go.transform.localPosition;
		}
		
		return position /= total;
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