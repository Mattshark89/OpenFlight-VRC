using UdonSharp;

namespace Koyashiro.UdonJson
{
    using Koyashiro.UdonDictionary;
    using Koyashiro.UdonList;

    [UnityEngine.AddComponentMenu("")]
    public class UdonJsonValue : UdonSharpBehaviour
    {
        public static UdonJsonValue NewString(string s)
        {
            return New(UdonJsonValueKind.String, s);
        }

        public static UdonJsonValue NewNumber(double n)
        {
            return New(UdonJsonValueKind.Number, n);
        }

        public static UdonJsonValue NewObject()
        {
            return New(UdonJsonValueKind.Object, UdonDictionary<string, object>.New());
        }

        public static UdonJsonValue NewObject(UdonDictionary<string, object> obj)
        {
            return New(UdonJsonValueKind.Object, obj);
        }

        public static UdonJsonValue NewArray()
        {
            return New(UdonJsonValueKind.Array, UdonList<object>.New());
        }

        public static UdonJsonValue NewArray(UdonList<object> array)
        {
            return New(UdonJsonValueKind.Array, array);
        }

        public static UdonJsonValue NewTrue()
        {
            return New(UdonJsonValueKind.True, true);
        }

        public static UdonJsonValue NewFalse()
        {
            return New(UdonJsonValueKind.False, false);
        }

        public static UdonJsonValue NewNull()
        {
            return New(UdonJsonValueKind.Null, null);
        }

        private static UdonJsonValue New(UdonJsonValueKind kind, object value)
        {
            return (UdonJsonValue)(object)(new object[] { kind, value });
        }
    }
}
