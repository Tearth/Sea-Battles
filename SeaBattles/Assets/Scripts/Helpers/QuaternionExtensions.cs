using UnityEngine;

public static class QuaternionExtensions
{
    public static Vector3 ToVector3(this Quaternion quaternion)
    {
        return new Vector3(quaternion.x, quaternion.y, quaternion.z);
    }
}