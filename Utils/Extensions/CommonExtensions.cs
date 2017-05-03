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
}
