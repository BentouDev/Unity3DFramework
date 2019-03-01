using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using UnityEngine;

namespace Framework
{
    public class CharacterMesh : BaseBehaviour
    {
        [Header("Pawn")]
        [RequireValue]
        public Framework.BasePawn Pawn;
        
        [Header("Animation")]
        [RequireValue]
        public RuntimeAnimatorController AnimGraph;
        
        [RequireValue]
        public Avatar Skeleton;
        
        [Header("Slots")]
        public CharacterSlotsDefinition SlotDefinition;
        
        public GameObject RootMesh;
    
        [SerializeField]
        [HideInInspector]
        public List<SlotInfo> Slots;
    
        private Transform _rootInstance;
    
        [System.Serializable]
        public struct SlotInfo
        {
            [SerializeField]
            public GameObject Mesh;
    
            [SerializeField]
            public string Name;
        }
    
        private void UpdateSlots()
        {
            if (SlotDefinition)
            {
                Slots.Resize(SlotDefinition.Slots.Count);
    
                for (int i = 0; i < SlotDefinition.Slots.Count; i++)
                {
                    var slot = Slots[i];
                    slot.Name = SlotDefinition.Slots[i].Name;
                    Slots[i] = slot;
                }
            }
        }
    
        void OnValidate()
        {
            UpdateSlots();
        }
    
        void Start()
        {
            var go = Instantiate(RootMesh, transform);
    
            _rootInstance = go.transform;
            
            var anim = go.GetOrAddComponent<Animator>();
            
            anim.avatar = Skeleton;
            anim.runtimeAnimatorController = AnimGraph;
    
            Pawn.Anim = anim;
            
            foreach (var child in Slots)
            {
                if (child.Mesh == null) continue;

                // Create temporary instance to copy data from
                GameObject tempObject = Instantiate(child.Mesh);
                AddLimb(tempObject, _rootInstance);
                Destroy(tempObject);
            }
        }
        
        void AddLimb(GameObject limb, Transform root)
        {
            var bonedObjects = limb.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedRenderer in bonedObjects)
                ProcessBonedObject(skinnedRenderer, root);
        }
        
        private void ProcessBonedObject(SkinnedMeshRenderer thisRenderer, Transform root)
        {
            var newObj = new GameObject(thisRenderer.gameObject.name);
            newObj.transform.SetParent(root.transform);
            newObj.AddComponent<SkinnedMeshRenderer>();

            var newRenderer = newObj.GetComponent<SkinnedMeshRenderer>();
            var newBones = new Transform[thisRenderer.bones.Length];
            
            for (var i = 0; i < thisRenderer.bones.Length; i++)
                newBones[i] = FindChildByName(thisRenderer.bones[i].name, root.transform);
            
            newRenderer.bones      = newBones;
            newRenderer.sharedMesh = thisRenderer.sharedMesh;
            newRenderer.materials  = thisRenderer.materials;
        }
     
        private Transform FindChildByName(string thisName, Transform thisObj)
        {
            if (thisObj.name == thisName)
                return thisObj.transform;
    
            for (var i = 0; i < thisObj.childCount; i++)
            {
                var returnObj = FindChildByName(thisName, thisObj.GetChild(i));
                if (returnObj)
                    return returnObj;
            }
            
            return null;
        }
    }
}
