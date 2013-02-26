using UnityEngine;
using System.Collections;

public class SelectionController : MonoBehaviour
{
	protected TouchController touchController;
	protected TroopController troopController;
	protected FactoryController factoryController;
	protected GameplayManager gameplayManager;
	
	protected Vector3 windowSize = Vector3.zero; 
	
	public void Init ()
	{
		touchController = GameController.GetInstance().GetTouchController();
		troopController = GameController.GetInstance().GetTroopController();
		factoryController = GameController.GetInstance().GetFactoryController();
		gameplayManager = GameController.GetInstance().GetGameplayManager();
	}
	
	void Update ()
	{
		if (touchController.touchType == TouchController.TouchType.Ended)
		{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
			if (!touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id1)
			{
				troopController.DeselectAllSoldiers ();
				return;
			}
			else
#endif
			if (touchController.idTouch == TouchController.IdTouch.Id0)
			{
				factoryController.DeselectFactory ();
				
				if (touchController.DragOn)
				{
					windowSize.x = touchController.GetFirstPoint.x-touchController.GetFinalPoint.x > 0f ? 
						touchController.GetFirstPoint.x-touchController.GetFinalPoint.x : touchController.GetFinalPoint.x-touchController.GetFirstPoint.x;
					windowSize.y = touchController.GetFirstPoint.y-touchController.GetFinalPoint.y > 0f ? 
						touchController.GetFirstPoint.y-touchController.GetFinalPoint.y : touchController.GetFinalPoint.y-touchController.GetFirstPoint.y;
					windowSize.z = touchController.GetFirstPoint.z-touchController.GetFinalPoint.z > 0f ? 
						touchController.GetFirstPoint.z-touchController.GetFinalPoint.z : touchController.GetFinalPoint.z-touchController.GetFirstPoint.z;
	
					Bounds b = new Bounds((touchController.GetFirstPoint+touchController.GetFinalPoint)/2, windowSize + (Vector3.up * 100f) );
					
					DebugDrawCube (b, Color.green);
					
					troopController.DeselectAllSoldiers ();
					
#if !UNITY_IPHONE && !UNITY_ANDROID || UNITY_EDITOR
					Unit enemySoldier = null;
#endif
					foreach (Unit soldier in troopController.soldiers)
					{
						if (!gameplayManager.IsSameTeam (soldier))
						{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
							continue;
#else
							if (troopController.selectedSoldiers.Count != 0 ||
								enemySoldier != null) continue;
#endif
						}
						
						if (soldier.collider != null)
						{
							if (b.Intersects (soldier.collider.bounds))
							{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
								troopController.SelectSoldier (soldier, true);
#else
								if (!gameplayManager.IsSameTeam (soldier))
								{
									enemySoldier = soldier;
									continue;
								}
								else
								{
									troopController.SelectSoldier (soldier, true);
								}
#endif
							}
							else
							{
								troopController.SelectSoldier (soldier, false);
							}
						}
						else
						{
							if (AABBContains (soldier.transform.localPosition, b, IgnoreVector.Y))
							{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
								troopController.SelectSoldier (soldier, true);
#else
								if (!gameplayManager.IsSameTeam (soldier))
								{
									enemySoldier = soldier;
									continue;
								}
								else
								{
									troopController.SelectSoldier (soldier, true);
								}
#endif
							}
							else
							{
								troopController.SelectSoldier (soldier, false);
							}
						}
					}
					
					if (troopController.selectedSoldiers.Count != 0) return;
#if !UNITY_IPHONE && !UNITY_ANDROID || UNITY_EDITOR
					else if (enemySoldier != null) troopController.SelectSoldier (enemySoldier, true);
#endif
					
					foreach (FactoryBase factory in factoryController.factorys)
					{
						if (factory.collider != null)
						{
							if (b.Intersects (factory.collider.bounds))
							{
								factoryController.SelectFactory (factory);
							}
						}
						else
						{
							if (AABBContains (factory.transform.localPosition, b, IgnoreVector.Y))
							{
								factoryController.SelectFactory (factory);
							}
						}
					}
				}
				else
				{
					RaycastHit hit;
					
					if (Physics.Raycast (touchController.GetFinalRaycast, out hit))
					{
						if (hit.transform.CompareTag ("Unit"))
						{
							if (gameplayManager.IsSameTeam (hit.transform.GetComponent <Unit>()))
							{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR							
								troopController.DeselectAllSoldiers ();
#else
								if (! Input.GetKey (KeyCode.LeftShift))
								{
									troopController.DeselectAllSoldiers ();
								}
#endif							
								if (!troopController.selectedSoldiers.Contains (hit.transform.GetComponent<Unit> ()))
								{
									troopController.SelectSoldier (hit.transform.GetComponent<Unit> (), true);
								}
								else
								{
									troopController.SelectSoldier (hit.transform.GetComponent<Unit> (), false);
								}
								return;
							}
							else
							{
								troopController.DeselectAllSoldiers ();
								
								if (!troopController.selectedSoldiers.Contains (hit.transform.GetComponent<Unit> ()))
								{
									troopController.SelectSoldier (hit.transform.GetComponent<Unit> (), true);
								}
								else
								{
									troopController.SelectSoldier (hit.transform.GetComponent<Unit> (), false);
								}
								return;
							}
						}
						
						if (hit.transform.CompareTag ("Factory"))
						{
							if (gameplayManager.IsSameTeam (hit.transform.GetComponent<FactoryBase> ()))
							{
								troopController.DeselectAllSoldiers ();
								
								factoryController.SelectFactory (hit.transform.GetComponent<FactoryBase>());
							}
							return;
						}
						
#if !UNITY_IPHONE && !UNITY_ANDROID || UNITY_EDITOR						
						troopController.DeselectAllSoldiers ();
#endif
					}
					else
					{
#if !UNITY_IPHONE && !UNITY_ANDROID || UNITY_EDITOR						
						troopController.DeselectAllSoldiers ();
#endif
					}
					
				}
			}
		}
	}

