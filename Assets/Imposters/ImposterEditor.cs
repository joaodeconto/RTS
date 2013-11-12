// Automatic 3D Billboard Imposters
// By CWKX

using UnityEngine;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
 
public class ImposterEditor : MonoBehaviour
{
	public string exportPath;
	public string exportSuffix = "";
	public bool   exportPrefabs = true;
	public float  lodScalar = 7.0f;
	public GameObject lighting = null;
	
	[System.Serializable]
	public class Imposter
	{
		[SerializeField] public string exportName;
		[SerializeField] public GameObject obj;
		[SerializeField] public int tileSize = 32;
		[SerializeField] public float yStep = 0.5f;
	}
	
	public Imposter[] imposters;
	
	private Material imposterMaterial = null;
	private bool errorReset = false;
	private bool enableGUI = true;
	private bool preview = false;
	private bool export = false;
	private float rotationY;
	private float moveSpeed = 1.0f;
	private float previewZ = 10f;
	private GameObject outputParent = null;
	private Color ambientLight;
	
	void Start()
	{
		outputParent = new GameObject("Output");
		
		// You may add custom imposter shaders here/use the unity ones such as Transparent (however they do not have stippling)
		imposterMaterial = new Material(Shader.Find("StipplingImposter"));
		
		ambientLight = RenderSettings.ambientLight;
		RenderSettings.ambientLight = Color.white;
		
		// Setup Camera
		gameObject.AddComponent<Camera>();
		gameObject.AddComponent<GUILayer>();
		
		gameObject.camera.backgroundColor = new Color32(0, 0, 0, 254);
		gameObject.camera.farClipPlane = 10000f;
	}
	
