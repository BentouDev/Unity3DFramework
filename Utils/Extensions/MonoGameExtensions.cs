using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MonoBehaviourExtenions
{
    public static void TryInit<T>(this Component mono, ref T comp) where T : Component
    {
        if (comp == null)
            comp = mono.GetComponentInChildren<T>();
    }

    public static void InitList<T>(this Component mono, ref List<T> list)
    {
        if (list == null)
            list = new List<T>();
        else
            list.Clear();
    }

    public static T FindInWorld<T>(this Component mono, string tag) where T : Component
    {
        return GameObject.FindGameObjectWithTag(tag)?.GetComponent<T>();
    }
    
    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    public static T GetOrAddComponent<T> (this Component child) where T: Component
    {
		T result = child.GetComponent<T>() ?? child.gameObject.AddComponent<T>();
        return result;
	}

    public static void SafeDestroy(this Component mono, GameObject obj)
    {
        var pooled = obj.GetComponent<PooledObject>();
        if (pooled)
        {
            pooled.ReturnToPool();
        }
        else
        {
            Component.Destroy(obj);
        }
    }
}
