namespace Koyashiro.UdonDictionary
{
    public static class UdonDictionaryExt
    {
        public static TValue GetValue<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key)
        {
            return Core.UdonDictionary.GetValue<TValue>((object[])(object)dic, key);
        }

        public static void SetValue<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            Core.UdonDictionary.SetValue((object[])(object)dic, key, value);
        }

        public static int Count<TKey, TValue>(this UdonDictionary<TKey, TValue> dic)
        {
            return Core.UdonDictionary.Count((object[])(object)dic);
        }

        public static TKey[] Keys<TKey, TValue>(this UdonDictionary<TKey, TValue> dic)
        {
            return Core.UdonDictionary.Keys<TKey>((object[])(object)dic);
        }

        public static TValue[] Values<TKey, TValue>(this UdonDictionary<TKey, TValue> dic)
        {
            return Core.UdonDictionary.Values<TValue>((object[])(object)dic);
        }

        public static UdonKeyValuePair<TKey, TValue>[] KeyValuePairs<TKey, TValue>(this UdonDictionary<TKey, TValue> dic)
        {
            return Core.UdonDictionary.KeyValuePairs<TKey, TValue>((object[])(object)dic);
        }

        public static void Add<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            Core.UdonDictionary.Add((object[])(object)dic, key, value);
        }

        public static void Clear<TKey, TValue>(this UdonDictionary<TKey, TValue> dic)
        {
            Core.UdonDictionary.Clear((object[])(object)dic);
        }

        public static bool ContainsKey<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key)
        {
            return Core.UdonDictionary.ContainsKey((object[])(object)dic, key);
        }

        public static bool ContainsValue<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TValue value)
        {
            return Core.UdonDictionary.ContainsValue((object[])(object)dic, value);
        }

        public static bool Remove<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key)
        {
            return Core.UdonDictionary.Remove((object[])(object)dic, key);
        }

        public static bool TryGetValue<TKey, TValue>(this UdonDictionary<TKey, TValue> dic, TKey key, out TValue value)
        {
            return Core.UdonDictionary.TryGetValue((object[])(object)dic, key, out value);
        }
    }
}
