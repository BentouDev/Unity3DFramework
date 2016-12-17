using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StringUtils
{
    public static string MakeUnique(string name, IEnumerable<string> collection)
    {
        string result = name;
        
        for (int i = 0; collection.Any(c => c.Equals(result)); i++)
        {
            result = string.Format("{0} {1}", name, i);
        }

        return result;
    }
}
