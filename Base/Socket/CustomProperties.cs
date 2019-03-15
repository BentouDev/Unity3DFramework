using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using UnityEngine;

public class CustomProperties : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	public List<Parameter> Properties = new List<Parameter>();

#if UNITY_EDITOR
	public void AddProperty(string name, object value)
	{
		var entry = Properties.FirstOrDefault(p => p.Name.Equals(name));
		if (entry == null)
		{
			entry = new Parameter(typeof(string), name);
			Properties.Add(entry);
		}
		else 
			entry.Name  = name;

		entry.Value.SetAs<string>(value.ToString());
	}
#endif
}
