using UnityEngine;
using System.Collections;
using System;
using System.Threading;
enum LODState {
	High,
	Medium,
	Low
}

public class LOD : MonoBehaviour {
	MeshFilter mf;
	bool hasSubmeshes = false;
	public Mesh LODMesh_High;
	public Mesh LODMesh_Medium;
	public Mesh LODMesh_Low;
	LODState state = LODState.High;
	
	void Awake() {
		mf = GetComponent<MeshFilter>();
		if (mf.mesh.subMeshCount > 0) {
			hasSubmeshes = true;
		} else {
			hasSubmeshes = false;
		}
		
		StartCoroutine(CheckForLOD());	
	}
	
	public void CreateLODMeshes() {
		if (mf == null)
			mf = GetComponent<MeshFilter>();
		LODMesh_High = mf.sharedMesh;
		MeshSimplify.Simplify(mf.sharedMesh,mf.sharedMesh.triangles.Length/2,out LODMesh_Medium);
		MeshSimplify.Simplify(mf.sharedMesh,mf.sharedMesh.triangles.Length/6,out LODMesh_Low);
	}
	
	IEnumerator CheckForLOD() {
		while(true) {
			yield return new WaitForSeconds(0.5f);
			bool changed = false;
		
			if (Camera.main != null) {
				float distance =Mathf.Abs ((transform.position - Camera.main.transform.position).magnitude); 
				if (distance < 30) {
					if ((state != LODState.High)) {
						mf.mesh = LODMesh_High;
						state = LODState.High;
						changed = true;
					}
				} else if (distance < 50) {
					if (state!= LODState.Medium) {
						if (hasSubmeshes) {
							for (int i = 0; i < mf.mesh.subMeshCount-1; i++) 
							{
								mf.mesh.SetTriangles(LODMesh_Medium.GetTriangles(i),i);	
							}
						} else { 
							mf.mesh = LODMesh_Medium;	
						}
						state = LODState.Medium;
						changed = true;
					}
				} else {
					if (state != LODState.Low) {
						if (hasSubmeshes) {
							for (int i = 0; i < mf.mesh.subMeshCount-1; i++) {
								mf.mesh.SetTriangles(LODMesh_Low.GetTriangles(i),i);
						}
						
						} else { 
							mf.mesh = LODMesh_Low;	
						}
						state = LODState.Low;
						changed = true;
					}
				}
				
			}
			
			if (changed) {
				//mf.mesh.RecalculateNormals();
				//mf.mesh.RecalculateBounds();
			}
		}
	}
	
	
	
}



