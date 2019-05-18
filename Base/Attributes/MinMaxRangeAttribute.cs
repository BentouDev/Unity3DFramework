using UnityEngine;

namespace Framework
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float MinLimit = 0;
        public float MaxLimit = 1;
        public bool ShowEditRange;
        public bool ShowDebugValues;

        public MinMaxRangeAttribute(int min, int max)
        {
            MinLimit = min;
            MaxLimit = max;
        }
    }
}