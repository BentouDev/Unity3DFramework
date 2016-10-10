using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static bool		 IsQuitting	= false;
	private	static object	_lock		= new object();
	private	static T		_instance;
	public	static T		 Instance
	{
		get
		{
			if(IsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '"+ typeof(T) +
								 "' already destroyed on application quit." +
								 " Unable create again - returning null.");
				return null;
			}
			lock(_lock)
			{
				if (_instance == null)
				{
					_instance = (T) FindObjectOfType(typeof(T));
					
					if ( FindObjectsOfType(typeof(T)).Length > 1 )
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

    public void OnDestroy()
    {
        IsQuitting = true;
    }
}
