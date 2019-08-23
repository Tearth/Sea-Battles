using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ShipEntity : MonoBehaviour, ISelectable
{
    public Transform Blocks;
    public Transform Chunks;
    public Transform Cannons;
    public Transform Keel;
    public Rigidbody ShipRigidbody;
    public GameObject ChunkPrefab;
    public GameObject DynamicBlockPrefab;
    public LayerMask StaticBlocksLayer;
    public int ChunkWidth;
    public float VoxelSize;
    public float WavesForce;
    public float WavesFrequency;
    public float StabilizationForce;
    public float MaxStabilizationForce;
    public float BuoyancyForce;
    public float SpeedAffectRatio;
    public float SpeedWavesFrequency;
    public float MoveForwardForce;
    public float TurnForce;
    public float TurnSwingForce;

    public int CannonsCount;
    public int CrewCount;

    public bool Selected { get; set; }

    private Vector3Int _shipSize;
    private Vector3 _shipCorner;
    private bool[,,] _shipMap;

    private float _wavesSwingAngle;
    private float _speedSwingAngle;

    void Start()
    {
        (_shipSize, _shipCorner) = GetShipSizeAndCorner();

        CreateShipArrayMap();
        RegenerateShipMesh();
        CalculateVariables();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Transform cannon in Cannons)
            {
                cannon.GetComponent<CannonEntity>().Shoot();
            }
        }
    }

    void FixedUpdate()
    {
        var speedForce = ShipRigidbody.velocity.sqrMagnitude * SpeedAffectRatio;

        // Add buoyancy force (more submerged = more force)
        if (Keel.position.y < 0)
        {
            ShipRigidbody.AddForce(0, BuoyancyForce * Mathf.Abs(Keel.position.y), 0, ForceMode.Acceleration);
        }

        // Add swing (right-left) force due to waves
        var torqueAngle = Mathf.Sin(_wavesSwingAngle);
        var torqueAngleSign = Mathf.Sign(torqueAngle);

        ShipRigidbody.AddRelativeTorque(WavesForce * torqueAngle + torqueAngleSign * speedForce, 0, 0, ForceMode.Acceleration);
        _wavesSwingAngle = (_wavesSwingAngle + WavesFrequency) % (2 * Mathf.PI);

        // Add swing (frond-back) force due to speed
        var speedAngle = Mathf.Sin(_speedSwingAngle);
        var speedAngleSign = Mathf.Sign(speedAngle);

        ShipRigidbody.AddRelativeTorque(0, 0, speedAngleSign * torqueAngle * speedForce, ForceMode.Acceleration);
        _speedSwingAngle = (_speedSwingAngle + SpeedWavesFrequency) % (2 * Mathf.PI);

        // Stabilize ship's swing
        ShipRigidbody.AddRelativeTorque(
            Mathf.DeltaAngle(transform.eulerAngles.x, 0) * StabilizationForce, 
            0,
            Mathf.DeltaAngle(transform.eulerAngles.z, 0) * StabilizationForce, 
            ForceMode.Acceleration);

        MoveForward();
        TurnLeft();
    }

    private void CreateShipArrayMap()
    {
        _shipMap = new bool[_shipSize.x, _shipSize.y, _shipSize.z];
        foreach (Transform block in Blocks)
        {
            var blockArrayCoords = GetArrayCoordsOfBlock(block.position);
            _shipMap[blockArrayCoords.x, blockArrayCoords.y, blockArrayCoords.z] = true;
        }
    }

    private void RegenerateShipMesh()
    {
        for (var x = 0; x < _shipSize.x; x += ChunkWidth)
        {
            RegenerateChunk(x);
        }

        SimplifyColliders();

        foreach (Transform block in Blocks)
        {
            block.GetComponent<Renderer>().enabled = false;
            if (block.GetComponent<BoxCollider>() == null)
            {
                Destroy(block.gameObject);
            }

            Destroy(block.GetComponent<MeshFilter>());
            Destroy(block.GetComponent<MeshRenderer>());
        }

        gameObject.AddComponent<MeshCollider>().convex = true;
    }

    private void CalculateVariables()
    {
        CannonsCount = 0;
        CrewCount = 0;

        foreach (Transform cannon in Cannons)
        {
            var cannonEntity = cannon.GetComponent<CannonEntity>();

            CannonsCount++;
            CrewCount += cannonEntity.CrewCount;
        }
    }

    private void RegenerateChunk(int x)
    {
        var chunk = Chunks.Find($"Chunk {x}")?.gameObject;
        if (chunk == null)
        {
            chunk = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.identity, Chunks);
            chunk.name = $"Chunk {x}";
        }

        var meshFilter = chunk.GetComponent<MeshFilter>();
        var generator = new MeshGenerator(VoxelSize);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        var squareCount = 0;
        
        for (var chunkX = x; chunkX < x + ChunkWidth; chunkX++)
        {
            if (chunkX >= _shipSize.x)
            {
                break;
            }

            for (var y = 0; y < _shipSize.y; y++)
            {
                for (var z = 0; z < _shipSize.z; z++)
                {
                    if (!_shipMap[chunkX, y, z])
                    {
                        continue;
                    }

                    var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                    var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

                    if (y == _shipSize.y - 1 || !_shipMap[chunkX, y + 1, z])
                    {
                        generator.GenerateTopFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }

                    if (y == 0 || !_shipMap[chunkX, y - 1, z])
                    {
                        generator.GenerateBottomFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }

                    if (chunkX == _shipSize.x - 1 || !_shipMap[chunkX + 1, y, z])
                    {
                        generator.GenerateRightFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }

                    if (chunkX == 0 || !_shipMap[chunkX - 1, y, z])
                    {
                        generator.GenerateLeftFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }

                    if (z == _shipSize.z - 1 || !_shipMap[chunkX, y, z + 1])
                    {
                        generator.GenerateFrontFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }

                    if (z == 0 || !_shipMap[chunkX, y, z - 1])
                    {
                        generator.GenerateBackFace(centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;
                    }
                }
            }
        }

        meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.uv = uv.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

    private void SimplifyColliders()
    {
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

                var rayLength = blockCollider.size * VoxelSize;
                var visibilityData = GetVisibilityDataOfVoxel(block.position, rayLength);


                if (!visibilityData.Right && blockCollider.size.x >= blockCollider.size.z)
                {
                    if (Math.Abs(visibilityData.RightCollider.size.z - blockCollider.size.z) < 0.001f)
                    {
                        blockCollider.size += new Vector3(visibilityData.RightCollider.size.x, 0, 0);
                        blockCollider.center += new Vector3(visibilityData.RightCollider.size.x / 2, 0, 0);
                        DestroyImmediate(visibilityData.RightCollider);

                        changedCollisions++;
                    }
                }
                else if (!visibilityData.Forward && blockCollider.size.x <= blockCollider.size.z)
                {
                    if (Math.Abs(visibilityData.ForwardCollider.size.x - blockCollider.size.x) < 0.001f)
                    {
                        blockCollider.size += new Vector3(0, 0, visibilityData.ForwardCollider.size.z);
                        blockCollider.center += new Vector3(0, 0, visibilityData.ForwardCollider.size.z / 2);
                        DestroyImmediate(visibilityData.ForwardCollider);

                        changedCollisions++;
                    }
                }
            }
        }
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
        if (Physics.Raycast(position, dir, out var hit, dist, StaticBlocksLayer.value))
        {
            hitCollider = (BoxCollider)hit.collider;
            return !(hitCollider.gameObject.GetComponent<Renderer>().enabled);
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

        var xSize = Mathf.RoundToInt((x.max - x.min) * VoxelSize);
        var ySize = Mathf.RoundToInt((y.max - y.min) * VoxelSize);
        var zSize = Mathf.RoundToInt((z.max - z.min) * VoxelSize);

        return (new Vector3Int(xSize + 1, ySize + 1, zSize + 1), new Vector3(x.min, y.min, z.min));
    }

    private Vector3Int GetArrayCoordsOfBlock(Vector3 position)
    {
        var offset = (position - _shipCorner) / VoxelSize;
        return new Vector3Int(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y), Mathf.RoundToInt(offset.z));
    }

    private Vector3 GetRealBlockPositionByArray(Vector3Int arrayCoords)
    {
        return (Vector3)arrayCoords * VoxelSize + _shipCorner;
    }

    public void DeleteVoxel(Vector3 position)
    {
        var locPoint = transform.InverseTransformPoint(position);

        var voxelArrayCoords = GetArrayCoordsOfBlock(locPoint);
        var targetX = voxelArrayCoords.x - (voxelArrayCoords.x % ChunkWidth);

        _shipMap[voxelArrayCoords.x, voxelArrayCoords.y, voxelArrayCoords.z] = false;

        if (targetX > ChunkWidth - 1)
        {
            RegenerateChunk(targetX - ChunkWidth);
        }

        RegenerateChunk(targetX);

        if (targetX < _shipSize.x - ChunkWidth)
        {
            RegenerateChunk(targetX + ChunkWidth);
        }
    }

    public void DeleteCollider(Vector3 position, BoxCollider collider, Collision collision, ColliderType type)
    {
        // Distance to the center of collider
        var dist = Vector3.Distance(collider.center, position);
        var originalSize = collider.size;

        // Check if we have long or wide collider
        switch (type)
        {
            // Long collider
            case ColliderType.ForwardBack:
            {
                if (position.x < collider.center.x)
                {
                    // Before:
                    //
                    //    hit    center 
                    //    \/       \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // oooo [                   ]
                    // 

                    collider.size = new Vector3(collider.size.x / 2 - dist - 0.5f, collider.size.y, collider.size.z);
                    collider.center = new Vector3(collider.size.x / 2 - 0.5f, collider.center.y, collider.center.z);
                }
                else
                {
                    //           center    hit
                    //             \/      \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // ooooooooooooo [          ]
                    // 

                    var diff = collider.center.x - dist;

                    collider.size = new Vector3(collider.size.x - diff - 1, collider.size.y, collider.size.z);
                    collider.center = new Vector3(collider.size.x / 2 - 0.5f, collider.center.y, collider.center.z);
                }

                // Create new collider in the blank space (on the right)
                var sizeOfNewCollider = originalSize - new Vector3(collider.size.x + 1, 0, 0);
                if (sizeOfNewCollider.x > 0)
                {
                    var rightGameObject = Instantiate(collider.gameObject, Vector3.zero, Quaternion.identity, Blocks);
                    var rightCollider = rightGameObject.GetComponent<BoxCollider>();

                    rightCollider.transform.position = collider.transform.TransformPoint(position + new Vector3(1, 0, 0));
                    rightCollider.size = sizeOfNewCollider;
                    rightCollider.center = new Vector3(rightCollider.size.x / 2 - 0.5f, 0, 0);
                }

                // If left collider has length equal to zero, remove it
                if (collider.size.x <= 0)
                {
                    Destroy(collider.gameObject);
                }

                break;
            }

            // Wide collider
            case ColliderType.RightLeft:
            {
                if (position.z < collider.center.z)
                {
                    // Before:
                    //
                    //    hit    center 
                    //    \/       \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // oooo [                   ]
                    // 

                    collider.size = new Vector3(collider.size.x, collider.size.y, collider.size.z / 2 - dist - 0.5f);
                    collider.center = new Vector3(collider.center.x, collider.center.y, collider.size.z / 2 - 0.5f);
                }
                else
                {
                    //           center    hit
                    //             \/      \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // ooooooooooooo [          ]
                    // 

                    var diff = collider.center.z - dist;

                    collider.size = new Vector3(collider.size.x, collider.size.y, collider.size.z - diff - 1);
                    collider.center = new Vector3(collider.center.x, collider.center.y, collider.size.z / 2 - 0.5f);
                }

                // Create new collider in the blank space (on the right)
                var sizeOfNewCollider = originalSize - new Vector3(0, 0, collider.size.z + 1);
                if (sizeOfNewCollider.z > 0)
                {
                    var rightGameObject = Instantiate(collider.gameObject, Vector3.zero, Quaternion.identity, Blocks);
                    var rightCollider = rightGameObject.GetComponent<BoxCollider>();

                    rightCollider.transform.position = collider.transform.TransformPoint(position + new Vector3(0, 0, 1));
                    rightCollider.size = sizeOfNewCollider;
                    rightCollider.center = new Vector3(0, 0, rightCollider.size.z / 2 - 0.5f);
                }

                // If left collider has length equal to zero, remove it
                if (collider.size.z <= 0)
                {
                    Destroy(collider.gameObject);
                }

                break;
            }
        }

        var dynamicBlock = Instantiate(DynamicBlockPrefab, collider.transform.TransformPoint(position), Quaternion.identity, Blocks);
        dynamicBlock.GetComponent<Rigidbody>().velocity = collision.relativeVelocity;
    }

    public void MoveForward()
    {
        ShipRigidbody.AddRelativeForce(MoveForwardForce, 0, 0, ForceMode.Acceleration);
    }

    public void TurnRight()
    {
        ShipRigidbody.AddRelativeTorque(TurnSwingForce, TurnForce, 0, ForceMode.Acceleration);
    }

    public void TurnLeft()
    {
        ShipRigidbody.AddTorque(0, -TurnForce, 0, ForceMode.Acceleration);
        ShipRigidbody.AddRelativeTorque(-TurnSwingForce, 0, 0, ForceMode.Acceleration);
    }
}
