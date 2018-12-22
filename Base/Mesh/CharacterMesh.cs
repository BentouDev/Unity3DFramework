using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using UnityEditor.Animations;
using UnityEngine;

namespace Framework
{
    public class CharacterMesh : MonoBehaviour
    {
        [Header("Pawn")]
        public Framework.BasePawn Pawn;
        
        [Header("Animation")]
        public AnimatorController AnimGraph;
        public Avatar Skeleton;
        
        [Header("Slots")]
        public CharacterSlotsDefinition SlotDefinition;
        
        public GameObject RootMesh;
    
        [SerializeField]
        [HideInInspector]
        public List<SlotInfo> Slots;
    
        private Transform RootInstance;
    
        [System.Serializable]
        public struct SlotInfo
        {
            [SerializeField]
            public GameObject Mesh;
    
            [SerializeField]
            public string Name;
    
            [HideInInspector]
            internal GameObject Instance;
            
            internal void SetInstance(GameObject obj)
            {
                Instance = obj;
            }
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
    
            RootInstance = go.transform;
            
            var anim = go.GetOrAddComponent<Animator>();
            
            anim.avatar = Skeleton;
            anim.runtimeAnimatorController = AnimGraph;
    
            Pawn.Anim = anim;
            
            foreach (var child in Slots)
            {
                if (child.Mesh == null) continue;
    
                child.SetInstance(Instantiate(child.Mesh));
                AddLimb(child.Instance.transform, RootInstance);
                Destroy(child.Instance.gameObject);
            }
        }
        
        void AddLimb(Transform limb, Transform root)
        {
            var BonedObjects = limb.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var SkinnedRenderer in BonedObjects)
                ProcessBonedObject(SkinnedRenderer, root);
        }
        
        private void ProcessBonedObject(SkinnedMeshRenderer thisRenderer, Transform root)
        {
            var NewObj = new GameObject( thisRenderer.gameObject.name );
            NewObj.transform.parent = root.transform;
            NewObj.AddComponent<SkinnedMeshRenderer>();
            
            var NewRenderer = NewObj.GetComponent<SkinnedMeshRenderer>();
            var MyBones = new Transform[thisRenderer.bones.Length];
            
            for (var i = 0; i < thisRenderer.bones.Length; i++)
                MyBones[i] = FindChildByName(thisRenderer.bones[i].name, root.transform );
            
            NewRenderer.bones      = MyBones;
            NewRenderer.sharedMesh = thisRenderer.sharedMesh;
            NewRenderer.materials  = thisRenderer.materials;
        }
     
        private Transform FindChildByName(string thisName, Transform thisObj)
        {
            Transform ReturnObj;
            if (thisObj.name == thisName)
                return thisObj.transform;
    
            for (var i = 0; i < thisObj.childCount; i++)
            {
                ReturnObj = FindChildByName(thisName, thisObj.GetChild(i));
                if (ReturnObj)
                    return ReturnObj;
            }
            
            return null;
        }
    }
}
