using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    private float _size;
    
    public MeshGenerator(float size)
    {
        _size = size;
    }

    public void GenerateTopFace(float length, Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y + _size, position.z));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z + length));
        vertices.Add(new Vector3(position.x, position.y + _size, position.z + length));

        AddUvForSquare(uv, 0);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateBottomFace(float length, Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + _size, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z + length));
        vertices.Add(new Vector3(position.x + _size, position.y, position.z + length));

        AddUvForSquare(uv, 1);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateFrontFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + _size, position.y, position.z + _size));
        vertices.Add(new Vector3(position.x, position.y, position.z + _size));
        vertices.Add(new Vector3(position.x, position.y + _size, position.z + _size));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z + _size));

        AddUvForSquare(uv, 2);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateBackFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x + _size, position.y, position.z));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z));
        vertices.Add(new Vector3(position.x, position.y + _size, position.z));

        AddUvForSquare(uv, 3);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateRightFace(float length, Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + _size, position.y, position.z));
        vertices.Add(new Vector3(position.x + _size, position.y, position.z + length));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z + length));
        vertices.Add(new Vector3(position.x + _size, position.y + _size, position.z));

        AddUvForSquare(uv, 4);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateLeftFace(float length, Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y, position.z + length));
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y + _size, position.z));
        vertices.Add(new Vector3(position.x, position.y + _size, position.z + length));

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
        uv.Add(new Vector2(squareNumber, 0));
        uv.Add(new Vector2(squareNumber + 1, 0));
        uv.Add(new Vector2(squareNumber + 1, 1));
        uv.Add(new Vector2(squareNumber, 1));
    }
}