using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;

[CustomEditor(typeof(TerrainScript))]
public class TerrainPaint : Editor
{
	static string appTitle = "Terrain painting tools v.3.1";
	
	// global variables
	private bool needToSave = false;
	private Texture2D mixMap;
	private Material currentMaterial;
	private GameObject targetObject;
	private Material mat;
	
	// paint variable
    private static Texture2D brushTexture;
	private static float brushSize = 2;
	private static float hardness = 1f;
	private static bool isPainting = false;
	private static bool isTreePainting = false;
	private static bool isTreeRemoving = false;
	private TextureBrush currentTextureBrush;
	private TreeBrush currentTreeBrush;
	private TerrainPreview previewMesh;
	private static int currentSelectedBrush = 0;
	private static int currentSelectedTexture = 1;
	private static int currentSelectedPrototype = 0;
	private Texture2D[] s_BrushTextures;
	private Texture2D[] s_Textures;
	private Texture2D[] s_objectPreviews;
	private GameObject newPrototype;

	
	
	public static int AspectSelectionGrid (int selected, Texture[] textures, int approxSize, GUIStyle style, string emptyString, out bool doubleClick)
	{
		GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.MinHeight (10f) };
		GUILayout.BeginVertical ("box", options);
		doubleClick = false; 
		int num = 0;
		if (textures != null ) {
			if (textures.Length != 0) {
				float num2 = (Screen.width - 20) / approxSize;
				int num3 = (int)Mathf.Ceil (((float)textures.Length) / num2);
				Rect aspectRect = GUILayoutUtility.GetAspectRect (num2 / ((float)num3));
				Event current = Event.current;
				if (((current.type == EventType.MouseDown) && (current.clickCount == 2)) && aspectRect.Contains (current.mousePosition)) {
					doubleClick = true;
					current.Use ();
				}
				num = GUI.SelectionGrid (aspectRect, selected, textures, (Screen.width - 20) / approxSize, style);
			} else {
				GUILayout.Label (emptyString, new GUILayoutOption[0]);
			}
		}
		GUILayout.EndVertical ();
		
