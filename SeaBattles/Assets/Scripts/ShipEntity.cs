using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ShipEntity : MonoBehaviour
{
    public Transform Blocks;
    public string StaticBlockTag;
    public string DynamicBlockTag;

    private Vector3Int _shipSize;
    private Vector3 _shipCorner;
    private bool[,,] _shipMap;

    void Start()
    {
        (_shipSize, _shipCorner) = GetShipSizeAndCorner();
        RegenerateShipMesh();
    }

    void Update()
    {
        
    }

    private void RegenerateShipMesh()
    {
        _shipMap = new bool[_shipSize.x, _shipSize.y, _shipSize.z];

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

            var blockArrayCoords = GetArrayCoordsOfBlock(block.position);
            _shipMap[blockArrayCoords.x, blockArrayCoords.y, blockArrayCoords.z] = true;
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

    private (Vector3Int size, Vector3 corner) GetShipSizeAndCorner()
    {
        (float min, float max) x = (float.MaxValue, float.MinValue);
        (float min, float max) y = (float.MaxValue, float.MinValue);
        (float min, float max) z = (float.MaxValue, float.MinValue);

        foreach (Transform block in Blocks)
        {
            x.min = Mathf.Min(x.min, block.position.x);
            y.min = Mathf.Min(y.min, block.position.y);
            z.min = Mathf.Min(z.min, block.position.z);

            x.max = Mathf.Max(x.max, block.position.x);
            y.max = Mathf.Max(y.max, block.position.y);
            z.max = Mathf.Max(z.max, block.position.z);
        }

        var xSize = Mathf.RoundToInt((x.max - x.min) * 4);
        var ySize = Mathf.RoundToInt((y.max - y.min) * 4);
        var zSize = Mathf.RoundToInt((z.max - z.min) * 4);

        return (new Vector3Int(xSize + 1, ySize + 1, zSize + 1), new Vector3(x.min, y.min, z.min));
    }

    private Vector3Int GetArrayCoordsOfBlock(Vector3 position)
    {
        var offset = (position - _shipCorner) * 4;
        return new Vector3Int(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y), Mathf.RoundToInt(offset.z));
    }
}
