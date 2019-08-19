using System.Collections.Generic;
using UnityEngine;

public class ShipEntity : MonoBehaviour
{
    public Transform Blocks;

    void Start()
    {
        RegenerateShipMesh();
    }

    void Update()
    {
        
    }

    private void RegenerateShipMesh()
    {
        var generator = new MeshGenerator();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();

        var meshFilter = GetComponent<MeshFilter>();
        var squareCount = 0;

        foreach (Transform block in Blocks)
        {
            var visibilityData = GetVisibilityDataOfVoxel(block.position);
            var centerOffset = block.position - new Vector3(0.25f, 0.25f, 0.25f) / 2;

            if (visibilityData.Up)
            {
                generator.GenerateTopFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Down)
            {
                generator.GenerateBottomFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Forward)
            {
                generator.GenerateFrontFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Back)
            {
                generator.GenerateBackFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Right)
            {
                generator.GenerateRightFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Left)
            {
                generator.GenerateLeftFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }
        }

        foreach (Transform block in Blocks)
        {
            block.GetComponent<Renderer>().enabled = false;
        }

        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.uv = uv.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

    private VisibilityData GetVisibilityDataOfVoxel(Vector3 position)
    {
        return new VisibilityData
        {
            Up = GetVisibilityDataOfVoxel(position, Vector3.up),
            Down = GetVisibilityDataOfVoxel(position, Vector3.down),
            Forward = GetVisibilityDataOfVoxel(position, Vector3.forward),
            Back = GetVisibilityDataOfVoxel(position, Vector3.back),
            Right = GetVisibilityDataOfVoxel(position, Vector3.right),
            Left = GetVisibilityDataOfVoxel(position, Vector3.left)
        };
    }

    private bool GetVisibilityDataOfVoxel(Vector3 position, Vector3 dir)
    {
        if (Physics.Raycast(position, dir, out var hit, 0.25f))
        {
            var hitGameObject = hit.collider.gameObject;
            return !(hitGameObject.tag == "Block" && hitGameObject.GetComponent<Renderer>().enabled);
        }

        return true;
    }
}
