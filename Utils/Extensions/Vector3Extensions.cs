using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static void Decompose(this Matrix4x4 matrix, out Vector3 pos, out Quaternion rot, out Vector3 scl)
    {
        // Extract new local position
        pos = matrix.GetColumn(3);
 
        // Extract new local rotation
//        rot = Quaternion.LookRotation(
//            matrix.GetColumn(2),
//            matrix.GetColumn(1)
//        );

        rot = matrix.rotation;

        // Extract new local scale
        scl = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );
    }
}