		return num;
	}

	private void createMixTexture ()
	{
		if (needToSave) {
			string path = EditorUtility.SaveFilePanel ("Save texture as?", "", "mixTexture.png", "png");
			if (path.Length != 0) {
				mixMap = new Texture2D (256, 256);
				Color[] c = new Color[256 * 256];
				for (int i = 0; i < c.Length; i++) {
					c[i] = new Color (0, 0, 0, 0);
				}
				mixMap.SetPixels (c);
				byte[] data = mixMap.EncodeToPNG ();
				File.WriteAllBytes (path, data);
				AssetDatabase.Refresh (ImportAssetOptions.ForceSynchronousImport);
				path = path.Substring (path.IndexOf ("Asset"));
				mixMap = (Texture2D)AssetDatabase.LoadAssetAtPath (path, typeof(Texture2D));
				TextureImporter textureImporter = AssetImporter.GetAtPath (path) as TextureImporter;
				textureImporter.textureFormat = TextureImporterFormat.ARGB32;
				if (textureImporter.isReadable == false) {
					textureImporter.isReadable = true;
					AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
				}
				currentMaterial.SetTexture ("_Control", mixMap);
				needToSave = false;
			}
		} else {
			if (mixMap != null) {
				string path = AssetDatabase.GetAssetPath (mixMap);
				if (path.Length > 0) {
					byte[] textureBytes = mixMap.EncodeToPNG ();
					File.WriteAllBytes (path, textureBytes);
				}
				AssetDatabase.Refresh ();
			}
		}
	}
	private void checkMixTexture() {
		if (mixMap != null) {
			string p = AssetDatabase.GetAssetPath(mixMap);
			TextureImporter ti = TextureImporter.GetAtPath(p) as TextureImporter;
			ti.textureFormat = TextureImporterFormat.ARGB32;
			ti.isReadable = true;
			AssetDatabase.ImportAsset (p, ImportAssetOptions.ForceUpdate);
		}
						
	}
	private void clearMap ()
	{
		string path = AssetDatabase.GetAssetPath (mixMap);
		Color[] c = new Color[512 * 512];
		for (int i = 0; i < c.Length; i++) {
			c[i] = new Color (0, 0, 0, 0);
		}
		mixMap.SetPixels (c);
		byte[] textureBytes = mixMap.EncodeToPNG ();
		File.WriteAllBytes (path, textureBytes);
		AssetDatabase.Refresh ();
		
	}
	private void LoadTextures ()
	{
		ArrayList list = new ArrayList ();
		list.Add (currentMaterial.GetTexture ("_Splat3") as Texture2D);
		list.Add (currentMaterial.GetTexture ("_Splat2") as Texture2D);
		list.Add (currentMaterial.GetTexture ("_Splat1") as Texture2D);
		list.Add (currentMaterial.GetTexture ("_Splat0") as Texture2D);
		s_Textures = list.ToArray (typeof(Texture2D)) as Texture2D[];
	}
	private void LoadBrushIcons ()
	{
		ArrayList list = new ArrayList ();
		int num = 1;
		Texture texture = null;
		do {
			texture = (Texture)EditorGUIUtility.Load ("Builtin Skins/Brushes/builtin_brush_" + num + ".png");
			if (texture != null) {
				list.Add (texture);
			}
			num++;
		} while (texture != null);
		num = 0;
		do {
			texture = EditorGUIUtility.FindTexture ("brush_" + num + ".png");
			if (texture != null) {
				list.Add (texture);
			}
			num++;
		} while (texture != null);
		s_BrushTextures = list.ToArray (typeof(Texture2D)) as Texture2D[];
	}
	private void LoadPrototypePreview() {
		TerrainScript terrain = ((TerrainScript)target);
		ArrayList list = new ArrayList();
		foreach (MyTreePrototype prototype in terrain.treePrototypes) {
			Texture2D preview = EditorUtility.GetAssetPreview(prototype.prefab);
			preview = EditorUtility.GetAssetPreview(prototype.prefab);
			preview = EditorUtility.GetAssetPreview(prototype.prefab); // hmm perhpas second time better ?
			list.Add(preview);
		}
		s_objectPreviews = list.ToArray (typeof(Texture2D)) as Texture2D[];
	}
	
	private void loadBrush ()
	{
		brushTexture = s_BrushTextures[currentSelectedBrush];
		if (currentTextureBrush != null) 
			currentTextureBrush.Dispose();
		currentTextureBrush = new TextureBrush();
		currentTextureBrush.Load(brushTexture,64);
	}
	private void loadBrush (int index)
	{
		brushTexture = s_BrushTextures[index];
		if (currentTextureBrush != null) 
			currentTextureBrush.Dispose();
		currentTextureBrush = new TextureBrush();
		currentTextureBrush.Load(brushTexture,64);
	}
	
	private void LoadTreeBrush() {
		if (currentTreeBrush == null) {
			currentTreeBrush = new TreeBrush();
		}
		previewMesh = new TerrainPreview();
		if (((TerrainScript)target).treePrototypes.Count > 0)
			previewMesh.CreatePreviewmesh( ((TerrainScript)target).treePrototypes[currentSelectedPrototype].prefab );			
	}
	private void paintTrees() {
		Event e = Event.current;
		RaycastHit hit;
		Ray ray = HandleUtility.GUIPointToWorldRay (e.mousePosition);
		if (Physics.Raycast (ray, out hit)) {
			if (hit.collider.gameObject.name == target.name) {
				if (isTreeRemoving) {
					TreeBrush.RemoveTrees(((TerrainScript)target),hit.point, true);
				}else {
						TreeBrush.PlaceTrees(((TerrainScript)target), hit.point, hit.normal, currentSelectedPrototype);
					
				}
				previewMesh.changed = true;
				EditorUtility.SetDirty(target);
			}
		}

	}
	
	private void paintOnTexture ()
	{
		Event e = Event.current;
		RaycastHit hit;
		Ray ray = HandleUtility.GUIPointToWorldRay (e.mousePosition);
		if (Physics.Raycast (ray, out hit)) {
			if (hit.collider.gameObject.name == target.name) {
				float xCenterNormalized = hit.textureCoord.x;
				float yCenterNormalized = hit.textureCoord.y;
				
				int size = 64 * (int) brushSize;
				int num = Mathf.FloorToInt (xCenterNormalized * mixMap.width);
				int num2 = Mathf.FloorToInt (yCenterNormalized * mixMap.height);
				int num3 = Mathf.RoundToInt ((float)size) / 2;
				int num4 = Mathf.RoundToInt ((float)size) % 2;
				int x = Mathf.Clamp (num - num3, 0, mixMap.width - 1);
				int y = Mathf.Clamp (num2 - num3, 0, mixMap.height - 1);
				int num7 = Mathf.Clamp ((num + num3) + num4, 0, mixMap.width);
				int num8 = Mathf.Clamp ((num2 + num3) + num4, 0, mixMap.height);
				int width = num7 - x;
				int height = num8 - y;
				Color[] srcPixels = mixMap.GetPixels (x, y, width, height, 0);
				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						Color targetColor = currentTextureBrush.GetColor(currentSelectedTexture);
						int ix = (x + j) - ((num - num3) + num4);
						int iy = (y + i) - ((num2 - num3) + num4);
						int index = (i * width) + j;
						float blendFactor = currentTextureBrush.GetStrengthInt (ix / (int)brushSize, iy / (int)brushSize) * hardness;
						srcPixels[index] = Color.Lerp (srcPixels[index], targetColor, blendFactor);
					}
				}
				
				mixMap.SetPixels (x, y, width, height, srcPixels, 0);
				
				
			}
			mixMap.Apply ();
		}
	}
	
	private void UpdatePreviewBrush()
    {
		TerrainScript targetScript = ((TerrainScript) target);
		if (previewMesh == null)
			return;
		
		if (targetScript.treePrototypes.Count >0)
			previewMesh.UpdatePreviewMesh(targetScript.terrainData.treeInstances, targetScript.treePrototypes[0].prefab.renderer.bounds.size.y);
		
		
		Vector3 normal = Vector3.zero;
		Vector3 hitPos = Vector3.zero;
        Vector2 vector;
        Vector3 vector2 = Vector3.zero;
		float m_Size = 16 * brushSize;
        Projector previewProjector = currentTextureBrush.GetPreviewProjector();
        float num = 1f;
     	
		float num2 = mixMap.width / mixMap.height;
		
		Vector2 size = targetScript.getSizeOfMesh();// terrain.getSizeOfMesh();
		
		int terrainSizeX = (int)size.x;
		int terrainSizeZ = (int)size.y; 
        
		Transform PPtransform = previewProjector.transform;
        bool flag = true;
		
		Vector2 newMousePostion = Event.current.mousePosition;
		newMousePostion.y = Screen.height - (Event.current.mousePosition.y + 35);
		Ray ray = Camera.current.ScreenPointToRay( newMousePostion );
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit,1000f)) {
			vector2 = hit.point;
			hitPos = hit.point;
			normal = hit.normal;
			float num4 = ((m_Size % 2) != 0) ? 0.5f : 0f; 
            int alphamapWidth = 64;
            int alphamapHeight = 64;
            vector.x = (Mathf.Floor(hit.textureCoord.x * alphamapWidth) + num4) / ((float) alphamapWidth);
            vector.y = (Mathf.Floor(hit.textureCoord.y * alphamapHeight) + num4)  / ((float) alphamapHeight);
            vector2.x = vector.x * -terrainSizeX + (terrainSizeX/2);
            vector2.z = vector.y * -terrainSizeZ + (terrainSizeZ/2);
            vector2 +=  Selection.activeGameObject.transform.position;
            num = ((m_Size * 0.5f) / ((float) alphamapWidth)) * terrainSizeX;
            num2 = ((float) alphamapWidth) / ((float) alphamapHeight);
		} else {
			flag = false;	
		}
		
		previewProjector.enabled = flag;
        if (flag)
        {
			PPtransform.position = hitPos + (normal * 100);
			PPtransform.rotation = Quaternion.LookRotation( normal );
		}
        previewProjector.orthographicSize = num / num2;
        previewProjector.aspectRatio = num2;
  	} 
	
	private void GUI_Textures ()
	{
		GUILayout.Label ("Textures", EditorStyles.boldLabel, new GUILayoutOption[0]);
		bool flag;
		currentSelectedTexture = AspectSelectionGrid (currentSelectedTexture, s_Textures, 0x40, "gridlist", "No textures defined.",  out flag);
		if (flag) {
			
		}
	}
	private void GUI_Brushes ()
	{
		GUILayout.Label ("Brushes", EditorStyles.boldLabel, new GUILayoutOption[0]);
		bool flag;
		if (s_BrushTextures == null) 
			LoadTextures();
		currentSelectedBrush = AspectSelectionGrid (currentSelectedBrush, s_BrushTextures, 0x20, "gridlist", "No brushes defined.",  out flag);
		if (flag) {
			loadBrush ();
		}
	}
	private void GUI_Settings ()
	{
		GUILayout.Label ("Settings", EditorStyles.boldLabel, new GUILayoutOption[0]);
		EditorGUIUtility.LookLikeControls (100f, 90f);
		brushSize = (int)EditorGUILayout.IntSlider("brushSize",(int) brushSize, 1, 5);
		hardness = EditorGUILayout.Slider ("brushHardness", hardness, 0.01f, 1f);
		
		
	}
	
	private void GUI_PrototypePreview() {
		bool flag;
		if (s_objectPreviews == null) 
			LoadPrototypePreview();
		
		currentSelectedPrototype = AspectSelectionGrid (currentSelectedPrototype, s_objectPreviews, 0x20, "gridlist", "No brushes defined.",  out flag);
		if (flag) {
			TreeBrush.selectedTree = currentSelectedPrototype;
		}	
	}
	private void GUI_Prototype() {
		
		newPrototype = (GameObject) EditorGUILayout.ObjectField("Add prototype :",newPrototype,typeof(GameObject));
		if (newPrototype != null) {
			if (GUILayout.Button("Add Treeprototype")) {
				
				string absolutePath  = Application.dataPath + "/TerrainPrefabs/";
				if (!Directory.Exists(absolutePath)) {
					Directory.CreateDirectory(absolutePath);
				}
				string relativePath = "Assets/TerrainPrefabs/TreePrototype" + target.name + ((TerrainScript)target).treePrototypes.Count +1  + ".prefab";
				UnityEngine.Object newObj = EditorUtility.CreateEmptyPrefab(relativePath);
				GameObject pref =  EditorUtility.ReplacePrefab(newPrototype,newObj,ReplacePrefabOptions.Default);
				pref.AddComponent<LOD>();
				((TerrainScript)target).AddTreePrototype(pref);
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
				LoadPrototypePreview();
			}
		}
		
		if (GUILayout.Button("clear")) {
			((TerrainScript)target).treePrototypes.Clear();
			LoadPrototypePreview();
			
		}
			
				
	}
	private void GUI_Trees() {
		GUILayout.Label("Number of trees :" + ((TerrainScript)target).terrainData.treeInstances.Count);
		if (currentTreeBrush!= null) {
			TreeBrush.paintOnNormals = EditorGUILayout.Toggle("Use normals?", TreeBrush.paintOnNormals);
			TreeBrush.brushSize = EditorGUILayout.Slider("Brushsize", TreeBrush.brushSize,1f, 100f);
			brushSize = TreeBrush.brushSize/10;
			TreeBrush.spacing = EditorGUILayout.Slider("Spacing", TreeBrush.spacing,0.1f, 10f);
			TreeBrush.treeColorAdjustment = EditorGUILayout.Slider("Color adjustment",TreeBrush.treeColorAdjustment,0.1f, 1f);
			TreeBrush.treeHeight = EditorGUILayout.Slider("Height",TreeBrush.treeHeight,0.2f, 20f);
			TreeBrush.treeHeightVariation = EditorGUILayout.Slider("Height variation",TreeBrush.treeHeightVariation,0.0f, 1f);
			TreeBrush.treeWidth = EditorGUILayout.Slider("Width",TreeBrush.treeWidth,1f, 3f);
			TreeBrush.treeWidthVariation = EditorGUILayout.Slider("Width variation", TreeBrush.treeWidthVariation,0.0f, 1f);
			TreeBrush.treeRotationVariation = EditorGUILayout.Slider("Rotation variation", TreeBrush.treeRotationVariation,0.0f, 360f);
		}
		
		if (GUILayout.Button("Clear trees")) {
			((TerrainScript)target).terrainData.ClearTrees();
			previewMesh.changed = true;
			EditorUtility.SetDirty(target);
		}
	}
	
	public override void OnInspectorGUI ()
	{
		
		GUILayout.Label (appTitle, EditorStyles.boldLabel);
		GUILayout.Label ("martijn.pixelstudio@gmail.com", EditorStyles.boldLabel);
		if (isTreePainting) {
			GUI_Prototype();
			GUI_PrototypePreview();
			GUI_Trees();
			if (GUILayout.Button("Stop placing trees")) {
				isTreePainting = false;
				if (previewMesh != null) 
					previewMesh.Dispose();
			}
				
		} else {
			if (GUILayout.Button("Edit Trees")) {
				LoadBrushIcons ();
				loadBrush(0);
				LoadTreeBrush();
				LoadPrototypePreview();
				isTreePainting = true;	
				isPainting = false;
			}
		}
		
		if (!isTreePainting) {
			GUILayout.Label("Textures");
			currentMaterial = targetObject.renderer.sharedMaterial;

			if (currentMaterial.shader.name == "TerrainPaint/Lightmap-FirstPass") {
				if (isPainting) {
					GUI_Brushes ();
					GUI_Textures ();
					GUI_Settings ();
					if (GUILayout.Button ("Clear mixmap")) {
						clearMap ();
					}
					if (GUILayout.Button ("Stop painting")) {
						if (currentTextureBrush!= null)
							currentTextureBrush.Dispose();
						createMixTexture ();
						isPainting = false;
					}
				} else {
					if (GUILayout.Button ("Start painting (ctrl/cmd + click)")) {
						mixMap = currentMaterial.GetTexture ("_Control") as Texture2D;
						checkMixTexture();
						LoadBrushIcons ();
						loadBrush ();
						LoadTextures ();
						if (mixMap == null) {
							needToSave = true;
						} else {
							needToSave = false;
						}
						isPainting = true;
					}
					
					mixMap = currentMaterial.GetTexture ("_Control") as Texture2D;
					if (mixMap == null) {
						createMixTexture();
					}				
				}
				
			} else {
				GUILayout.Label ("Shader type is not correct! unable to paint");
			}
		
		}
	}
	
	void OnRenderObject () {
		if (mat == null)
			mat = new Material(Shader.Find("Diffuse"));
		
		foreach (MyTreeInstance t in ((TerrainScript)target).terrainData.treeInstances) {
			Mesh m = ((TerrainScript)target).treePrototypes[t.prototypeIndex].prefab.GetComponent<MeshFilter>().sharedMesh;
			Material mm = ((TerrainScript)target).treePrototypes[t.prototypeIndex].prefab.renderer.sharedMaterial;
			Graphics.DrawMesh(m, t.position, t.rotation, mm, 0);		
		}
		
		
	}

	void DisableProjector() {
		if (currentTextureBrush!= null) {
			currentTextureBrush.GetPreviewProjector().enabled = false;	
		}
	}
	
	void OnSceneGUI ()
	{
		int ctrlID = GUIUtility.GetControlID (appTitle.GetHashCode (), FocusType.Passive);
		
		if (Input.GetKeyDown(KeyCode.LeftShift))
			isTreeRemoving = true;
		else 
			isTreeRemoving = false;
		
		if ((isPainting) || (isTreePainting)) {
			switch (Event.current.type) 
			{
				case EventType.mouseDrag:
					if (Event.current.control || Event.current.command) {
						if (Event.current.shift)
							isTreeRemoving = true;
						else 
							isTreeRemoving = false;
						if (isTreePainting) paintTrees();
						if (isPainting) paintOnTexture ();
						Event.current.Use ();
					}
					break;
				case EventType.layout:
					HandleUtility.AddDefaultControl (ctrlID);
					break;
	            case EventType.MouseMove:
        	        HandleUtility.Repaint();
                	break;
            	case EventType.Repaint:
                	if (isPainting) DisableProjector();
                	break;
			}
			if (currentTextureBrush != null)
				UpdatePreviewBrush();
		}
		
		//OnRenderObject();
	}
	
	void OnEnable() {
		targetObject = Selection.activeGameObject;	
	}
	
	void OnDisable() {
		createMixTexture();
		isPainting = false;
		isTreePainting = false;
		if (currentTextureBrush != null){
			currentTextureBrush.Dispose();	
		}
		
		if (previewMesh != null) {
			previewMesh.Dispose();
		}
		
	}
	
	public bool Raycast(out Vector2 uv, out Vector3 pos)
	{
	    RaycastHit hit;
	    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
	    if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
	    {
	        uv = hit.textureCoord;
	        pos = hit.point;
	        return true;
	    }
	    uv = Vector2.zero;
	    pos = Vector3.zero;
	    return false;
	}

 

 

	
	
	
}
