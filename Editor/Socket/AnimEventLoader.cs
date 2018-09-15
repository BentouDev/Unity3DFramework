using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class AnimEventLoader : UnityEditor.AssetPostprocessor
    {
        public const float FramePerSecond = 30.0f;

        [System.Serializable]
        public struct AnimEvent
        {
            public string comment;
            public int[]  color;
            public int    frame;
        }

        [System.Serializable]
        public class AnimData
        {
            public AnimEvent[] Events;
        }

        internal class AnimClipEvents
        {
            internal ModelImporterClipAnimation Clip;
            internal List<AnimationEvent>       Events = new List<AnimationEvent>();
        }

        private Dictionary<string, AnimClipEvents> AnimEventBank;

        static string BuildJsonPath(string assetPath)
        {
            var dotPos = assetPath.LastIndexOf(".", StringComparison.Ordinal);
            if (dotPos < 0)
                return string.Empty;

            var cardinal = assetPath.Substring(0, dotPos);
            var jsonPath = cardinal + ".json";

            return jsonPath;
        }

        static AnimData ParseJson(string path)
        {
            string rawJson  = File.ReadAllText(path);
            string wrapJson = WrapToClass(rawJson, "Events");

            return JsonUtility.FromJson<AnimData>(wrapJson);
        }

        static void BuildEventBank(ModelImporter model, out Dictionary<string, AnimClipEvents> eventBank)
        {
            model.clipAnimations = model.defaultClipAnimations;

            eventBank = new Dictionary<string, AnimClipEvents>();
            foreach (var clip in model.clipAnimations)
            {
                eventBank[clip.takeName] = new AnimClipEvents(){ Clip = clip };
            }
        }

        static void SaveAnimEvents(ModelImporter model, Dictionary<string, AnimClipEvents> eventBank)
        {
            foreach (var animClip in eventBank.Values)
            {
                animClip.Clip.events = animClip.Events.ToArray();
            }

            model.clipAnimations = eventBank.Values.Select(a => a.Clip).ToArray();            
        }
        
        void OnPreprocessAnimation()
        {
            string jsonPath = BuildJsonPath(assetPath);

            if (!File.Exists(jsonPath))
                return;

            AnimData animData = ParseJson(jsonPath);

            if (animData == null || !animData.Events.Any())
                return;
            
            ModelImporter  model = assetImporter as ModelImporter;
            BuildEventBank(model, out AnimEventBank);

            foreach (var @event in animData.Events)
            {
                var def = new AnimationEvent { stringParameter = @event.comment };

                foreach (var take in model.importedTakeInfos)
                {
                    float eventTime = @event.frame / take.sampleRate;
                    if (EqualOrLarger (eventTime, take.startTime)
                    &&  EqualOrSmaller(eventTime, take.stopTime))
                    {
                        AnimClipEvents target;
                        if (AnimEventBank.TryGetValue(take.name, out target))
                        {
                            def.time = eventTime - take.startTime;
                            target.Events.Add(def);
                        }
                        else
                        {
                            Debug.LogErrorFormat("Error: Got event for take {0} with no clip generated for!", take.name);
                        }
                    }
                }
            }

            SaveAnimEvents(model, AnimEventBank);
        }
 
        bool EqualOrSmaller(float a, float b)
        {
            return a < b || (Mathf.Abs(a - b) < Mathf.Epsilon);
        }
        
        bool EqualOrLarger(float a, float b)
        {
            return a > b || (Mathf.Abs(a - b) < Mathf.Epsilon);
        }

        static string WrapToClass(string source, string className)
        {
            return $"{{ \"{className}\": {source}}}";
        }
    }
}