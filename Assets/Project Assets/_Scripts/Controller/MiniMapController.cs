using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class MiniMapController : MonoBehaviour
{
	public GameObject pref_UnitMiniMap;
	public GameObject pref_StructureMiniMap;

	public GameObject miniMapPanel;
	public UIRoot MiniMapRoot;

	public GameObject CamPositionMiniMap;

	public Transform mapTransform;

	public Vector3 visualizationSize;
	public Vector3 visualizationPosition;

	private Vector3 offsetCamPos = new Vector3(20,0,30);

	public float MiniMapRefreshInterval = 0.4f;

	private Vector3 miniMapSize;

	public Vector3 mapSize { get; private set; }

	private List<Transform>[] structureList;
	private List<Transform>[] unitList;

	private List<GameObject>[] UnitMiniMapList;
	private List<GameObject>[] StructureMiniMapList;

	private GameObject mainCameraGO;

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

		//CameraBounds cb = ComponentGetter.Get<CameraBounds>("Main Camera");

		//mapSize = new Vector3 ( cb.scenario.x.max - cb.scenario.x.min,
								//0,
								//cb.scenario.z.max - cb.scenario.z.min);

		mainCameraGO = GameObject.Find("Main Camera");

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
		miniMapSize = mapTransform.localScale;//miniMapCollider.bounds.max - miniMapCollider.bounds.min;//.size;//(miniMapMaxPoint - miniMapMinPoint);
		Vector3 newScale = new Vector3( visualizationSize.x / mapSize.x,
										visualizationSize.y / mapSize.z,
										1f);

		newScale.x *= mapTransform.localScale.x;
		newScale.y *= mapTransform.localScale.y;
		newScale.z *= mapTransform.localScale.z;

		CamPositionMiniMap.transform.localScale = newScale;
	}

	void UpdateMiniMap()
	{
#if UNITY_EDITOR
		RefreshMiniMapSize();
#endif
		//Update camera mini map position
		UpdateMiniMapCameraPosition();

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

	void UpdateMiniMapCameraPosition()
	{
		//CamPositionMiniMap, mainCameraGO.transform

		Vector3 percentPos = new Vector3 (  (mainCameraGO.transform.position.x) / mapSize.x,
											(offsetCamPos.z + mainCameraGO.transform.position.z) / mapSize.z,
											-5);

		//Debug.Log("percentPos (" + referenceTrns.name + "): " + percentPos);

		CamPositionMiniMap.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
																 (mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),
																 (-5));
	}

	void UpdatePosition(GameObject miniMapObject, Transform referenceTrns)
	{
		Vector3 percentPos = new Vector3(referenceTrns.position.x / mapSize.x,
										 referenceTrns.position.z / mapSize.z,
										 -5);

		//Debug.Log("percentPos (" + referenceTrns.name + "): " + percentPos);
		//Debug.Log("miniMapSize: " + miniMapSize);

		miniMapObject.transform.localPosition = new Vector3((mapTransform.localPosition.x + (miniMapSize.x * percentPos.x)),
															(mapTransform.localPosition.y + (miniMapSize.y * percentPos.y)),
															(-5));
	}

	public void UpdateCameraPosition()
	{
		CameraBounds camBounds = mainCameraGO.GetComponent<CameraBounds>();

		Vector3 camBoundsSize = mapSize;
								//new Vector3((camBounds.scenario.x.max - camBounds.scenario.x.min),
											//(camBounds.scenario.y.max - camBounds.scenario.y.min),
											//(camBounds.scenario.z.max - camBounds.scenario.z.min));

		Debug.Log("mainCameraGO.transform.localPosition: " + mainCameraGO.transform.localPosition);
		//Debug.Log("UICamera.lastTouchPosition: " + UICamera.lastTouchPosition * MiniMapRoot.pixelSizeAdjustment);

		Vector2 percentPos = new Vector2 (  (mapTransform.localPosition.x +
												(MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.x)) / miniMapSize.x,
											(mapTransform.localPosition.y +
												(MiniMapRoot.pixelSizeAdjustment * UICamera.lastTouchPosition.y)) / miniMapSize.y);

		Debug.Log("percentPos: " + percentPos);
		Debug.Log("mapSize: " + mapSize);
		Debug.Log("mainCameraGO.transform.position: " + mainCameraGO.transform.position);

		Vector3 newCameraPosition = new Vector3((camBoundsSize.x * percentPos.x)         - (offsetCamPos.x ),
											    (mainCameraGO.transform.localPosition.y) - (offsetCamPos.y),
											    (camBoundsSize.z * percentPos.y)         - (offsetCamPos.z * 1.5f)  );

		mainCameraGO.transform.position = mainCameraGO.GetComponent<CameraBounds>().ClampScenario(newCameraPosition);
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
