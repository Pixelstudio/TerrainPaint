using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[Serializable]
public class TData
{
	public TData() {

	}
	
	public List<MyTreeInstance> treeInstances = new List<MyTreeInstance>();
	
	public void AddTree(MyTreeInstance instance) {
		treeInstances.Add(instance);
	}
	public void RemoveTrees(Vector3 position,float radius, int treeType) {
		List<MyTreeInstance> toRemove = new List<MyTreeInstance>();
		for (int i = 0; i < treeInstances.Count; i++) {
			Vector3 treePos = treeInstances[i].position;
			if ((treePos-position).magnitude < radius) {
					toRemove.Add(treeInstances[i]);
			}
		}
		foreach (MyTreeInstance tree in toRemove) {
			treeInstances.Remove(tree);	
		}
		toRemove = null;
	}
	public void ClearTrees() {
		treeInstances.Clear();
	}
	
}

public class TerrainScript : MonoBehaviour {
	
	[SerializeField]
	public List<MyTreePrototype> treePrototypes = new List<MyTreePrototype>();
	
	private Vector2 SizeOfMesh;
	
	[SerializeField]
	public TData terrainData;
	
	void Start() {
		if (treePrototypes != null) {
			foreach (MyTreePrototype prototype in treePrototypes) {
				prototype.prefab.GetComponent<LOD>().CreateLODMeshes();	
			}
		}
		if (terrainData != null) {
			foreach (MyTreeInstance t in terrainData.treeInstances) {
				GameObject newObj = (GameObject) GameObject.Instantiate(treePrototypes[t.prototypeIndex].prefab,t.position,Quaternion.identity );
				newObj.renderer.material.color = t.color;
				newObj.transform.localScale = new Vector3(t.widthScale,t.heightScale,t.widthScale);
				newObj.transform.localRotation =  t.rotation ;//Quaternion.AngleAxis(UnityEngine.Random.Range(0,360) ,Vector3.up);
				
			}
		}
	}
	
	public Vector2 getSizeOfMesh() {
		if (SizeOfMesh == Vector2.zero) {
			MeshFilter mf = gameObject.GetComponent<MeshFilter>();
			Vector2 result = Vector2.zero;
			if (mf != null) {
				result.x = mf.sharedMesh.bounds.size.x;
			 	result.y = mf.sharedMesh.bounds.size.y;
			}
			SizeOfMesh = result;	
		}
		
		return SizeOfMesh;
	}
	
	public bool isInsideOfBounds(Vector3 position) {
		Ray ray = new Ray(new Vector3(position.x, 10f,position.z), Vector3.down);
		if (Physics.Raycast(ray))
			return true;
		
		return false;
		
	}
	
	public void AddTreePrototype(GameObject prefab) {
		MyTreePrototype newTree = new MyTreePrototype(prefab);
		treePrototypes.Add(newTree);
	}
	
	public void AddTreeInstance(MyTreeInstance tree) {
		terrainData.AddTree(tree);
	}
	
	public void RemoveTrees(Vector3 position,float radius, int treeType) {
		terrainData.RemoveTrees(position,radius,treeType);	
	}
	 /*
	void OnDrawGizmos()
    {
		if (terrainData != null) {
	    	foreach (MyTreeInstance t in terrainData.treeInstances) {
				Gizmos.DrawWireSphere(t.position,1f );
			}
		} 
    }
    */
	
}

[Serializable]
public class MyTreeInstance
{
	[SerializeField]
    private Quaternion m_Rotation;
	[SerializeField]
    private Vector3 m_Position;
    [SerializeField]
	private float m_WidthScale;
    [SerializeField]
	private float m_HeightScale;
    [SerializeField]
	private Color m_Color;
    [SerializeField]
	private Color m_LightmapColor;
    [SerializeField]
	private int m_Index;
 	 public Quaternion rotation
    {
        get
        {
            return this.m_Rotation;
        }
        set
        {
            this.m_Rotation = value;
        }
    }
	public Vector3 position
    {
        get
        {
            return this.m_Position;
        }
        set
        {
            this.m_Position = value;
        }
    }
    public float widthScale
    {
        get
        {
            return this.m_WidthScale;
        }
        set
        {
            this.m_WidthScale = value;
        }
    }
    public float heightScale
    {
        get
        {
            return this.m_HeightScale;
        }
        set
        {
            this.m_HeightScale = value;
        }
    }
    public Color color
    {
        get
        {
            return this.m_Color;
        }
        set
        {
            this.m_Color = value;
        }
    }
    public Color lightmapColor
    {
        get
        {
            return this.m_LightmapColor;
        }
        set
        {
            this.m_LightmapColor = value;
        }
    }
    public int prototypeIndex
    {
        get
        {
            return this.m_Index;
        }
        set
        {
            this.m_Index = value;
        }
    }
   
}

[Serializable]
public class MyTreePrototype {
	public GameObject prefab;
	
	//constructors
	public MyTreePrototype ( GameObject prefab) {
		this.prefab = prefab;
	}
}



