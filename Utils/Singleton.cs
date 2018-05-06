using UnityEngine;
using System.Collections;

public interface ISingletonInstanceListener
{
	void OnSetInstance();
}

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static bool		 IsQuitting	= false;
	private	static object	_lock		= new object();
	private	static T		_instance;
	public	static T		 Instance
	{
		get
		{
			if (IsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '"+ typeof(T) +
								 "' already destroyed on application quit." +
								 " Unable create again - returning null.");
				return null;
			}
			lock (_lock)
			{
				if (_instance == null)
				{
					SetInstance((T) FindObjectOfType(typeof(T)));
					
					if (FindObjectsOfType(typeof(T)).Length > 1 )
					{
						Debug.LogError(	"[Singleton] Multiple instances of '" + typeof(T).Name +
										"' found - This means some serious problems. " +
										" Reopenning the scene might fix it.");
						return _instance;
					}
					
					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) "+ typeof(T).ToString();
						
						DontDestroyOnLoad(singleton);
						
						Debug.Log(	"[Singleton] An instance of " + typeof(T) + 
									" not found - '" + singleton +
									"' was created with DontDestroyOnLoad.");
					} else {
						Debug.Log(	"[Singleton] Found instance: '" +
									_instance.gameObject.name + "'.");
					}
				}
				
				return _instance;
			}
		}
	}

	private static void SetInstance(T instance)
	{
		_instance = instance;
		(_instance as ISingletonInstanceListener)?.OnSetInstance();
	}

	private void Awake()
	{
		IsQuitting = false;
	}

    public void OnDestroy()
    {
        IsQuitting = true;
    }
}