using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipEntity : MonoBehaviour
{
    public Transform Blocks;
    public string StaticBlockTag;
    public string DynamicBlockTag;

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
            var visibilityData = GetVisibilityDataOfVoxel(block.position, new Vector3(0.25f, 0.25f, 0.25f));
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

        var changedCollisions = int.MaxValue;
        while (changedCollisions > 0)
        {
            changedCollisions = 0;

            foreach (Transform block in Blocks)
            {
                var blockCollider = block.GetComponent<BoxCollider>();
                if (blockCollider == null)
                {
                    continue;
                }

                var rayLength = blockCollider.size / 4;
                var visibilityData = GetVisibilityDataOfVoxel(block.position, rayLength);

                if (!visibilityData.Forward)
                {
                    if (Math.Abs(visibilityData.ForwardCollider.size.x - blockCollider.size.x) < 0.001f)
                    {
                        blockCollider.size += new Vector3(0, 0, visibilityData.ForwardCollider.size.z);
                        blockCollider.center += new Vector3(0, 0, visibilityData.ForwardCollider.size.z / 2);
                        DestroyImmediate(visibilityData.ForwardCollider);

                        changedCollisions++;
                    }
                }

                if (!visibilityData.Right)
                {
                    if (Math.Abs(visibilityData.RightCollider.size.z - blockCollider.size.z) < 0.001f)
                    {
                        blockCollider.size += new Vector3(visibilityData.RightCollider.size.x, 0, 0);
                        blockCollider.center += new Vector3(visibilityData.RightCollider.size.x / 2, 0, 0);
                        DestroyImmediate(visibilityData.RightCollider);

                        changedCollisions++;
                    }
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

    private VisibilityData GetVisibilityDataOfVoxel(Vector3 position, Vector3 dist)
    {
        var data = new VisibilityData();
        data.Up = GetVisibilityDataOfVoxel(position, Vector3.up, dist.y, out data.UpCollider);
        data.Down = GetVisibilityDataOfVoxel(position, Vector3.down, dist.y, out data.DownCollider);
        data.Forward = GetVisibilityDataOfVoxel(position, Vector3.forward, dist.z, out data.ForwardCollider);
        data.Back = GetVisibilityDataOfVoxel(position, Vector3.back, dist.z, out data.BackCollider);
        data.Right = GetVisibilityDataOfVoxel(position, Vector3.right, dist.x, out data.RightCollider);
        data.Left = GetVisibilityDataOfVoxel(position, Vector3.left, dist.x, out data.LeftCollider);

        return data;
    }

    private bool GetVisibilityDataOfVoxel(Vector3 position, Vector3 dir, float dist, out BoxCollider hitCollider)
    {
        if (Physics.Raycast(position, dir, out var hit, dist))
        {
            hitCollider = (BoxCollider)hit.collider;
            return !(hitCollider.gameObject.tag == StaticBlockTag && hitCollider.gameObject.GetComponent<Renderer>().enabled);
        }

        hitCollider = null;
        return true;
    }
}
