using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class TreeBrush
{
    // Fields
    public static float brushSize = 40f;
    public static int selectedTree;
    public static float spacing = 0.8f;
    public static float treeColorAdjustment = 0.4f;
    public static float treeHeight = 1f;
    public static float treeHeightVariation = 0.1f;
    public static float treeWidth = 1f;
    public static float treeWidthVariation = 0.1f;
	public static float treeRotationVariation = 0f;
	public static bool paintOnNormals = true;
    // Methods
    private static Color GetTreeColor()
    {
        Color color = Color.white * UnityEngine.Random.Range((float) 1f, (float) (1f - treeColorAdjustment));
        color.a = 1f;
        return color;
    }

    private static float GetTreeHeight()
    {
        return (treeHeight * UnityEngine.Random.Range((float) (1f - treeHeightVariation), (float) (1f + treeHeightVariation)));
    }
	
	private static Quaternion GetTreeRotation(Vector3 direction) 
	{
		if (paintOnNormals)
			return (Quaternion.FromToRotation(Vector3.up,direction) * Quaternion.AngleAxis(UnityEngine.Random.Range((float) (1f - treeRotationVariation), (float) (1f + treeRotationVariation)),direction)) ;
		else
			return Quaternion.AngleAxis(UnityEngine.Random.Range((float) (1f - treeRotationVariation), (float) (1f + treeRotationVariation)),Vector3.up);
	}
	
    private float GetTreePlacementSize(float treeCount)
    {
        //TODO create new function
		//return TerrainInspectorUtil.GetTreePlacementSize(terrain.terrainData, selectedTree, spacing, treeCount);
		return 1f;
    }

    private static float GetTreeWidth()
    {
        return (treeWidth * UnityEngine.Random.Range((float) (1f - treeWidthVariation), (float) (1f	 + treeWidthVariation)));
    }

    public static void PlaceTrees(TerrainScript terrain, Vector3 position, Vector3 normal, int selectedTree)
    {
		
        if (terrain.treePrototypes.Count != 0)
        {
			if (true)
			{
				
                int num = 0;
                MyTreeInstance instance = new MyTreeInstance();
                instance.position = position;
				instance.rotation = GetTreeRotation(normal);
                instance.color = GetTreeColor();
                instance.lightmapColor = Color.white;
                instance.prototypeIndex = selectedTree;
                instance.widthScale = GetTreeWidth();
                instance.heightScale = GetTreeHeight();
				if (((Event.current.type != EventType.MouseDrag) && (brushSize <= 1f)) || TerrainFunctions.CheckTreeDistance(terrain.terrainData, instance.position, instance.prototypeIndex, spacing))
               	{
			     	terrain.AddTreeInstance(instance);
			        num++;
                }
                
				Vector3 prototypeExtent = Vector3.one;
			    prototypeExtent.y = 0f;
                float num2 = brushSize / ((prototypeExtent.magnitude * spacing) * 0.5f);
                int num3 =(int) ((num2 * num2) * 0.5f);
                num3 = Mathf.Clamp(num3, 0, 100);
                for (int i = 1; (i < num3) && (num < num3); i++)
                {
                    Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
                    insideUnitCircle.x *= (brushSize*100) / terrain.getSizeOfMesh().x;
                    insideUnitCircle.y *= (brushSize*100) / terrain.getSizeOfMesh().y;
				
					Vector3 off = new Vector3(insideUnitCircle.x, 0, insideUnitCircle.y);
					Vector3 pos = Vector3.Cross(normal, off);
					Vector3 position2 = position + pos;
					Vector3 nom = position2 - (position2 + normal*10);
					
					Ray ray = new Ray(position2, nom);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit)) {
						
					//if (terrain.isInsideOfBounds(position2)) {
						if (true) {
							if (TerrainFunctions.CheckTreeDistance(terrain.terrainData, hit.point, instance.prototypeIndex, spacing)) 
							{
		                        instance = new MyTreeInstance();
		                        instance.position = hit.point;
								instance.rotation = GetTreeRotation(hit.normal);
		                        instance.color = GetTreeColor();
		                        instance.lightmapColor = Color.white;
		                        instance.prototypeIndex = selectedTree;
		                        instance.widthScale = GetTreeWidth();
		                        instance.heightScale = GetTreeHeight();
		                        terrain.AddTreeInstance(instance);
		                        num++;
		                    }
						}
					}
                
                }
				
            }
        }
    }
	
    public static void RemoveTrees(TerrainScript terrain, Vector3 position, bool clearSelectedOnly)
    {
        float radius = brushSize /2;
        terrain.RemoveTrees(position, radius, !clearSelectedOnly ? -1 : selectedTree);
    }
}

 
 
