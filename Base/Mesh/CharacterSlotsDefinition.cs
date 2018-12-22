using System.Collections;
using System.Collections.Generic;
using Framework;
using Malee;
using UnityEngine;

namespace Framework
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Data/Character Slots Definiton", fileName = "New Character Slots Definiton")]
#endif
    [System.Serializable]
    public class CharacterSlotsDefinition : Framework.BaseScriptableObject
    {
        [System.Serializable]
        public struct SlotInfo
        {
            [SerializeField]
            public string Name;
        }
    
        [SerializeField]
        public List<SlotInfo> Slots;
    }
}
