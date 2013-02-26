using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class MiniMapController : MonoBehaviour
{
	public GameObject pref_UnitMiniMap;
	public GameObject pref_StructureMiniMap;

	public const float MiniMapRefreshInterval = 1.0f;

	private List<Transform>[] structureList;
	private List<Transform>[] unitList;

	private List<GameObject>[] UnitMiniMapList;
	private List<GameObject>[] StructureMiniMapList;

	private bool WasInitialized;

	public MiniMapController Init()
	{
		if(WasInitialized)
		{
			Debug.LogError("Classe ja inicializada!");
			return this;
		}

		WasInitialized = true;

		GameplayManager gm = ComponentGetter.Get<GameplayManager>();

		int nTeams = gm.teams.Length;

		structureList = new List<Transform>[nTeams];
		unitList      = new List<Transform>[nTeams];

		UnitMiniMapList      = new List<GameObject>[nTeams];
		StructureMiniMapList = new List<GameObject>[nTeams];

		InvokeRepeating("UpdateMiniMap",
						MiniMapRefreshInterval,
						MiniMapRefreshInterval);

		return this;
	}

	void Awake()
	{
		if(!WasInitialized)
			Init();
	}

	void UpdateMiniMap()
	{

	}

#region Add and Remove Structures/Units

	GameObject InstantiateMiniMapObject(GameObject go, Transform trns)
	{
		GameObject _go = Instantiate(go, Vector3.zero, Quaternion.identity) as GameObject;



		return _go;
	}

	public void AddStructure (Transform trns, int teamId)
	{
		GameObject miniMapObject = InstantiateMiniMapObject(pref_StructureMiniMap, trns);

			   structureList[teamId].Add(trns);
		StructureMiniMapList[teamId].Add(miniMapObject);
	}

	public void AddUnit (Transform trns, int teamId)
	{
		GameObject miniMapObject = InstantiateMiniMapObject(pref_UnitMiniMap, trns);

			   unitList[teamId].Add(trns);
		UnitMiniMapList[teamId].Add(miniMapObject);
	}

	public void RemoveStructure (Transform trns, int teamId)
	{
		int index = structureList[teamId].IndexOf(trns);

		GameObject obj = StructureMiniMapList[teamId][index];

		Destroy(obj);

			   structureList[teamId].RemoveAt(index);
		StructureMiniMapList[teamId].RemoveAt(index);
	}

	public void RemoveUnit (Transform trns, int teamId)
	{
		int index = unitList[teamId].IndexOf(trns);

		GameObject obj = UnitMiniMapList[teamId][index];

		Destroy(obj);

			   unitList[teamId].RemoveAt(index);
		UnitMiniMapList[teamId].RemoveAt(index);
	}
#endregion

}
