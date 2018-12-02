using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    [CustomEditor(typeof(SceneDataSet))]
    class SceneDataSetEditor : DataBankEditor
    {
        private SceneDataSet DataSet => target as SceneDataSet;
        protected override DataBank Target => DataSet.Bank;
    }
}