using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{

    Mesh mesh;

    private static int xSize = 140;
    private static int ySize = 80;
    Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
    int[] triangles = new int[xSize * ySize * 6];



    // Use this for initialization
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Renderer mr = GetComponent<Renderer>();
        mr.material.color = Color.white;

        CreateTriangle();
        UpdateMesh();


    }


    void CreateTriangle()
    {

        for (int i = 0, y = -ySize/2; y <= ySize/2; y++)
        {
            for (int x = -xSize/2; x <= xSize/2; x++)
            {
                vertices[i] = new Vector3((float)x, (float)y, 0);
                i++;
            }
        }


        int vert = 0;
        int tri = 0;

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tri + 0] = vert + 0;
                triangles[tri + 1] = vert + xSize + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xSize + 1;
                triangles[tri + 5] = vert + xSize + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }



    void UpdateMesh()
    {

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //    {
    //        return;
    //    }

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], 0.1f);
    //    }
    //}
}
