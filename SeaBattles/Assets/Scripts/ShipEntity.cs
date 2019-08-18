using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityData
{
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Forward { get; set; }
    public bool Back { get; set; }
    public bool Right { get; set; }
    public bool Left { get; set; }
}

public class ShipEntity : MonoBehaviour
{
    public Transform Blocks;

    void Start()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();

        foreach (Transform block in Blocks)
        {
            block.GetComponent<Renderer>().enabled = false;

            var visibilityData = GetVisibilityData(block.position);
            var squareCount = vertices.Count / 4;
            if (visibilityData.Up)
            {
                GenerateTopFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Down)
            {
                GenerateBottomFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Forward)
            {
                GenerateFrontFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Back)
            {
                GenerateBackFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Right)
            {
                GenerateRightFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }

            if (visibilityData.Left)
            {
                GenerateLeftFace(block.position, vertices, triangles, uv, squareCount);
                squareCount++;
            }
        }

        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.uv = uv.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

    void Update()
    {
        
    }

    private VisibilityData GetVisibilityData(Vector3 position)
    {
        return new VisibilityData
        {
            Up = !Physics.Raycast(position, Vector3.up, 0.25f),
            Down = !Physics.Raycast(position, Vector3.down, 0.25f),
            Forward = !Physics.Raycast(position, Vector3.forward, 0.25f),
            Back = !Physics.Raycast(position, Vector3.back, 0.25f),
            Right = !Physics.Raycast(position, Vector3.right, 0.25f),
            Left = !Physics.Raycast(position, Vector3.left, 0.25f)
        };
    }

    private void GenerateTopFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z + 0.25f));

        AddUvForSquare(uv, 0);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void GenerateBottomFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));

        AddUvForSquare(uv, 1);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void GenerateFrontFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));

        AddUvForSquare(uv, 2);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void GenerateBackFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z));

        AddUvForSquare(uv, 3);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void GenerateRightFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));

        AddUvForSquare(uv, 4);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void GenerateLeftFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z + 0.25f));

        AddUvForSquare(uv, 5);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    private void AddTrianglesForSquare(List<int> triangles, int squareNumber)
    {
        triangles.Add(squareNumber * 4);
        triangles.Add(squareNumber * 4 + 3);
        triangles.Add(squareNumber * 4 + 2);
        triangles.Add(squareNumber * 4 + 2);
        triangles.Add(squareNumber * 4 + 1);
        triangles.Add(squareNumber * 4);
    }

    private void AddUvForSquare(List<Vector2> uv, int squareNumber)
    {
        var UvWidthStep = 1;
        var UvHeightPerType = 1;
        var MaxTextureTypesCount = 1;
        var TextureType = 0;

        uv.Add(new Vector2(UvWidthStep * squareNumber, UvHeightPerType * (int)(MaxTextureTypesCount - 1 - TextureType)));
        uv.Add(new Vector2(UvWidthStep * (squareNumber + 1), UvHeightPerType * (int)(MaxTextureTypesCount - 1 - TextureType)));
        uv.Add(new Vector2(UvWidthStep * (squareNumber + 1), UvHeightPerType * ((int)(MaxTextureTypesCount - 1 - TextureType) + 1)));
        uv.Add(new Vector2(UvWidthStep * squareNumber, UvHeightPerType * ((int)(MaxTextureTypesCount - 1 - TextureType) + 1)));
    }
}
