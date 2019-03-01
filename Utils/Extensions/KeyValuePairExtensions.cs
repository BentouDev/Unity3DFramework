using System.Collections.Generic;

namespace Framework
{
    public static class KeyValuePairExtensions
    {
        // No suitable Deconstruct instance or extension method was found for type 'KeyValuePair<string, ParametrizedProperty>', with 2 out parameters and a void return type.
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}