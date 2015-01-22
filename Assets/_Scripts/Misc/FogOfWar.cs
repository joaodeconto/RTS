using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
			
	public int SIZE_TEXTURE = 128;

	public bool UseFog  = true;
	public bool DarkFog = true;

	public GameObject pref_plane;
	
	public Terrain terrain;
	public Vector3 mapSize;

	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);
	public Color blackFogColor    = new Color(0.0f,0.0f,0.0f,1.0f);

	public float fogHeight = 0.0f;
	public float fogUpdate = 0.6f;

	public Texture2D FogTexture { get; private set; }

	private List<Transform> allies = new List<Transform> ();
	private List<Transform> enemies = new List<Transform> ();
	private List<IStats> entityAllies = new List<IStats> ();
	private List<IStats> entityEnemies = new List<IStats> ();

	private enum FogFlag
	{
		NOT_KNOWN_AREA,
		VISIBLE,
		KNOWN_AREA,
	}

	//Temporary var	iables
	Transform trns = null;
	int maxX, maxY, minX, minY,
		  range, xRange = 0,
		  posX, posY,
		  i,j,k;

	FogFlag[,] matrixFogFlag;

	public FogOfWar Init()
	{
		if(!UseFog)
			return this;

		FogTexture    = new Texture2D (SIZE_TEXTURE, SIZE_TEXTURE, TextureFormat.ARGB32, false);
		matrixFogFlag = new FogFlag[SIZE_TEXTURE,SIZE_TEXTURE];

		//GameplayManager
		TerrainData td = terrain.terrainData;

		for (i = 0; i != SIZE_TEXTURE; ++i)
			for (j = 0; j != SIZE_TEXTURE; ++j)
			{
				if(DarkFog)
					FogTexture.SetPixel(i,j, blackFogColor);
				else
					FogTexture.SetPixel(i,j, knownAreaColor);

				matrixFogFlag[i,j] = FogFlag.NOT_KNOWN_AREA;
			}

		FogTexture.Apply();

		// posicionando FogOfWar no local correto
//		this.transform.position = new Vector3(mapSize.x * 0.5f , 0, mapSize.z * 0.5f);
		
#if UNITY_ANDROID || UNITY_IPHONE || true	
		GameObject poly = Instantiate(pref_plane, Vector3.zero, Quaternion.identity) as GameObject;

		poly.layer = LayerMask.NameToLayer("FogOfWar");

		Transform polyTrns = poly.transform;

		polyTrns.parent = this.transform;
		polyTrns.localPosition    = Vector3.up * fogHeight;
		polyTrns.localScale       = new Vector3(mapSize.x, mapSize.z, mapSize.y) * 0.5f;
		polyTrns.localEulerAngles = new Vector3 (270,180,0);

		poly.renderer.material.mainTexture = FogTexture;
#endif

		InvokeRepeating ("UpdateFog", 0f,fogUpdate);
		
		return this;
	}

	void UpdateFog ()
	{
		if (!UseFog || allies == null)
			return;
		

		for (i = (allies.Count - 1); i != -1; --i)
		{
			trns = allies[i];
			
			if (trns == null)
			{
				allies.RemoveAt(i);
				entityAllies.RemoveAt(i);
				continue;
			}

			posX = (int)(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = (int)(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			range = (int)(SIZE_TEXTURE * (entityAllies[i].fieldOfView / mapSize.x));

			maxY = (Mathf.Clamp(posY + range, 0, SIZE_TEXTURE));
			minY = (Mathf.Clamp(posY - range, 0, SIZE_TEXTURE));

			for (k = minY; k != maxY; ++k)//, angle += changeAngleRate)
			{
				xRange = (int)(Mathf.Sqrt((range * range) - ((k - posY) * (k - posY)) ));

				maxX = (Mathf.Clamp(posX + xRange, 0, SIZE_TEXTURE));
				minX = (Mathf.Clamp(posX - xRange, 0, SIZE_TEXTURE));

				for (j = minX; j != maxX; ++j)
				{
					matrixFogFlag [j,k] = FogFlag.VISIBLE;
				}
			}
		}

		UpdateEnemyVisibility();

		for (i = 0; i != SIZE_TEXTURE; ++i)
			for (j = 0; j != SIZE_TEXTURE; ++j)
			{
				if(matrixFogFlag[i,j] == FogFlag.VISIBLE)//== FogFlag.VISIBLE)
				{
					FogTexture.SetPixel(i, j, visibleAreaColor);

					//deixar área invisivel para que seja verificada posteriormente se é visivel
					matrixFogFlag[i,j] = FogFlag.KNOWN_AREA;
				}
				else if(matrixFogFlag[i,j] == FogFlag.KNOWN_AREA)//fogNodeVisited[i,j])
					FogTexture.SetPixel(i, j, knownAreaColor);
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
				continue;
			}

			posX = (short)(SIZE_TEXTURE * (trns.position.x / mapSize.x));
			posY = (short)(SIZE_TEXTURE * (trns.position.z / mapSize.z));

			bool positionIsVisible = (matrixFogFlag[posX,posY] == FogFlag.VISIBLE);

			//Só aplicando se mudar o estado de visibilidade do inimigo
			if(!entityEnemies[i].IsVisible && positionIsVisible)
			{
				entityEnemies[i].SetVisible(true);
			}
			else if(entityEnemies[i].IsVisible && !positionIsVisible)
			{
				entityEnemies[i].SetVisible(false);
			}
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
//				Debug.LogError ("allies: " + entity.name);
				allies.Add(trnsEntity);
				entityAllies.Add(entity);
			}
			else
			{
//				Debug.LogError ("enemies: " + entity.name);
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

	public void RemoveBlackFog()
	{
		for (i = 0; i != SIZE_TEXTURE; ++i)
			for (j = 0; j != SIZE_TEXTURE; ++j)
		{
			FogTexture.SetPixel(i,j, knownAreaColor);
			
			matrixFogFlag[i,j] = FogFlag.KNOWN_AREA;
		}
		FogTexture.Apply();
	}


	public bool IsKnownArea (Transform trns)
	{
		if (!UseFog)
			return true;
		
		if (!DarkFog)
			return true;
		
		posX = (short)(SIZE_TEXTURE * (trns.position.x / mapSize.x));
		posY = (short)(SIZE_TEXTURE * (trns.position.z / mapSize.z));

		return (matrixFogFlag[posX, posY] == FogFlag.KNOWN_AREA);
	}
}