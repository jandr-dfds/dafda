using System.Collections.Generic;

namespace Dafda.Serializing
{
    internal static class KeyValuePairExtension
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
        {
            key = source.Key;
            value = source.Value;
        }
    }
}