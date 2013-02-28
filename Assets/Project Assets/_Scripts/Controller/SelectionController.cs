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

					Bounds b = new Bounds((touchController.GetFirstPoint+touchController.GetFinalPoint)/2, windowSize + (Vector3.up * 100f) );

					VDebug.DrawCube (b, Color.green);

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
								factoryController.SelectFactory (factory);
							}
						}
						else
						{
							if (Math.AABBContains (factory.transform.localPosition, b, Math.IgnoreVector.Y))
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
		troopController.SelectGroup (numberOfGroup);
	}

	void OnGUI ()
	{
		if (touchController.DragOn && touchController.idTouch == TouchController.IdTouch.Id0)
		{
			GUI.Box (new Rect(touchController.FirstPosition.x, touchController.FirstPosition.y,
				touchController.CurrentPosition.x - touchController.FirstPosition.x, touchController.CurrentPosition.y - touchController.FirstPosition.y), "");
		}
	}
	}
