using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Data/Prefab Randomizer List", fileName = "New Prefab Randomizer List")]
#endif
    [System.Serializable]
    public class PrefabRandomizerList : ScriptableObject
    {
        [SerializeField]
        public List<GameObject> Prefabs = new List<GameObject>();

        public int Count => Prefabs.Count;
        public bool Any => Prefabs.Any();
        public GameObject this[int i] => Prefabs[i];
        public GameObject Random => Prefabs[UnityEngine.Random.Range(0, Prefabs.Count)];
    }
}