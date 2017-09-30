using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{ 
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        public Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();

        [SerializeField]
        private List<TKey> SerializedKeys = new List<TKey>();

        private List<TValue> SerializedValues = new List<TValue>();

        public void OnBeforeSerialize()
        {
            SerializedKeys = Dictionary.Keys.ToList();
            SerializedValues = Dictionary.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            Dictionary = new Dictionary<TKey, TValue>();

            Assert.AreEqual(SerializedKeys.Count, SerializedValues.Count);

            for (int i = 0; i < SerializedKeys.Count; i++)
            {
                Dictionary[SerializedKeys[i]] = SerializedValues[i];
            }

            SerializedKeys.Clear();
            SerializedValues.Clear();
        }
    }
}
