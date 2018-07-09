using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Base
{
    [CustomEditor(typeof(BaseScriptableObject), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class BaseScriptableObjectEditor : BaseFrameworkEditor
    {
        
    }
}