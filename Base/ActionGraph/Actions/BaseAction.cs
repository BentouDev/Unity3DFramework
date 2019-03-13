using UnityEngine;

namespace Framework.Actions
{
    [System.Serializable]
    public abstract class BaseAction : ParametrizedScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public ActionGraph Graph;

        public void Execute()
        {
            OnExecute();
        }

        protected abstract void OnExecute();        

        public override IDataSetProvider GetProvider()
        {
            return Graph;
        }
    }
}