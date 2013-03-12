using UnityEngine;
using System.Collections;
using Visiorama;
using Visiorama.Utils;

public class SelectionController : MonoBehaviour
{
	protected TouchController touchController;
	protected TroopController troopController;
	protected FactoryController factoryController;
	protected GameplayManager gameplayManager;

	protected Vector3 windowSize = Vector3.zero;

	public void Init ()
	{
		touchController   = ComponentGetter.Get<TouchController>();
		troopController   = ComponentGetter.Get<TroopController>();
		factoryController = ComponentGetter.Get<FactoryController>();
		gameplayManager   = ComponentGetter.Get<GameplayManager>();
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

					Bounds b = new Bounds((touchController.GetFirstPoint+touchController.GetFinalPoint)/2, windowSize + (Vector3.up * 999999f) );
					
//					VDebug.DrawCube (b, Color.green);
					
//					Plane[] PlaneBuffer = CalculateRect (new Rect(Mathf.Min(touchController.FirstPosition.x, touchController.FinalPosition.x),
//						Mathf.Min(touchController.FirstPosition.y, touchController.FinalPosition.y),
//						Mathf.Abs(touchController.FirstPosition.x - touchController.FinalPosition.x), 
//						Mathf.Abs(touchController.FirstPosition.y - touchController.FinalPosition.y)));
					
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
//							if (GeometryUtility.TestPlanesAABB (PlaneBuffer, soldier.collider.bounds))
//							{
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
							if (Math.AABBContains (soldier.transform.localPosition, b, Math.IgnoreVector.Y))
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
//							if (GeometryUtility.TestPlanesAABB (PlaneBuffer, factory.collider.bounds))
//							{
								factoryController.SelectFactory (factory);
								break;
							}
						}
						else
						{
							if (Math.AABBContains (factory.transform.localPosition, b, Math.IgnoreVector.Y))
							{
								factoryController.SelectFactory (factory);
								break;
							}
						}
					}
				}
				else
				{
					RaycastHit hit;

					if (Physics.Raycast (touchController.GetFinalRay, out hit))
					{
						if (hit.transform.CompareTag ("Unit"))
						{
							if (gameplayManager.IsSameTeam (hit.transform.GetComponent <Unit>()))
							{
#if UNITY_IPHONE || UNITY_ANDROID && !UNITY_EDITOR
								troopController.DeselectAllSoldiers ();
#else
								if (Input.GetKey (KeyCode.LeftControl))
								{
									troopController.DeselectAllSoldiers ();
									
									int category = hit.transform.GetComponent<Unit> ().Category;
									foreach (Unit soldier in troopController.soldiers)
									{
										if (gameplayManager.IsSameTeam (soldier.Team))
										{
											if (soldier.Category == category)
											{
												troopController.SelectSoldier (soldier, true);
											}
										}
									}
									return;
								}
								else
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
							troopController.DeselectAllSoldiers ();

							factoryController.SelectFactory (hit.transform.GetComponent<FactoryBase>());
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
#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.Keypad0))
		{
			troopController.CreateGroup (0);
		}
		if (Input.GetKeyDown (KeyCode.Keypad1))
		{
			troopController.CreateGroup (1);
		}
		if (Input.GetKeyDown (KeyCode.Keypad2))
		{
			troopController.CreateGroup (2);
		}
		if (Input.GetKeyDown (KeyCode.Keypad3))
		{
			troopController.CreateGroup (3);
		}
		if (Input.GetKeyDown (KeyCode.Keypad4))
		{
			troopController.CreateGroup (4);
		}
		if (Input.GetKeyDown (KeyCode.Keypad5))
		{
			troopController.CreateGroup (5);
		}
		if (Input.GetKeyDown (KeyCode.Keypad6))
		{
			troopController.CreateGroup (6);
		}
		if (Input.GetKeyDown (KeyCode.Keypad7))
		{
			troopController.CreateGroup (7);
		}
		if (Input.GetKeyDown (KeyCode.Keypad8))
		{
			troopController.CreateGroup (8);
		}
		if (Input.GetKeyDown (KeyCode.Keypad9))
		{
			troopController.CreateGroup (9);
		}
#endif
#if !UNITY_IPHONE && !UNITY_ANDROID
		bool leftCtrl = Input.GetKey(KeyCode.LeftControl);
		if (leftCtrl)
		{
			if (Input.GetKeyDown (KeyCode.Alpha0))
			{
				troopController.CreateGroup (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				troopController.CreateGroup (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2))
			{
				troopController.CreateGroup (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3))
			{
				troopController.CreateGroup (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4))
			{
				troopController.CreateGroup (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5))
			{
				troopController.CreateGroup (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6))
			{
				troopController.CreateGroup (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7))
			{
				troopController.CreateGroup (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8))
			{
				troopController.CreateGroup (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9))
			{
				troopController.CreateGroup (9);
			}
		}
		else
		{
			if (Input.GetKeyDown (KeyCode.Alpha0))
			{
				SelectGroup (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				SelectGroup (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2))
			{
				SelectGroup (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3))
			{
				SelectGroup (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4))
			{
				SelectGroup (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5))
			{
				SelectGroup (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6))
			{
				SelectGroup (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7))
			{
				SelectGroup (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8))
			{
				SelectGroup (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9))
			{
				SelectGroup (9);
			}
		}
#endif
	}
	
	void SelectGroup (int numberOfGroup)
	{
		factoryController.DeselectFactory ();
		bool hasGroup = troopController.SelectGroup (numberOfGroup);
		
		if (!hasGroup) return;
		
		Vector3 getPosition = troopController.selectedSoldiers[0].transform.position - Vector3.forward * touchController.mainCamera.orthographicSize;
		getPosition = touchController.mainCamera.GetComponent<CameraBounds> ().ClampScenario (getPosition);
		
		touchController.mainCamera.transform.position = getPosition;
		
	}

	void OnGUI ()
	{
		if (touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id0)
		{
			GUI.Box (new Rect(Mathf.Min(touchController.FirstPosition.x, touchController.CurrentPosition.x),
						Mathf.Min(touchController.FirstPosition.y, touchController.CurrentPosition.y),
						Mathf.Abs(touchController.FirstPosition.x - touchController.CurrentPosition.x), 
						Mathf.Abs(touchController.FirstPosition.y - touchController.CurrentPosition.y)), "");
		}
	}
	
	Plane[] CalculateRect (Rect r)
	{
		var c = Camera.mainCamera;

		// Project the rectangle into world space
		var c0 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, c.nearClipPlane));
		var c1 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMax, c.nearClipPlane));
		var c2 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMin, c.nearClipPlane));
		var c3 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, c.nearClipPlane));

		var c4 = c.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, c.farClipPlane));
		var c5 = c.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, c.farClipPlane));
		
		Plane[] planes = new Plane[4];
		// Define the planes of the rectangle projected into the world
		planes[0] = new Plane(c0, c4, c2);
		planes[1] = new Plane(c2, c5, c3);
		planes[2] = new Plane(c3, c5, c1);
		planes[3] = new Plane(c1, c4, c0);
		
		return planes;
	}
}