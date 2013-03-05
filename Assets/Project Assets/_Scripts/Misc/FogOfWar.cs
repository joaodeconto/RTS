using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Visiorama;

public class FogOfWar : MonoBehaviour
{
	public GameObject pref_plane;

	public bool DarkFog = true;
	public Color visibleAreaColor = new Color(1.0f,1.0f,1.0f,0.0f);
	public Color knownAreaColor   = new Color(0.5f,0.5f,0.5f,0.5f);

	public Texture2D texture;

	public List<Transform> allyTransforms;
	public List<GameObject> enemyGO;

	public Vector3 mapSize;

	Renderer r;

	private const int SizeTexture = 64;

	void Start()
	{
		texture = new Texture2D(SizeTexture,SizeTexture, TextureFormat.ARGB32, false);

		for(int i = 0; i != SizeTexture; ++i)
			for(int j = 0; j != SizeTexture; ++j)
			{
				if(DarkFog)
					texture.SetPixel(i,j, Color.black);
				else
					texture.SetPixel(i,j, knownAreaColor);
			}

		texture.Apply();

		GameObject poly = Instantiate(pref_plane, Vector3.zero, Quaternion.identity) as GameObject;

		poly.layer = LayerMask.NameToLayer("FogOfWar");

		Transform polyTrns = poly.transform;

		polyTrns.parent = this.transform;
		polyTrns.localPosition    = Vector3.zero;
		polyTrns.localScale       = Vector3.one * 105f;
		polyTrns.localEulerAngles = new Vector3(270,180,0);

		r = poly.renderer;//.GetComponent<MeshRenderer>();

		//GameplayManager
		TerrainData td = ComponentGetter.Get<Terrain>("Terrain").terrainData;
		mapSize = td.size;
	}

	void Update()
	{
		foreach(Transform trns in allyTransforms)
		{
			Debug.Log("(int)(SizeTexture * (trns.position.x / mapSize.x)): " + (int)(SizeTexture * (trns.position.x / mapSize.x)));
			Debug.Log("(int)(SizeTexture * (trns.position.z / mapSize.z)): " + (int)(SizeTexture * (trns.position.z / mapSize.z)));

			texture.SetPixel((int)(SizeTexture * (trns.position.x / mapSize.x)),
							 (int)(SizeTexture * (trns.position.z / mapSize.z)),
							 visibleAreaColor);
		}

		texture.Apply();
		r.material.mainTexture = texture;
	}
}
