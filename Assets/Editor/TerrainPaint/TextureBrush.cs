using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class TextureBrush
{
    // Fields
    internal const int kMinBrushSize = 3;
    private Texture2D m_Brush;
    private Projector m_BrushProjector;
    private Texture2D m_Preview;
    private int m_Size;
    private float[] m_Strength;

    // Methods
    private void CreatePreviewBrush()
    {
        Type[] components = new Type[] { typeof(Projector) };
		GameObject obj2 = EditorUtility.CreateGameObjectWithHideFlags("TerrainPaintrBrushPreview", HideFlags.HideAndDontSave, components);
        this.m_BrushProjector = obj2.GetComponent(typeof(Projector)) as Projector;
        this.m_BrushProjector.enabled = false;
        this.m_BrushProjector.nearClipPlane = -1000f;
        this.m_BrushProjector.farClipPlane = 1000f;
        this.m_BrushProjector.orthographic = true;
        this.m_BrushProjector.orthographicSize = 10f;
        this.m_BrushProjector.transform.Rotate((float) 90f, 0f, (float) 180f);
        Material material = new Material("Shader \"Hidden/Terrain Brush Preview\" {\nProperties {\n\t_MainTex (\"Main\", 2D) = \"gray\" { TexGen ObjectLinear }\n\t_CutoutTex (\"Cutout\", 2D) = \"black\" { TexGen ObjectLinear }\n}\nSubshader {\n\tZWrite Off\n\tOffset -1, -1\n\tFog { Mode Off }\n\tAlphaTest Greater 0\n\tColorMask RGB\n\tPass\n\t{\n\t\tBlend SrcAlpha OneMinusSrcAlpha\n\t\tSetTexture [_MainTex]\n\t\t{\n\t\t\tconstantColor (.2,.7,1,.5)\n\t\t\tcombine constant, texture * constant\n\t\t\tMatrix [_Projector]\n\t\t}\n\n\t\tSetTexture [_CutoutTex]\n\t\t{\n\t\t\tcombine previous, previous * texture\n\t\t\tMatrix [_Projector]\n\t\t}\n\t}\n}\n}");
        material.shader.hideFlags = HideFlags.HideAndDontSave;
        material.hideFlags = HideFlags.HideAndDontSave;
        material.SetTexture("_CutoutTex", (Texture2D) EditorGUIUtility.Load("Builtin Skins/Brushes/brush_cutout.png"));
        this.m_BrushProjector.material = material;
        this.m_BrushProjector.enabled = false;
    }

    public void Dispose()
    {
        if (this.m_BrushProjector != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_BrushProjector.material.shader);
            UnityEngine.Object.DestroyImmediate(this.m_BrushProjector.material);
            UnityEngine.Object.DestroyImmediate(this.m_BrushProjector.gameObject);
            this.m_BrushProjector = null;
        }
        UnityEngine.Object.DestroyImmediate(this.m_Preview);
        this.m_Preview = null;
    }

    public Projector GetPreviewProjector()
    {
        return this.m_BrushProjector;
    }

    public float GetStrengthInt(int ix, int iy)
    {
        ix = Mathf.Clamp(ix, 0, this.m_Size - 1);
        iy = Mathf.Clamp(iy, 0, this.m_Size - 1);
        return this.m_Strength[(iy * this.m_Size) + ix];
    }
	
	public Color GetColor(int i) {
	
		
		i = Mathf.Clamp(i,0,3);
		if (i == 0)
			return ( new Color (0, 0, 0, 1));
		if (i == 1)
			return ( new Color (0, 0, 1, 0));
		if (i == 2)
			return ( new Color (0, 1, 0, 0));
		if (i == 3)
			return ( new Color (1, 0, 0, 0));
		return Color.black;
	}
	
    public bool Load(Texture2D brushTex, int size)
    {
        if (((this.m_Brush == brushTex) && (size == this.m_Size)) && (this.m_Strength != null))
        {
            return true;
        }
        if (brushTex != null)
        {
            float num = size;
            this.m_Size = size;
            this.m_Strength = new float[this.m_Size * this.m_Size];
            if (this.m_Size > 3)
            {
                for (int j = 0; j < this.m_Size; j++)
                {
                    for (int k = 0; k < this.m_Size; k++)
                    {
                        this.m_Strength[(j * this.m_Size) + k] = brushTex.GetPixelBilinear((k + 0.5f) / num, ((float) j) / num).a;
                    }
                }
            }
            else
            {
                for (int m = 0; m < this.m_Strength.Length; m++)
                {
                    this.m_Strength[m] = 1f;
                }
            }
            UnityEngine.Object.DestroyImmediate(this.m_Preview);
            this.m_Preview = new Texture2D(this.m_Size, this.m_Size, TextureFormat.ARGB32, false);
            this.m_Preview.hideFlags = HideFlags.HideAndDontSave;
            this.m_Preview.wrapMode = TextureWrapMode.Repeat;
            this.m_Preview.filterMode = FilterMode.Point;
            Color[] colors = new Color[this.m_Size * this.m_Size];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(1f, 1f, 1f, this.m_Strength[i]);
            }
            this.m_Preview.SetPixels(0, 0, this.m_Size, this.m_Size, colors, 0);
            this.m_Preview.Apply();
            if (this.m_BrushProjector == null)
            {
                this.CreatePreviewBrush();
            }
            this.m_BrushProjector.material.mainTexture = this.m_Preview;
            this.m_Brush = brushTex;
            return true;
        }
        this.m_Strength = new float[] { 1f };
        this.m_Size = 1;
        return false;
    }
	
}

 

