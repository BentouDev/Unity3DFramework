using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabPool : MonoBehaviour
{
    private static Dictionary<string, PrefabPool> pools = new Dictionary<string, PrefabPool>();

    public GameObject Prefab;
    public int Count;
    public string Name;

    private List<GameObject> instances;
    private int freeListEnd;
    private int freeListBegin;

    void OnDestroy()
    {
        if (pools.ContainsKey(Name))
        {
            pools.Remove(Name);
        }
    }

    public static PrefabPool GetPool(string name)
    {
        if (pools.ContainsKey(name))
            return pools[name];
        else
        {
            return null;
        }
    }
    
    void Start()
    {
        if (Prefab)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Name = Prefab.name;
            }

            if (pools.ContainsKey(name))
            {
                Debug.LogError("Pool " + Name + " already exists!");
            }
            else
            {
                pools[Name] = this;
                InitializeInstances();
            }
        }
    }

    private void InitializeInstances()
    {
        instances = new List<GameObject>(Count);
        for (int i = 0; i < Count; i++)
        {
            var go = Instantiate(Prefab) as GameObject;
            go.AddComponent<PooledObject>().SetPool(this);
            go.transform.SetParent(transform);
            go.SetActive(false);
            instances.Add(go);
        }
    }

    public GameObject GetObject()
    {
        if (instances.Count > 0)
        {
            GameObject obj = instances[0];
            instances.RemoveAt(0);
            return obj;
        }
        return null;
    }
    
    public void ReturnObject(GameObject obj)
    {
        instances.Add(obj);
        obj.SetActive(false);
    }

    public void ClearPool()
    {

    }
}
