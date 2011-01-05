using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerrainPreview 
{
	string previewShader =  " Shader \"Vegetation\" {"+
							" Properties { " +
							" 	_Color (\"Main Color\", Color) = (1, 1, 1, 1)" +
							" 	_MainTex (\"Base (RGB) Alpha (A)\", 2D) = \"white\" {}" +
							" 	_Cutoff (\"Base Alpha cutoff\", Range (0,.9)) = .5" +
							" }" +
							" SubShader {" +
							"	Tags {\"IgnoreProjector\"=\"True\" \"RenderType\"= \"TransparentCutout\"}"+
							" 	Material {" +
							" 		Diffuse [_Color]" +
							" 		Ambient [_Color]" +
							" 		Emission [_Color]" +
							" 	}" +
							" 	Lighting On" +
							" 	Cull Off" +
							" 	Pass {" +
							" 		AlphaTest Greater [_Cutoff]" +
							" 		SetTexture [_MainTex] {" +
							" 			combine texture * primary DOUBLE, texture * primary" +
							" 		}" +
							" 	}" +
							" 	Pass {" +
							" 		ZWrite off" +
							" 		ZTest Less" +
							" 		AlphaTest LEqual [_Cutoff]" +
							" 		Blend SrcAlpha OneMinusSrcAlpha" +
							" 		SetTexture [_MainTex] {	combine texture * primary DOUBLE, texture * primary}" +
							" 	}" +
							" }" +
							" } ";
	
	
	public bool changed = true;
	GameObject previewMesh;
	
	public void Dispose()
    {
        if (previewMesh != null)
        {
            UnityEngine.Object.DestroyImmediate(this.previewMesh);
            previewMesh = null;
        }
    }

	public void UpdatePreviewMesh(List<MyTreeInstance> treeInstances, float size) {
		if ((!changed)|| (previewMesh==null))
			return;
			
		if (previewMesh.GetComponent<MeshFilter>() == null)
			previewMesh.AddComponent<MeshFilter>();

		Mesh mesh = previewMesh.GetComponent<MeshFilter>().sharedMesh;
		if (mesh == null)
			mesh = new Mesh();
		
		mesh.Clear();
		Vector3[] verts = new Vector3[treeInstances.Count * 8];
		int[] triangles = new int[treeInstances.Count * 12];
		Vector2[] uv = new Vector2[verts.Length];
		for (int i = 0; i < treeInstances.Count; i++) {
			Vector3 pos = treeInstances[i].position;
			int vi = i*8;
			verts[vi+0] = (pos + new Vector3(-0.5f,0,0) * size);
			verts[vi+1] = (pos + new Vector3(0.5f,0,0) * size);
			verts[vi+2] = (pos + new Vector3(0.5f,1,0) * size);
			verts[vi+3] = (pos + new Vector3(-0.5f,1,0) * size);
			
			verts[vi+4] = (pos + new Vector3(0,0,-0.5f) * size);
			verts[vi+5] = (pos + new Vector3(0,0,0.5f) * size);
			verts[vi+6] = (pos + new Vector3(0,1,0.5f) * size);
			verts[vi+7] = (pos + new Vector3(0,1,-0.5f) * size);
			
			uv[vi+0] = new Vector2(1,0);
			uv[vi+1] = new Vector2(0,0);
			uv[vi+2] = new Vector2(0,1);
			uv[vi+3] = new Vector2(1,1);
			
			uv[vi+4] = new Vector2(1,0);
			uv[vi+5] = new Vector2(0,0);
			uv[vi+6] = new Vector2(0,1);
			uv[vi+7] = new Vector2(1,1);
			
			int ti = i*12;
			triangles[ti+0] = vi+0;
			triangles[ti+1] = vi+1;
			triangles[ti+2] = vi+2;
			triangles[ti+3] = vi+2;
			triangles[ti+4] = vi+3;
			triangles[ti+5] = vi+0;
			
			triangles[ti+6] = vi+4;
			triangles[ti+7] = vi+5;
			triangles[ti+8] = vi+6;
			triangles[ti+9] = vi+6;
			triangles[ti+10] = vi+7;
			triangles[ti+11] = vi+4;
		}
		
		mesh.vertices = verts;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		previewMesh.GetComponent<MeshFilter>().mesh = mesh;
		changed = false;
	}
	
	public GameObject CreatePreviewmesh(GameObject prefab) {
		previewMesh = EditorUtility.CreateGameObjectWithHideFlags("MeshPreview", HideFlags.HideAndDontSave);
		
		Material m = new Material(previewShader);
		
		Texture2D preview = EditorUtility.GetAssetPreview(prefab);
		Color background = preview.GetPixel(0,0);
		for (int x = 0; x < preview.width; x++) {
			for (int y = 0; y < preview.height; y++) {
				Color c = preview.GetPixel(x,y);
				if (c == background) {
					preview.SetPixel(x,y, new Color(c.r,c.g,c.b,0));
				}
			}
		}
		preview.Apply();
		m.mainTexture = preview;
		
		previewMesh.AddComponent<MeshFilter>();
		MeshRenderer mr = previewMesh.AddComponent<MeshRenderer>();
		mr.receiveShadows = false;
		mr.castShadows = false;
		
		previewMesh.renderer.material = m;
		return previewMesh;
	}
	
	
}

