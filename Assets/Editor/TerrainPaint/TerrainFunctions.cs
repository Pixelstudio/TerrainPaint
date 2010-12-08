using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

// class containing some helper function to compute terrain data
public static class TerrainFunctions
{
	public static bool CheckTreeDistance( TData terrainData,  Vector3 position, int prototypeIndex, float spacing) {
		foreach (MyTreeInstance tree in terrainData.treeInstances) {
			if (tree.prototypeIndex == prototypeIndex) {
				if ((tree.position - position).magnitude < spacing) {
					return false;
				}
			}
		}
		return true;
	}
	
}