	void Update()
	{
		if (!preview) return;
		
		// Tiny FPS camera look
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
	        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 10f;
	
	        rotationY += Input.GetAxis("Mouse Y") * 10f;
	        rotationY = Mathf.Clamp (rotationY, -90f, 90f);
	
	        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0f);
		}
		
		// Tiny FPS camera move
		moveSpeed = Mathf.Max(0.0f, moveSpeed + Input.GetAxis("Mouse ScrollWheel") * moveSpeed * 2.0f);
		transform.Translate(new Vector3(Input.GetAxis("Horizontal") * moveSpeed, 0f, Input.GetAxis("Vertical") * moveSpeed));
	}
	
	void OnGUI()
	{
		if (!enableGUI) return;
		
		GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Space(20f);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (errorReset) 
			GUILayout.Box("Error");
		else if (preview)
		{
			if (GUILayout.Button("Preview"))
			{
				for (int n=0; n<3; n++)
				{
					GameObject parent = (GameObject)Instantiate(outputParent);
					parent.name = "Preview";
					Arrange(parent);
				}
			}
		}
		else
		{
			if (GUILayout.Button("Generate"))
			{
		    	StartCoroutine(GenerateImposters());
				preview = true;
			}
		}
		
		if (export || errorReset || !preview) GUILayout.Box("Export");
		else if (GUILayout.Button("Export"))
		{
			export = true;
			
			StartCoroutine(Export());
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	System.Collections.IEnumerator GenerateImposters()
	{
		enableGUI = false;
		lighting.SetActive(false);
		
		// Disactivate the generated output so they don't interfere with oneanother
		outputParent.SetActive(false);
		GameObject cameraClone = (GameObject)Instantiate(gameObject); cameraClone.SetActive(false);
		
		foreach (Imposter imposter in imposters)
		{
			if (!imposter.obj)
			{
				Debug.Log ("Cannot create imposter! No GameObject specified!");
				continue;
			}
			
			yield return StartCoroutine(CreateImposter(imposter));	
		}
		
		// Activate and preview generated imposters
		outputParent.SetActive(true);
		GameObject instPar = (GameObject)Instantiate(outputParent); instPar.name = "Working Parent";
		GameObject lodView = (GameObject)Instantiate(instPar);
		lodView.name = "Lod View";
		foreach (Transform transform in lodView.transform) {
			transform.gameObject.GetComponent<ImposterLOD>().lodDistance = 0f;
			transform.gameObject.GetComponent<ImposterLOD>().Reset();
		}
		Arrange(lodView);
		Arrange(instPar);
		outputParent.SetActive(false);
		outputParent = instPar;
		
		// Reset Camera
		camera.transform.position = cameraClone.camera.transform.position;
		camera.transform.rotation = cameraClone.camera.transform.rotation;
		camera.transform.localScale = cameraClone.camera.transform.localScale;
		camera.orthographic = cameraClone.camera.orthographic;
		camera.orthographicSize = cameraClone.camera.orthographicSize;
		camera.nearClipPlane = cameraClone.camera.nearClipPlane;
		camera.farClipPlane = cameraClone.camera.farClipPlane;
		camera.rect = cameraClone.camera.rect;
		
		// Reset Lighting
		lighting.SetActive(true);
		RenderSettings.ambientLight = ambientLight;
		
		Destroy(cameraClone);
		
		enableGUI = true;
		
		yield return null;
	}
	
	System.Collections.IEnumerator CreateImposter(Imposter imposter)
	{	
		// Get clean string naming
		imposter.obj.SetActive(true);
		imposter.exportName = Regex.Replace(imposter.exportName, "[^\\w\\._]", " ");
		while (imposter.exportName.Length>0 && imposter.exportName[imposter.exportName.Length-1] == ' ')
			imposter.exportName = imposter.exportName.Remove(imposter.exportName.Length-1, 1);
		if (imposter.exportName.Length>0 && imposter.exportName[0] == ' ')
		{
			imposter.exportName = imposter.exportName.Remove(0, 1);
			imposter.exportName = imposter.exportName.Insert(0, "_");
		}
		if (imposter.exportName == "")
			imposter.exportName = "_";
		
		// Create objects (this places whatever it is [obj, fbx, GameObject, ...] inside a game object "renderParent" for consistency)
		GameObject lod0 = (GameObject)Instantiate(imposter.obj); lod0.name = "Lod_0";
		GameObject renderParent = new GameObject(imposter.exportName); renderParent.name = imposter.exportName;
		GameObject lod1 = new GameObject(imposter.exportName); lod1.name = "Lod_1";
		
		lod0.transform.position = Vector3.zero; 
		lod0.transform.parent = renderParent.transform;
		lod1.transform.parent = outputParent.transform;
		
		if (imposter.tileSize <= 0)
		{
			Debug.Log("Could not create imposter: " + imposter.exportName + " \"Tile Size\" too small! Use a value such as \"32\"");
			errorReset = true;
			yield break;
		}
		if (imposter.tileSize > Mathf.Min(Screen.width, Screen.height))
		{
			Debug.Log("Could not create imposter: " + imposter.exportName + " \"Tile Size\" is larger than screen area! Please resize editor window or reduce the tile size!");
			errorReset = true;
			yield break;
		}
		
		// Init Mesh
		MeshFilter mf = lod1.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		
		// Init Material
		Renderer mr = lod1.AddComponent<MeshRenderer>();
		Texture2D tex = new Texture2D(imposter.tileSize*2, imposter.tileSize*2, TextureFormat.ARGB32, true);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.filterMode = FilterMode.Bilinear;
		mr.material = imposterMaterial;
		mr.material.name = imposter.exportName;
		mr.material.mainTexture = tex;
		mr.material.SetFloat("_Cutoff", 0.45f);
		tex.Apply();
		
		// Init Bounds
		renderParent.SetActive(true);
		Bounds bounds = GetBounds(renderParent);
		
		// Transform Render Obj to occupy in Unit Cube
		renderParent.transform.localScale = new Vector3((1f / bounds.size.x), 
													 	(1f / bounds.size.y), 
													 	(1f / bounds.size.z));
		
		// Recalculate Cube Bounds
		Bounds cubeBounds = GetBounds(renderParent);
		
		// Init Imposter Camera
		camera.transform.position = cubeBounds.center;
		camera.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		camera.transform.localScale = Vector3.one; // cubeBounds.size;
		camera.orthographic = true;
		camera.orthographicSize = 0.5f; // Mathf.Max(cubeBounds.extents.x, Mathf.Max(cubeBounds.extents.y, cubeBounds.extents.z));
		camera.nearClipPlane = -10f;
		camera.farClipPlane = 10f;
		camera.rect = new Rect(0f, 0f, (float)imposter.tileSize / Screen.width, (float)imposter.tileSize / Screen.height);
		
		// Setup Mesh
		Vector3[] vertices = new Vector3[12];
		int[] tri = new int[18];
		Vector3[] normals = new Vector3[12];
		Vector2[] uv = new Vector2[12];
		
		// Mesh Generate Data
		// Front
		vertices[0]  = new Vector3(0f, 0f, 0.5f);
		vertices[1]  = new Vector3(1f, 0f, 0.5f);
		vertices[2]  = new Vector3(0f, 1f, 0.5f);
		vertices[3]  = new Vector3(1f, 1f, 0.5f);
		tri[0] = 0; tri[1] = 2; tri[2] = 1;	
		tri[3] = 2; tri[4] = 3; tri[5] = 1;
		normals[0] = Vector3.up;
		normals[1] = Vector3.up;
		normals[2] = Vector3.up;
		normals[3] = Vector3.up;
		uv[0] = new Vector2(0.5f, 0.5f);
		uv[1] = new Vector2(1f, 0.5f);
		uv[2] = new Vector2(0.5f, 1f);
		uv[3] = new Vector2(1f, 1f);
		Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.forward);
		yield return StartCoroutine(Screenshot(tex, imposter.tileSize, imposter.tileSize, imposter.tileSize, imposter.tileSize));
		
		// Side
		vertices[4]  = new Vector3(0.5f, 0f, 0f);
		vertices[5]  = new Vector3(0.5f, 0f, 1f);
		vertices[6]  = new Vector3(0.5f, 1f, 0f);
		vertices[7]  = new Vector3(0.5f, 1f, 1f);
		tri[6] = 4; tri[7] = 6; tri[8] = 5;	
		tri[9] = 6; tri[10] = 7; tri[11] = 5;
		normals[4] = Vector3.up;
		normals[5] = Vector3.up;
		normals[6] = Vector3.up;
		normals[7] = Vector3.up;
		uv[4] = new Vector2(0f, 0.5f);
		uv[5] = new Vector2(0.5f, 0.5f);
		uv[6] = new Vector2(0f, 1f);
		uv[7] = new Vector2(0.5f, 1f);
		Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.left);
		yield return StartCoroutine(Screenshot(tex, imposter.tileSize, imposter.tileSize, 0, imposter.tileSize));
		
		// Top
		vertices[8]  = new Vector3(0f, imposter.yStep, 0f);
		vertices[9]  = new Vector3(1f, imposter.yStep, 0f);
		vertices[10] = new Vector3(0f, imposter.yStep, 1f);
		vertices[11] = new Vector3(1f, imposter.yStep, 1f);
		tri[12] = 8; tri[13] = 10; tri[14] = 9;	
		tri[15] = 10; tri[16] = 11; tri[17] = 9;
		normals[8] = Vector3.up;
		normals[9] = Vector3.up;
		normals[10] = Vector3.up;
		normals[11] = Vector3.up;
		uv[8]  = new Vector2(0.5f, 0f);
		uv[9]  = new Vector2(1f, 0f);
		uv[10] = new Vector2(0.5f, 0.5f);
		uv[11] = new Vector2(1f, 0.5f);
		Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.down);
		yield return StartCoroutine(Screenshot(tex, imposter.tileSize, imposter.tileSize, imposter.tileSize, 0));
		
		// Transform the geometry
		for (int i=0; i<12; i++) 
		{
			vertices[i].x = Mathf.Lerp(bounds.min.x, bounds.max.x, vertices[i].x);
			vertices[i].y = Mathf.Lerp(bounds.min.y, bounds.max.y, vertices[i].y);
			vertices[i].z = Mathf.Lerp(bounds.min.z, bounds.max.z, vertices[i].z);
		}
		
		// Create mipmaps
		tex.SetPixels(tex.GetPixels());
		
		// Set mesh data
		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.normals = normals;
		mesh.uv = uv;
		
		// Restore render parent and hide it to avoid intereference
		renderParent.transform.localScale = Vector3.one;
		lod1.transform.parent = renderParent.transform;
		renderParent.transform.parent = outputParent.transform;
		ImposterLOD imposterLOD = renderParent.AddComponent<ImposterLOD>();
		imposterLOD.lodDistance = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)) * lodScalar;
		imposterLOD.Reset();
	}
	
	System.Collections.IEnumerator Export()
	{
#if UNITY_EDITOR && !UNITY_WEBPLAYER
		
		// Create folders
		Directory.CreateDirectory(Application.dataPath + "/" + exportPath);
		Directory.CreateDirectory(Application.dataPath + "/" + exportPath + "/Imposters/Materials");
		Directory.CreateDirectory(Application.dataPath + "/" + exportPath + "/Imposters/Textures");

		if (exportPrefabs) 
			Directory.CreateDirectory(Application.dataPath + "/" + exportPath + "/Prefabs");
		
		// Create objects
		foreach (Transform transform in outputParent.transform)
		{
			GameObject obj = transform.FindChild("Lod_1").gameObject;
			MeshToFile(obj.GetComponent<MeshFilter>(), Application.dataPath + "/" + exportPath + "/Imposters/" + transform.gameObject.name + exportSuffix + ".obj", transform.gameObject.name);
		}
		
		// Create materials
		foreach (Transform transform in outputParent.transform)
		{
			GameObject obj = transform.FindChild("Lod_1").gameObject;
			
			// 1. Create textures
			Texture2D tex = (Texture2D)obj.GetComponent<MeshRenderer>().material.mainTexture;
			byte[] bytes = tex.EncodeToPNG();
   		    System.IO.File.WriteAllBytes(Application.dataPath + "/" + exportPath + "/Imposters/Textures/" +transform.gameObject.name + ".png", bytes);

			// 2. Overwrite OBJ materials
			AssetDatabase.CreateAsset(obj.GetComponent<MeshRenderer>().material, "Assets/" + exportPath + "/Imposters/Materials/" + stringObjProcess(transform.gameObject.name) + ".mat");
		}
		
		// Flush saved assets
		AssetDatabase.SaveAssets();
   	 	AssetDatabase.Refresh();
		
		
		// Link obj materials
		foreach (Transform transform in outputParent.transform)
		{
			GameObject obj = transform.gameObject;
			Texture2D texDisk = AssetDatabase.LoadAssetAtPath("Assets/" + exportPath + "/Imposters/Textures/" + obj.name + ".png", typeof(Texture2D)) as Texture2D;
			Material diskMat = AssetDatabase.LoadAssetAtPath("Assets/" + exportPath + "/Imposters/Materials/" + stringObjProcess(obj.name) + ".mat", typeof(Material)) as Material;
			diskMat.SetTexture("_MainTex", texDisk);
		}
		
		// Export Prefabs
		if (exportPrefabs)
		{	
			GameObject prefabParent = new GameObject("Prefabs");
			
			// Generate prefabs
			foreach (Imposter imposterSetting in imposters)
			{
				GameObject newPrefab = new GameObject(imposterSetting.exportName);
				newPrefab.transform.parent = prefabParent.transform;
				
				GameObject lod0 = (GameObject)Instantiate(imposterSetting.obj);
				GameObject disk = (GameObject)Instantiate((GameObject)AssetDatabase.LoadAssetAtPath("Assets/" + exportPath + "/Imposters/" + imposterSetting.exportName + exportSuffix + ".obj", typeof(GameObject)));
				Vector3 size = GetBounds(disk).size;
				float lodSize = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
				
				// Add two LODs to empty prefab
				lod0.name = "Lod_0";
				lod0.transform.parent = newPrefab.transform;
				lod0.transform.localPosition = Vector3.zero;
				
				// Strip out animator component of OBJ
				GameObject lod1 = disk.transform.GetChild(0).gameObject;
				lod1.name = "Lod_1";
				lod1.transform.parent = newPrefab.transform;
				Destroy(disk);
				
				// Add LOD script to parent
				ImposterLOD imposterLOD = newPrefab.AddComponent<ImposterLOD>();
				imposterLOD.lodDistance = lodSize * lodScalar;
				imposterLOD.Reset();

				Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + exportPath + "/Prefabs/" + imposterSetting.exportName + ".prefab");
				PrefabUtility.ReplacePrefab(newPrefab, prefab, ReplacePrefabOptions.ConnectToPrefab);
			}
			
			// Hide prefab parent
			prefabParent.SetActive(false);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		
#endif

		yield return null;
	}
	
	// Attempts to match the automatic .mat naming convention
	public static string stringObjProcess(string s)
	{
		for (int i=0; i<s.Length; i++)
		{
			if (s[i] == ' ') 
			{
				s = s.Remove(i,1);
   				s = s.Insert(i,"_");
				break;
			}
		}
		
		s += "Mat";
		return s;
	}
	
	// Custom OBJ exporter
 	public static string MeshToString(MeshFilter mf, string meshName)
	{
        Mesh m = mf.mesh;
        Material[] mats = mf.renderer.sharedMaterials;
 
		StringBuilder sb = new StringBuilder();
 
		sb.Append("g ").Append(meshName).Append("\n");
		foreach(Vector3 v in m.vertices) 
		{
			// Unity3D currently flips the coordinate system on import! (So we counter it with -v.x here)
			sb.Append(string.Format("v {0} {1} {2}\n",-v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.normals) {
			sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.uv) {
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}
		for (int material=0; material < m.subMeshCount; material ++) {
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[material].name).Append("\n");
			sb.Append("usemap ").Append(mats[material].name).Append("\n");
 
			int[] triangles = m.GetTriangles(material);
			for (int i=0;i<triangles.Length;i+=3) {
				sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
					triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
			}
		}
		return sb.ToString();
	}
 
    public static void MeshToFile(MeshFilter mf, string filename, string meshName) {
#if UNITY_EDITOR
        using (StreamWriter sw = new StreamWriter(filename)) 
        {
            sw.Write(MeshToString(mf, meshName));
        }
#endif
    }
	
	Bounds GetBounds(GameObject obj) 
	{
		bool newBounds = true;
		Bounds  bounds = new Bounds();
		
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
		
		foreach (Renderer renderer in renderers) {
			if (newBounds) {
				newBounds = false;
				bounds = renderer.bounds;
			}
			else
				bounds.Encapsulate(renderer.bounds);
		}

		return bounds;
	}
	
	void Arrange(GameObject parent)
	{
		float maxPreviewZ = 0f;
		float previewX = 0f;
		
		for (int i=0; i<2; i++)
		{
			foreach (Transform transform in parent.transform)
			{
				Bounds bounds = GetBounds(transform.gameObject);
				
				if (i==1) 
					transform.position = new Vector3(previewX + bounds.extents.x, 0f, previewZ);
				
				previewX += bounds.size.x;
				maxPreviewZ = Mathf.Max(maxPreviewZ, bounds.size.z);
			}
			previewX = -previewX * 0.5f;
			if (i==0) previewZ += maxPreviewZ;
		}
	}

    System.Collections.IEnumerator Screenshot(Texture2D tex, int tileWidth, int tileHeight, int destX, int destY)
    {	
		// Read pixels to buffer location
        yield return new WaitForEndOfFrame();
		tex.ReadPixels(new Rect(0, 0, tileWidth, tileHeight), 0, 0);
        tex.Apply();
		
        // Copy pixels to dest in buffer
		Color background = Camera.main.backgroundColor;
		Color alpha = background; alpha.a = 0.0f;
		for(int y = 0; y < tileHeight; y++)
        for(int x = 0; x < tileWidth; x++)
        {
            Color c = tex.GetPixel(x,y);
            if (c.r == background.r && c.g == background.g && c.b == background.b)
                tex.SetPixel(x + destX, y + destY, alpha);
			else
				tex.SetPixel(x + destX, y + destY, c);
        }
		
		// Clear buffer data
		for(int y = 0; y < tileHeight; y++)
        for(int x = 0; x < tileWidth; x++)
			tex.SetPixel(x,y, alpha);
        
        tex.Apply();
    }
}