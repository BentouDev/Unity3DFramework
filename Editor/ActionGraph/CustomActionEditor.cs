using System;
using UnityEngine;

namespace Framework.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomActionEditor : Attribute
    {
        public CustomActionEditor(System.Type type)
        {
            
        }
    }
}