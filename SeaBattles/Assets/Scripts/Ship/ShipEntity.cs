using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ShipEntity : MonoBehaviour, ISelectable
{
    public Transform Blocks;
    public Transform Cannons;
    public Transform Keel;
    public Rigidbody ShipRigidbody;
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
    private CombineInstance[] _chunks;

    private float _wavesSwingAngle;
    private float _speedSwingAngle;

    void Start()
    {
        (_shipSize, _shipCorner) = GetShipSizeAndCorner();
        _chunks = new CombineInstance[_shipSize.x / ChunkWidth];

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
        for (var x = 0; x < _shipSize.x / ChunkWidth; x++)
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

        CombineMeshes();
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
        if (_chunks[x].mesh == null)
        {
            _chunks[x].mesh = new Mesh();
            _chunks[x].transform = UnityEngine.Matrix4x4.TRS(-transform.position, Quaternion.identity, Vector3.one);
        }

        var generator = new MeshGenerator(VoxelSize);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        var squareCount = 0;

        for (var chunkX = x * ChunkWidth; chunkX <= x * ChunkWidth + ChunkWidth; chunkX++)
        {
            for (var y = 0; y < _shipSize.y; y++)
            {
                /////////////////////////////////////////////////////////////
                // Top face
                /////////////////////////////////////////////////////////////
                for (var z = 0; z < _shipSize.z; z++)
                {
                    var blocksCount = 0;
                    for (var existingBlockIndex = z; existingBlockIndex < _shipSize.z; existingBlockIndex++)
                    {
                        if (_shipMap[chunkX, y, existingBlockIndex] &&
                            (y == _shipSize.y - 1 || !_shipMap[chunkX, y + 1, existingBlockIndex]))
                        {
                            blocksCount++;
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (blocksCount > 0)
                    {
                        var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                        var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

                        generator.GenerateTopFace(blocksCount, centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;

                        z += blocksCount - 1;
                    }
                }

                /////////////////////////////////////////////////////////////
                // Bottom face
                /////////////////////////////////////////////////////////////
                for (var z = 0; z < _shipSize.z; z++)
                {
                    var blocksCount = 0;
                    for (var existingBlockIndex = z; existingBlockIndex < _shipSize.z; existingBlockIndex++)
                    {
                        if (_shipMap[chunkX, y, existingBlockIndex] && (y == 0 || !_shipMap[chunkX, y - 1, existingBlockIndex]))
                        {
                            blocksCount++;
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (blocksCount > 0)
                    {
                        var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                        var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

                        generator.GenerateBottomFace(blocksCount, centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;

                        z += blocksCount - 1;
                    }
                }

                /////////////////////////////////////////////////////////////
                // Right face
                /////////////////////////////////////////////////////////////
                for (var z = 0; z < _shipSize.z; z++)
                {
                    var blocksCount = 0;

                    for (var existingBlockIndex = z; existingBlockIndex < _shipSize.z; existingBlockIndex++)
                    {
                        if (_shipMap[chunkX, y, existingBlockIndex] &&
                            (chunkX == _shipSize.x - 1 || !_shipMap[chunkX + 1, y, existingBlockIndex]))
                        {
                            blocksCount++;
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (blocksCount > 0)
                    {
                        var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                        var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

                        generator.GenerateRightFace(blocksCount, centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;

                        z += blocksCount - 1;
                    }
                }

                /////////////////////////////////////////////////////////////
                // Left face
                /////////////////////////////////////////////////////////////
                for (var z = 0; z < _shipSize.z; z++)
                {
                    var blocksCount = 0;
                    for (var existingBlockIndex = z; existingBlockIndex < _shipSize.z; existingBlockIndex++)
                    {
                        if (_shipMap[chunkX, y, existingBlockIndex] && (chunkX == 0 || !_shipMap[chunkX - 1, y, existingBlockIndex]))
                        {
                            blocksCount++;
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (blocksCount > 0)
                    {
                        var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                        var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

                        generator.GenerateLeftFace(blocksCount, centerOffset, vertices, triangles, uv, squareCount);
                        squareCount++;

                        z += blocksCount - 1;
                    }
                }

                for (var z = 0; z < _shipSize.z; z++)
                {
                    if (!_shipMap[chunkX, y, z])
                    {
                        continue;
                    }

                    var realCoords = GetRealBlockPositionByArray(new Vector3Int(chunkX, y, z));
                    var centerOffset = realCoords - new Vector3(VoxelSize, VoxelSize, VoxelSize) / 2;

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
        Debug.Log(saved);
        _chunks[x].mesh.Clear();
        _chunks[x].mesh.vertices = vertices.ToArray();
        _chunks[x].mesh.triangles = triangles.ToArray();
        _chunks[x].mesh.uv = uv.ToArray();
        _chunks[x].mesh.RecalculateNormals();
    }

    private void CombineMeshes()
    {
        GetComponent<MeshFilter>().mesh.CombineMeshes(_chunks);
        GetComponent<MeshFilter>().mesh.Optimize();
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

                block.GetComponent<BlockEntity>().ShipEntity = this;

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

    public void AddDynamicVoxel(Vector3 position, Vector3 velocity)
    {
        var dynamicBlock = Instantiate(DynamicBlockPrefab, position, Quaternion.identity, Blocks);
        dynamicBlock.GetComponent<Rigidbody>().velocity = velocity;
    }

    public void DeleteVoxel(Vector3 position)
    {
        var locPoint = transform.InverseTransformPoint(position);

        var voxelArrayCoords = GetArrayCoordsOfBlock(locPoint);
        var voxelXInChunk = voxelArrayCoords.x % ChunkWidth;
        var targetX = voxelArrayCoords.x - voxelXInChunk;

        _shipMap[voxelArrayCoords.x, voxelArrayCoords.y, voxelArrayCoords.z] = false;

        if (voxelXInChunk == 0 && targetX > ChunkWidth - 1)
        {
            RegenerateChunk(targetX - 1);
        }

        RegenerateChunk(targetX);

        if (voxelXInChunk == ChunkWidth - 1 && targetX < _shipSize.x - ChunkWidth)
        {
            RegenerateChunk(targetX + 1);
        }

        CombineMeshes();
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
