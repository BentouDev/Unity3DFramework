using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonExtensions
{
    public static T Next<T>(this T src) where T : struct, System.IConvertible
    {
        if (!typeof(T).IsEnum)
            throw new System.ArgumentException (
                string.Format("Argumnent {0} is not an Enum", 
                typeof(T).FullName)
            );

        T[] Arr = (T[])System.Enum.GetValues(src.GetType());
        int j = System.Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static List<T> GetFrom<T>(this List<T> list, int index)
    {
        index = Mathf.Clamp(index, 0, list.Count - 1);
        if (index + 1 > list.Count - 1)
            return new List<T>();

        return list.GetRange(index + 1, list.Count - 1 - index);
    }

    public static List<T> GetTo<T>(this List<T> list, int index)
    {
        return list.GetRange(0, Mathf.Clamp(index, 0, list.Count - 1));
    }
}
