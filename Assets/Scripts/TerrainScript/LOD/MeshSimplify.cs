using UnityEngine;
using System;
using System.Runtime.InteropServices;

// using external plugin for creating lod objects.
// compiler error....
public class MeshSimplify
{
    // Methods
    public static bool Simplify(Mesh Mesh, int WantedTriangleCount, out Mesh OutputMesh)
    {
        OutputMesh = null;
        float[] inVertices = new float[Mesh.vertexCount * 3];
        for (int i = 0; i < Mesh.vertexCount; i++)
        {
            inVertices[i * 3] = Mesh.vertices[i].x;
            inVertices[(i * 3) + 1] = Mesh.vertices[i].y;
            inVertices[(i * 3) + 2] = Mesh.vertices[i].z;
        }
        float[] inUVCoordinates = new float[Mesh.vertexCount * 2];
        for (int j = 0; j < Mesh.vertexCount; j++)
        {
            inUVCoordinates[j * 2] = Mesh.uv[j].x;
            inUVCoordinates[(j * 2) + 1] = Mesh.uv[j].y;
        }
        float[] inNormals = new float[Mesh.vertexCount * 3];
        for (int k = 0; k < Mesh.vertexCount; k++)
        {
            inNormals[k * 3] = Mesh.normals[k].x;
            inNormals[(k * 3) + 1] = Mesh.normals[k].y;
            inNormals[(k * 3) + 2] = Mesh.normals[k].z;
        }
        float[] inTangents = new float[Mesh.vertexCount * 3];
        for (int m = 0; m < Mesh.vertexCount; m++)
        {
            inTangents[m * 3] = Mesh.tangents[m].x;
            inTangents[(m * 3) + 1] = Mesh.tangents[m].y;
            inTangents[(m * 3) + 2] = Mesh.tangents[m].z;
        }
        float[] inBinormals = new float[Mesh.vertexCount * 3];
        for (int n = 0; n < Mesh.vertexCount; n++)
        {
            Vector3 vector = Vector3.Cross(Mesh.normals[n], Mesh.tangents[n]) * Mesh.tangents[n].w;
            inBinormals[n * 3] = vector.x;
            inBinormals[(n * 3) + 1] = vector.y;
            inBinormals[(n * 3) + 2] = vector.z;
        }
        IntPtr outVertices = Marshal.AllocCoTaskMem(0);
        IntPtr outNormals = Marshal.AllocCoTaskMem(0);
        IntPtr outTangents = Marshal.AllocCoTaskMem(0);
        IntPtr outTriangles = Marshal.AllocCoTaskMem(0);
        IntPtr outUVCoordinates = Marshal.AllocCoTaskMem(0);
        int outNumVertices = 0;
        int outNumTriangles = 0;
        if (!SimplifyMesh(inVertices, inNormals, inTangents, inBinormals, Mesh.vertexCount, Mesh.triangles, Mesh.triangles.Length / 3, inUVCoordinates, WantedTriangleCount, ref outVertices, ref outNormals, ref outTangents, ref outNumVertices, ref outTriangles, ref outNumTriangles, ref outUVCoordinates))
        {
            Debug.LogError("Simplify mesh failed");
            return false;
        }
        OutputMesh = new Mesh();
        OutputMesh.name = Mesh.name;
        Vector3[] vectorArray = new Vector3[outNumVertices];
        for (int num8 = 0; num8 < outNumVertices; num8++)
        {
            vectorArray[num8] = (Vector3) Marshal.PtrToStructure(new IntPtr(outVertices.ToInt32() + (num8 * Marshal.SizeOf(typeof(Vector3)))), typeof(Vector3));
        }
        Vector3[] vectorArray2 = new Vector3[outNumVertices];
        for (int num9 = 0; num9 < outNumVertices; num9++)
        {
            vectorArray2[num9] =(Vector3)  Marshal.PtrToStructure(new IntPtr(outNormals.ToInt32() + (num9 * Marshal.SizeOf(typeof(Vector3)))), typeof(Vector3));
        }
        Vector4[] vectorArray3 = new Vector4[outNumVertices];
        for (int num10 = 0; num10 < outNumVertices; num10++)
        {
            Vector3 vector2 = (Vector3) Marshal.PtrToStructure(new IntPtr(outTangents.ToInt32() + (num10 * Marshal.SizeOf(typeof(Vector3)))), typeof(Vector3));
            vectorArray3[num10] = new Vector4(vector2.x, vector2.y, vector2.z, 1f);
        }
        Vector2[] vectorArray4 = new Vector2[outNumVertices];
        for (int num11 = 0; num11 < outNumVertices; num11++)
        {
            vectorArray4[num11] = (Vector2) Marshal.PtrToStructure(new IntPtr(outUVCoordinates.ToInt32() + (num11 * Marshal.SizeOf(typeof(Vector2)))), typeof(Vector2));
        }
        int[] numArray6 = new int[outNumTriangles * 3];
        Marshal.Copy(outTriangles, numArray6, 0, 3 * outNumTriangles);
        OutputMesh.vertices = vectorArray;
        OutputMesh.uv = vectorArray4;
        OutputMesh.normals = vectorArray2;
        OutputMesh.tangents = vectorArray3;
        OutputMesh.triangles = numArray6;
		
        return true;
    }

    [DllImport("DXMeshCombine")]
    private static extern bool SimplifyMesh(float[] InVertices, float[] InNormals, float[] InTangents, float[] InBinormals, int InNumVertices, int[] InTriangles, int InNumTriangles, float[] InUVCoordinates, int WantedTriangleCount, ref IntPtr OutVertices, ref IntPtr OutNormals, ref IntPtr OutTangents, ref int OutNumVertices, ref IntPtr OutTriangles, ref int OutNumTriangles, ref IntPtr OutUVCoordinates);
}