	void OnGUI ()
	{
		if (touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id0)
		{
			GUI.Box (new Rect(touchController.FirstPosition.x, touchController.FirstPosition.y, 
				touchController.CurrentPosition.x - touchController.FirstPosition.x, touchController.CurrentPosition.y - touchController.FirstPosition.y), "");
		}
	}
	
	// Códigos a adicionar no Framework
	
	void DebugDrawCube (Bounds bound, Color color)
	{
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), (Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.min.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.min.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), color);
		Debug.DrawLine((Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.max.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.max.z), color);
		Debug.DrawLine((Vector3.right * bound.max.x) + (Vector3.up * bound.min.y) + (Vector3.forward * bound.min.z), (Vector3.right * bound.max.x) + (Vector3.up * bound.max.y) + (Vector3.forward * bound.min.z), color);
	}
	
	public enum IgnoreVector
	{
		X, Y, Z, None
	}
	
	bool AABBContains (Vector3 position, Bounds bounds, IgnoreVector ignoreVector)
	{
		Vector3 _tempVec;
		
		_tempVec = bounds.min;
		
		if (ignoreVector == IgnoreVector.X)
		{
			if (position.y < _tempVec.y || position.z < _tempVec.z)
				return false;
		}
		else if (ignoreVector == IgnoreVector.Y)
		{
			if (position.x < _tempVec.x || position.z < _tempVec.z)
				return false;
		}
		else if (ignoreVector == IgnoreVector.Z)
		{
			if (position.x < _tempVec.x || position.y < _tempVec.y)
				return false;
		}
		else
		{
			if (position.x < _tempVec.x || position.y < _tempVec.y || position.z < _tempVec.z)
				return false;
		}

		_tempVec = bounds.max;
		
		if (ignoreVector == IgnoreVector.X)
		{
			if (position.y > _tempVec.y || position.z > _tempVec.z)
				return false;
		}
		else if (ignoreVector == IgnoreVector.Y)
		{
			if (position.x > _tempVec.x || position.z > _tempVec.z)
				return false;
		}
		else if (ignoreVector == IgnoreVector.Z)
		{
			if (position.x > _tempVec.x || position.y > _tempVec.y)
				return false;
		}
		else
		{
			if (position.x > _tempVec.x || position.y > _tempVec.y || position.z > _tempVec.z)
				return false;
		}

		return true;
	}
}
