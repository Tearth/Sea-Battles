using System;
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
            var blockCollider = block.GetComponent<BoxCollider>();

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
            else
            {
                if (blockCollider != null && Math.Abs(visibilityData.RightCollider.size.z - blockCollider.size.z) < 0.001f)
                {
                    blockCollider.size += new Vector3(visibilityData.RightCollider.size.x, 0, 0);
                    blockCollider.center += new Vector3(visibilityData.RightCollider.size.x / 2, 0, 0);
                    DestroyImmediate(visibilityData.RightCollider);
                }
            }

            if (visibilityData.Left)
            {
                generator.GenerateLeftFace(centerOffset, vertices, triangles, uv, squareCount);
                squareCount++;
            }
            else
            {
                if (blockCollider != null && Math.Abs(visibilityData.LeftCollider.size.z - blockCollider.size.z) < 0.001f)
                {
                    visibilityData.LeftCollider.size += new Vector3(blockCollider.size.x, 0, 0);
                    visibilityData.LeftCollider.center += new Vector3(blockCollider.size.x / 2, 0, 0);
                    DestroyImmediate(blockCollider);
                }
            }
        }

        foreach (Transform block in Blocks)
        {
            block.GetComponent<Renderer>().enabled = false;
            if (block.GetComponent<BoxCollider>() == null)
            {
                Destroy(block.gameObject);
            }
        }

        meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.uv = uv.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

    private VisibilityData GetVisibilityDataOfVoxel(Vector3 position)
    {
        var data = new VisibilityData();
        data.Up = GetVisibilityDataOfVoxel(position, Vector3.up, out data.UpCollider);
        data.Down = GetVisibilityDataOfVoxel(position, Vector3.down, out data.DownCollider);
        data.Forward = GetVisibilityDataOfVoxel(position, Vector3.forward, out data.ForwardCollider);
        data.Back = GetVisibilityDataOfVoxel(position, Vector3.back, out data.BackCollider);
        data.Right = GetVisibilityDataOfVoxel(position, Vector3.right, out data.RightCollider);
        data.Left = GetVisibilityDataOfVoxel(position, Vector3.left, out data.LeftCollider);

        return data;
    }
    
    private bool GetVisibilityDataOfVoxel(Vector3 position, Vector3 dir, out BoxCollider hitCollider)
    {
        if (Physics.Raycast(position, dir, out var hit, 0.25f))
        {
            hitCollider = (BoxCollider)hit.collider;
            return !(hitCollider.gameObject.tag == "Block" && hitCollider.gameObject.GetComponent<Renderer>().enabled);
        }

        hitCollider = null;
        return true;
    }
}
