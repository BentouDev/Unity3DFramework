using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    [CustomEditor(typeof(DataSet))]
    class DataSetEditor : DataBankEditor
    {
        private DataSet DataSet => target as DataSet;
        protected override DataBank Target => DataSet.Bank;
    }
}