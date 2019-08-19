using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public void GenerateTopFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z + 0.25f));

        AddUvForSquare(uv, 0);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateBottomFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));

        AddUvForSquare(uv, 1);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateFrontFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));

        AddUvForSquare(uv, 2);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateBackFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));
        vertices.Add(new Vector3(position.x, position.y + 0.25f, position.z));

        AddUvForSquare(uv, 3);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateRightFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
    {
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z));
        vertices.Add(new Vector3(position.x + 0.25f, position.y, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z + 0.25f));
        vertices.Add(new Vector3(position.x + 0.25f, position.y + 0.25f, position.z));

        AddUvForSquare(uv, 4);
        AddTrianglesForSquare(triangles, squareNumber);
    }

    public void GenerateLeftFace(Vector3 position, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int squareNumber)
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