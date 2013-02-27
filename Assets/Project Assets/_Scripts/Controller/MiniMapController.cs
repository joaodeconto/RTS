using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class MiniMapController : MonoBehaviour
{
	public GameObject pref_UnitMiniMap;
	public GameObject pref_StructureMiniMap;
	public GameObject panel;

	public Vector3 miniMapMaxPoint;
	public Vector3 miniMapMinPoint;

	private Vector3 miniMapSize;

	public float MiniMapRefreshInterval = 1.0f;

	public Vector3 mapSize { get; private set; }

	private List<Transform>[] structureList;
	private List<Transform>[] unitList;

	private List<GameObject>[] UnitMiniMapList;
	private List<GameObject>[] StructureMiniMapList;

	private bool WasInitialized;

	public MiniMapController Init()
	{
		if (WasInitialized)
		{
			Debug.LogError("Classe ja inicializada!");
			return this;
		}

		WasInitialized = true;

		GameplayManager gm = ComponentGetter.Get<GameplayManager>();

		int nTeams = gm.teams.Length;

		unitList      = new List<Transform>[nTeams];
		structureList = new List<Transform>[nTeams];

		UnitMiniMapList      = new List<GameObject>[nTeams];
		StructureMiniMapList = new List<GameObject>[nTeams];

		for(int i = structureList.Length - 1; i != -1; --i)
		{
				 unitList[i] = new List<Transform>();
			structureList[i] = new List<Transform>();

				 UnitMiniMapList[i] = new List<GameObject>();
			StructureMiniMapList[i] = new List<GameObject>();
		}

		InvokeRepeating("UpdateMiniMap",
						MiniMapRefreshInterval,
						MiniMapRefreshInterval);

		TerrainData td = ComponentGetter.Get<Terrain>("Terrain").terrainData;

		mapSize = td.size;

		RefreshMiniMapSize();

		return this;
	}

	void Awake()
	{
		if(!WasInitialized)
			Init();
	}

	void RefreshMiniMapSize()
	{
		miniMapSize = (miniMapMaxPoint - miniMapMinPoint);
	}

	void UpdateMiniMap()
	{
#if UNITY_EDITOR
		RefreshMiniMapSize();
#endif
		//iterate by teams
		for(int i = structureList.Length - 1; i != -1; --i)
		{
			//iterate by structures
			for(int j = structureList[i].Count - 1; j != -1; --j)
			{
				UpdatePosition(StructureMiniMapList[i][j], structureList[i][j]);
			}

			//iterate by unit
			for(int j = unitList[i].Count - 1; j != -1; --j)
			{
				UpdatePosition(UnitMiniMapList[i][j], unitList[i][j]);
			}
		}
	}

	void UpdatePosition(GameObject miniMapObject, Transform referenceTrns)
	{
		Vector3 percentPos = new Vector3 ( referenceTrns.position.x / mapSize.x,
										   referenceTrns.position.z / mapSize.z,
										   -5);

		Debug.Log("percentPos (" + referenceTrns.name + "): " + percentPos);

		miniMapObject.transform.localPosition = new Vector3((int)(miniMapSize.x * percentPos.x),
															(int)(miniMapSize.y * percentPos.y),
															(int)(miniMapSize.z * percentPos.z));
	}

#region Add and Remove Structures/Units
	GameObject InstantiateMiniMapObject(GameObject pref_go, Transform trns)
	{
		GameObject _go = Instantiate(pref_go, Vector3.zero, Quaternion.identity) as GameObject;

		_go.transform.parent     = panel.transform;
		_go.transform.localScale = pref_go.transform.localScale;

		UpdatePosition(_go, trns);

		return _go;
	}

	public void AddStructure (Transform trns, int teamId)
	{
		GameObject miniMapObject = InstantiateMiniMapObject(pref_StructureMiniMap, trns);

		Debug.Log("teamId: " + teamId);

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
