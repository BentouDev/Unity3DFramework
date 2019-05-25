using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 GetPos(this Matrix4x4 matrix)
    {
        return matrix.GetColumn(3);
    }

    public static Vector3 GetScale(this Matrix4x4 matrix)
    {
        return new Vector3
        (
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );
    }

    public static void SetRotation(this Matrix4x4 matrix, Quaternion newRot)
    {
        matrix.SetTRS(matrix.GetPos(), newRot, matrix.GetScale());
    }

    public static void Decompose(this Matrix4x4 matrix, out Vector3 pos, out Quaternion rot, out Vector3 scl)
    {
        pos = matrix.GetPos();

        rot = matrix.rotation;

        scl = matrix.GetScale();
    }
}

