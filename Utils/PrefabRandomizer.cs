using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    [ExecuteInEditMode]
    [SelectionBase]
    public class PrefabRandomizer : MonoBehaviour
    {
#if UNITY_EDITOR
        public List<GameObject> Prefabs;

        [SerializeField]
        [HideInInspector]
        public bool Locked = false;

        public static bool IsGloballyLocked;

        private int PickRandom()
        {
            return Random.Range(0, Prefabs.Count);
        }

        public void Randomize(bool force = false)
        {
            if (!Prefabs.Any())
                return;

            if (IsGloballyLocked)
                return;

            if (Locked && !force)
                return;

            RemoveChildren();

            Instantiate(Prefabs[PickRandom()], transform);
        }

        public void RemoveChildren()
        {
            var children = gameObject.GetComponentsInChildren<Transform>().Where(t => t != transform);
            
            foreach (Transform child in children)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        [SerializeField]
        [HideInInspector]
        private int localID = 0;

        void Awake()
        {
            if (Application.isPlaying)
                return;

            if (localID == 0)
            {
                localID = GetInstanceID();
                return;
            }

            if (localID != GetInstanceID() && GetInstanceID() < 0)
            {
                localID = GetInstanceID();
                Randomize();
            }
        }
#endif
    }
}