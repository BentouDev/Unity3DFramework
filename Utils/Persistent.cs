using UnityEngine;

public class Persistent : MonoBehaviour
{
	public bool IsDestroyedOnExit;

	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void DestroyOnExit()
	{
		Debug.Log("[Persistent] Object " + name + 
                  " destroyed " + IsDestroyedOnExit);

	    if (IsDestroyedOnExit)
	    {
	        Destroy(gameObject);
	    }
	}
}
