using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnyKey : MonoBehaviour
{
	public bool Single;
	public Button.ButtonClickedEvent SomeEvent;

	private bool Used;

	void Update()
	{
		if (Single && Used)
			return;

		if (Input.anyKey)
		{
			Used = true;
			SomeEvent.Invoke();
		}
	}
}
