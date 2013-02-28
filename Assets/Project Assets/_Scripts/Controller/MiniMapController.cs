using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class MiniMapController : MonoBehaviour
{
	public GameObject pref_UnitMiniMap;
	public GameObject pref_StructureMiniMap;
	public GameObject miniMapPanel;

	public Vector3 miniMapMaxPoint;
	public Vector3 miniMapMinPoint;

	private Vector3 miniMapSize;

	public float MiniMapRefreshInterval = 0.4f;

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

		miniMapObject.transform.localPosition = new Vector3((int)(miniMapSize.x * percentPos.x),
															(int)(miniMapSize.y * percentPos.y),
															(int)(miniMapSize.z * percentPos.z));
	}

	public void UpdateCameraPosition()
	{
		GameObject cameraGO = GameObject.Find("Main Camera");

		CameraBounds camBounds = cameraGO.GetComponent<CameraBounds>();

		Vector3 camBoundsSize = new Vector3((camBounds.scenario.x.max - camBounds.scenario.x.min),
											(camBounds.scenario.y.max - camBounds.scenario.y.min),
											(camBounds.scenario.z.max - camBounds.scenario.z.min));

		Vector2 percentPos = new Vector2 ( Input.mousePosition.x / mapSize.x,
										   Input.mousePosition.y / mapSize.z);

		//Vector3 percentPos = new Vector3 ( cameraGO.transform.position.x / camBoundsSize.x,
										   //cameraGO.transform.position.y / camBoundsSize.y,
										   //cameraGO.transform.position.z / camBoundsSize.z);

		cameraGO.transform.localPosition = new Vector3 (camBoundsSize.x * percentPos.x,
														cameraGO.transform.localPosition.y,
														camBoundsSize.z * percentPos.y);
	}

#region Add and Remove Structures/Units
	GameObject InstantiateMiniMapObject(GameObject pref_go, Transform trns, int teamId)
	{
		GameObject _go = Instantiate(pref_go, Vector3.zero, Quaternion.identity) as GameObject;

		_go.transform.parent     = miniMapPanel.transform;
		_go.transform.localScale = pref_go.transform.localScale;

		Color teamColor = ComponentGetter.Get<GameplayManager>().teams[teamId].color;

		_go.GetComponent<UISlicedSprite>().color = teamColor;

		UpdatePosition(_go, trns);

		return _go;
	}

	public void AddStructure (Transform trns, int teamId)
	{
#if UNITY_EDITOR
	   if(structureList.Length <= teamId)
	   {
			Debug.Log("O numero de times eh menor do que o id enviado");
			Debug.Log("teamId: " + teamId);
			Debug.Break();
	   }
#endif

		GameObject miniMapObject = InstantiateMiniMapObject(pref_StructureMiniMap, trns, teamId);

			   structureList[teamId].Add(trns);
		StructureMiniMapList[teamId].Add(miniMapObject);
	}

	public void AddUnit (Transform trns, int teamId)
	{
#if UNITY_EDITOR
	   if(unitList.Length <= teamId)
	   {
			Debug.Log("O numero de times eh menor do que o id enviado");
			Debug.Log("teamId: " + teamId);
			Debug.Break();
	   }
#endif
		GameObject miniMapObject = InstantiateMiniMapObject(pref_UnitMiniMap, trns, teamId);

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
