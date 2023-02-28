using System;

namespace Koyashiro.UdonList
{
    public static class UdonListExt
    {
        public static int Capacity<T>(this UdonList<T> list)
        {
            return Core.UdonList.Capacity((object[])(object)list);
        }

        public static void SetCapacity<T>(this UdonList<T> list, int capacity)
        {
            Core.UdonList.SetCapacity((object[])(object)list, capacity);
        }

        public static int Count<T>(this UdonList<T> list)
        {
            return Core.UdonList.Count((object[])(object)list);
        }

        public static T GetValue<T>(this UdonList<T> list, int index)
        {
            return Core.UdonList.GetValue<T>((object[])(object)list, index);
        }

        public static void SetValue<T>(this UdonList<T> list, int index, T item)
        {
            Core.UdonList.SetValue((object[])(object)list, index, item);
        }

        public static void Add<T>(this UdonList<T> list, T item)
        {
            Core.UdonList.Add((object[])(object)list, item);
        }

        public static void AddRange<T>(this UdonList<T> list, T[] collection)
        {
            Core.UdonList.AddRange((object[])(object)list, collection);
        }

        public static void Clear<T>(this UdonList<T> list)
        {
            Core.UdonList.Clear((object[])(object)list);
        }

        public static bool Contains<T>(this UdonList<T> list, T item)
        {
            return Core.UdonList.Contains((object[])(object)list, item);
        }

        public static void CopyTo<T>(this UdonList<T> list, T[] array)
        {
            Core.UdonList.CopyTo((object[])(object)list, array);
        }

        public static void CopyTo<T>(this UdonList<T> list, int index, T[] array, int arrayIndex, int count)
        {
            Core.UdonList.CopyTo((object[])(object)list, index, array, arrayIndex, count);
        }

        public static void CopyTo<T>(UdonList<T> list, T[] array, int arrayIndex)
        {
            Core.UdonList.CopyTo((object[])(object)list, array, arrayIndex);
        }

        public static int EnsureCapacity<T>(this UdonList<T> list, int capacity)
        {
            return Core.UdonList.EnsureCapacity((object[])(object)list, capacity);
        }

        public static UdonList<T> GetRange<T>(this UdonList<T> list, int index, int count)
        {
            return (UdonList<T>)(object)Core.UdonList.GetRange((object[])(object)list, index, count);
        }

        public static int IndexOf<T>(this UdonList<T> list, T item)
        {
            return Core.UdonList.IndexOf((object[])(object)list, item);
        }

        public static int IndexOf<T>(this UdonList<T> list, T item, int index)
        {
            return Core.UdonList.IndexOf((object[])(object)list, item, index);
        }

        public static int IndexOf<T>(this UdonList<T> list, T item, int index, int count)
        {
            return Core.UdonList.IndexOf((object[])(object)list, item, index, count);
        }

        public static void Insert<T>(this UdonList<T> list, int index, T item)
        {
            Core.UdonList.Insert((object[])(object)list, index, item);
        }

        public static void InsertRange<T>(this UdonList<T> list, int index, T[] collection)
        {
            Core.UdonList.InsertRange((object[])(object)list, index, collection);
        }

        public static int LastIndexOf<T>(this UdonList<T> list, T item)
        {
            return Core.UdonList.LastIndexOf((object[])(object)list, item);
        }

        public static int LastIndexOf<T>(this UdonList<T> list, T item, int index)
        {
            return Core.UdonList.LastIndexOf((object[])(object)list, item, index);
        }

        public static int LastIndexOf<T>(this UdonList<T> list, T item, int index, int count)
        {
            return Core.UdonList.LastIndexOf((object[])(object)list, item, index, count);
        }

        public static bool Remove<T>(this UdonList<T> list, T item)
        {
            return Core.UdonList.Remove((object[])(object)list, item);
        }

        public static void RemoveAt<T>(this UdonList<T> list, int index)
        {
            Core.UdonList.RemoveAt((object[])(object)list, index);
        }

        public static void RemoveRange<T>(this UdonList<T> list, int index, int count)
        {
            Core.UdonList.RemoveRange((object[])(object)list, index, count);
        }

        public static void Reverse<T>(this UdonList<T> list)
        {
            Core.UdonList.Reverse((object[])(object)list);
        }

        public static void Reverse<T>(this UdonList<T> list, int index, int count)
        {
            Core.UdonList.Reverse((object[])(object)list, index, count);
        }

        public static void Sort<T>(this UdonList<T> list) where T : IComparable
        {
            Core.UdonList.Sort<T>((object[])(object)list);
        }

        public static void Sort<T>(this UdonList<T> list, int index, int count) where T : IComparable
        {
            Core.UdonList.Sort<T>((object[])(object)list, index, count);
        }

        public static T[] ToArray<T>(this UdonList<T> list)
        {
            return Core.UdonList.ToArray<T>((object[])(object)list);
        }

        public static void TrimExcess<T>(this UdonList<T> list)
        {
            Core.UdonList.TrimExcess((object[])(object)list);
        }
    }
}
