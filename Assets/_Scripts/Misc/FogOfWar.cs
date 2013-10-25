using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
	protected FOWSystem fowSystem;
	protected FOWSystem FowSystem
	{
		get
		{
			if (fowSystem == null)
			{
				fowSystem = FOWSystem.instance;
			}
			return fowSystem;
		}
	}
	
	public const int SIZE_OF_TEXTURE = 128;
	
	public bool UseFog  = true;
	public bool DarkFog = true;

	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);
	
	public Terrain mapTerrain;
	
	public float fogHeight = 8.0f;

	public Texture2D FogTexture { get; private set; }

	private List<Transform> allies;
	private List<Transform> enemies;
	private List<IStats> entityAllies;
	private List<IStats> entityEnemies;

	private Vector3 mapSize;

	private enum FogFlag
	{
		NOT_KNOWN_AREA,
		VISIBLE,
		KNOWN_AREA,
	}

	//Temporary variables
	Transform trns = null;
	short maxX, maxY, minX, minY,
		  range, xRange = 0,
		  posX, posY,
		  i,j,k;

	FogFlag[,] matrixFogFlag;

	public FogOfWar Init()
	{
		if(!UseFog)
			return this;

		FogTexture    = new Texture2D (SIZE_OF_TEXTURE, SIZE_OF_TEXTURE, TextureFormat.ARGB32, false);
		matrixFogFlag = new FogFlag[SIZE_OF_TEXTURE,SIZE_OF_TEXTURE];
		
		//GameplayManager
		TerrainData td = mapTerrain.terrainData;
		
		float offsetSize = 1.1f;
		mapSize = new Vector3(td.size.x * offsetSize, td.size.y * offsetSize, td.size.z * offsetSize);
		
		for (i = 0; i != SIZE_OF_TEXTURE; ++i)
			for (j = 0; j != SIZE_OF_TEXTURE; ++j)
			{
//				if(DarkFog)
					FogTexture.SetPixel(i,j, new Color (0f, 0f, 0f, 0.75f));
//				else
//					FogTexture.SetPixel(i,j, knownAreaColor);

				matrixFogFlag[i,j] = FogFlag.NOT_KNOWN_AREA;
			}

		FogTexture.Apply();

		//posicionando FogOfWar no local correto
		this.transform.position = new Vector3(mapSize.x * 0.5f , 0, mapSize.z * 0.5f);

//		GameObject poly = Instantiate(pref_plane, Vector3.zero, Quaternion.identity) as GameObject;
//
//		poly.layer = LayerMask.NameToLayer("FogOfWar");
//
//		Transform polyTrns = poly.transform;
//
//		polyTrns.parent = this.transform;
//		polyTrns.localPosition    = Vector3.up * fogHeight;
//		polyTrns.localScale       = new Vector3(mapSize.x, mapSize.z, mapSize.y) * 0.5f;
//		polyTrns.localEulerAngles = new Vector3 (270,180,0);
//
//		poly.renderer.material.mainTexture = FogTexture;

		allies        = new List<Transform>();
		entityAllies  = new List<IStats>();
		enemies       = new List<Transform>();
		entityEnemies = new List<IStats>();

		InvokeRepeating ("UpdateNow", 0.1f, 1f);
		
		return this;
	}

	void UpdateNow()
	{
		if (!UseFog || allies == null)
			return;

//		UpdateEnemyVisibility();

		for (i = 0; i != SIZE_OF_TEXTURE; ++i)
			for (j = 0; j != SIZE_OF_TEXTURE; ++j)
			{
				//trocar pela textura do tasharen
//				if(matrixFogFlag[i,j] == FogFlag.VISIBLE)//== FogFlag.VISIBLE)
				Color mapColor = FowSystem.texture1.GetPixel (i, j);
				if (new Color (0f, 0f, 0f, 0f) != mapColor)
				{
					FogTexture.SetPixel(i, j, visibleAreaColor);

					//deixar área invisivel para que seja verificada posteriormente se é visivel
					matrixFogFlag[i,j] = FogFlag.KNOWN_AREA;
				}
//				else
//				{
//					FogTexture.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, color.a));
//				}
			}

		FogTexture.Apply();
	}

	void UpdateEnemyVisibility()
	{
		for (i = (short)(enemies.Count - 1); i != -1; --i)
		{
			trns = enemies[i];

			if (trns == null)
			{
					  enemies.RemoveAt(i);
				entityEnemies.RemoveAt(i);
				++i;
				continue;
			}

			//posX = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			//posY = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			posX = (short)(SIZE_OF_TEXTURE * (trns.position.x / mapSize.x));
			posY = (short)(SIZE_OF_TEXTURE * (trns.position.z / mapSize.z));

			bool positionIsVisible = (matrixFogFlag[posX,posY] == FogFlag.VISIBLE);

//			Debug.Log("positionIsVisible: " + positionIsVisible);
			//Só aplicando se mudar o estado de visibilidade do inimigo
			if(!entityEnemies[i].IsVisible && positionIsVisible)
			{
//				Debug.Log("chegou 1");
				entityEnemies[i].SetVisible(true);
			}
			else if(entityEnemies[i].IsVisible && !positionIsVisible)
			{
				entityEnemies[i].SetVisible(false);
			}

			//Debug.Log("matrixFogFlag[posX,posY]: " + matrixFogFlag[posX,posY]);
			//Debug.Log("posX: " + posX + " - posY: " + posY);
		}
	}

	public FogOfWar AddEntity(Transform trnsEntity, IStats entity)
	{
		if(!UseFog)
			return null;
		
		if(GameplayManager.mode == GameplayManager.Mode.Cooperative)
		{
			if(ComponentGetter.Get<GameplayManager>().IsAlly(entity.ally))
			{
				allies.Add(trnsEntity);
				entityAllies.Add(entity);
			}
			else
			{
				enemies.Add(trnsEntity);
				entityEnemies.Add(entity);
			}
		}
		else
		{
			if(ComponentGetter.Get<GameplayManager>().IsSameTeam(entity.team))
			{
				allies.Add(trnsEntity);
				entityAllies.Add(entity);
			}
			else
			{
				enemies.Add(trnsEntity);
				entityEnemies.Add(entity);
			}
		}

		return this;
	}

	public FogOfWar RemoveEntity(Transform trnsEntity, IStats entity)
	{
		if(!UseFog || trnsEntity == null)
			return null;
		
		if(GameplayManager.mode == GameplayManager.Mode.Cooperative)
		{
			if(ComponentGetter.Get<GameplayManager>().IsAlly(entity.ally))
			{
				int index = allies.IndexOf(trnsEntity) != null ? allies.IndexOf(trnsEntity) : -1;
	
				if (index == -1) return null;
	
					  allies.RemoveAt(index);
				entityAllies.RemoveAt(index);
			}
			else
			{
				int index = enemies.IndexOf(trnsEntity) != null ? enemies.IndexOf(trnsEntity) : -1;
	
				if (index == -1) return null;
	
					  enemies.RemoveAt(index);
				entityEnemies.RemoveAt(index);
			}
		}
		else
		{
			if(ComponentGetter.Get<GameplayManager>().IsSameTeam(entity.team))
			{
				int index = allies.IndexOf(trnsEntity) != null ? allies.IndexOf(trnsEntity) : -1;
			
				if (index == -1) return null;
			
					  allies.RemoveAt(index);
				entityAllies.RemoveAt(index);
			}
			else
			{
				int index = enemies.IndexOf(trnsEntity) != null ? enemies.IndexOf(trnsEntity) : -1;
			
				if (index == -1) return null;
			
					  enemies.RemoveAt(index);
				entityEnemies.RemoveAt(index);
			}

		}

		return this;
	}

	public bool IsKnownArea (Transform trns)
	{
		if (!UseFog)
			return true;
		
		if (!DarkFog)
			return true;
		
		//posX = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.x / mapSize.x));
		//posY = Mathf.RoundToInt(SIZE_TEXTURE * (trns.position.z / mapSize.z));

		posX = (short)(fowSystem.textureSize * (trns.position.x / mapSize.x));
		posY = (short)(fowSystem.textureSize * (trns.position.z / mapSize.z));

		return (matrixFogFlag[posX, posY] == FogFlag.KNOWN_AREA);
	}
}
