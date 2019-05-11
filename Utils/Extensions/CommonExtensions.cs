using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CommonExtensions
{
    public static object GetDefault(this System.Type type)
    {
        return type.IsValueType ? System.Activator.CreateInstance(type) : null;
    }
    
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

    public static T GetRandom<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list)  
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public static Framework.UniqueQueue<T> EnqueueRange<T>(this Framework.UniqueQueue<T> queue, IEnumerable<T> range)
    {
        foreach (T @object in range)
        {
            queue.Enqueue(@object);
        }

        return queue;
    }
        
    public static Queue<T> EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> range)
    {
        foreach (T @object in range)
        {
            queue.Enqueue(@object);
        }

        return queue;
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
    
    public static void Resize<T>(this List<T> list, int sz, T c = default(T))
    {
        int cur = list.Count;
        if (sz < cur)
            list.RemoveRange(sz, cur - sz);
        else if (sz > cur)
            list.AddRange(Enumerable.Repeat(c, sz - cur));
    }
}
