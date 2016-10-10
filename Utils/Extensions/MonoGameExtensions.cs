using UnityEngine;
using System.Collections;

static public class MonoBehaviourExtenions {
	/// <summary>
	/// Gets or add a component. Usage example:
	/// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T> (this Component child) where T: Component {
		T result = child.GetComponent<T>();
		if (result == null) {
			result = child.gameObject.AddComponent<T>();
		}
		return result;
	}

    static public void SafeDestroy(this MonoBehaviour mono, GameObject obj)
    {
        var pooled = obj.GetComponent<PooledObject>();
        if (pooled)
        {
            pooled.ReturnToPool();
        }
        else
        {
            MonoBehaviour.Destroy(obj);
        }
    }
}