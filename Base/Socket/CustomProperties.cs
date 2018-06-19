using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using UnityEngine;

public class CustomProperties : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	public List<GenericParameter> Properties = new List<GenericParameter>();

#if UNITY_EDITOR
	public void AddProperty(string name, object value)
	{
		var entry = Properties.FirstOrDefault(p => p.Name.Equals(name));
		if (entry == null)
		{
			entry = new GenericParameter(typeof(string));
			Properties.Add(entry);
		}
		
		entry.Name  = name;
		entry.SetAs<string>(value.ToString());
	}
#endif
}
