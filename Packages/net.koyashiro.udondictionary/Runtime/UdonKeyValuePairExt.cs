namespace Koyashiro.UdonDictionary
{
    public static class UdonKeyValuePairExt
    {
        public static TKey GetKey<TKey, TValue>(this UdonKeyValuePair<TKey, TValue> dic)
        {
            return (TKey)((object[])(object)dic)[0];
        }

        public static TValue GetValue<TKey, TValue>(this UdonKeyValuePair<TKey, TValue> dic)
        {
            return (TValue)((object[])(object)dic)[1];
        }
    }
}
