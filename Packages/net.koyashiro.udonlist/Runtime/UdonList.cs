using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonList
{
    [AddComponentMenu("")]
    public class UdonList<T> : UdonSharpBehaviour
    {
        public static UdonList<T> New()
        {
            return (UdonList<T>)(object)Core.UdonList.New<T>();
        }

        public static UdonList<T> New(T[] collection)
        {
            return (UdonList<T>)(object)Core.UdonList.New(collection);
        }

        public static UdonList<T> New(int capacity)
        {
            return (UdonList<T>)(object)Core.UdonList.New<T>(capacity);
        }
    }
}
