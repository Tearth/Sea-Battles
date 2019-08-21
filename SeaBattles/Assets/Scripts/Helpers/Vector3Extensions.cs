using System;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Sign(this Vector3 vector)
    {
        return new Vector3(Mathf.Sign(vector.x), Mathf.Sign(vector.y), Math.Sign(vector.z));
    }
}