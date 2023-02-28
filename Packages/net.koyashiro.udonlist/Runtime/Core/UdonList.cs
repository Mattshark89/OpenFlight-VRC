using System;
using Koyashiro.UdonList.Internal;

namespace Koyashiro.UdonList.Core
{
    public static class UdonList
    {
        private const int DEFAULT_CAPACITY = 4;

        public static object[] New<T>()
        {
            var items = Array.CreateInstance(typeof(T), 0);
            var size = 0;
            var type = typeof(T);

            return new object[] { items, size, type };
        }

        public static object[] New<T>(T[] collection)
        {
            if (collection == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(collection));
            }

            var items = (T[])collection.Clone();
            var size = collection.Length;
            var type = GetElementType(items);

            return new object[] { items, size, type };
        }

        public static object[] New<T>(int capacity)
        {
            if (capacity < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = Array.CreateInstance(typeof(T), capacity);
            var size = 0;
            var type = typeof(T);

            return new object[] { items, size, type };
        }

        public static int Capacity(object[] list)
        {
            var items = (Array)list[0];

            return items.Length;
        }

        public static void SetCapacity(object[] list, int capacity)
        {
            var size = (int)list[1];

            if (capacity < size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            if (capacity == items.Length)
            {
                return;
            }

            var type = (Type)list[2];

            if (capacity == 0)
            {
                list[0] = Array.CreateInstance(type, 0);
                return;
            }

            var newItems = Array.CreateInstance(type, capacity);
            Array.Copy(items, newItems, size);
            list[0] = newItems;
        }

        public static int Count(object[] list)
        {
            return (int)list[1];
        }

        public static T GetValue<T>(object[] list, int index)
        {
            var size = (int)list[1];

            if ((uint)index >= (uint)size)
            {
                ExceptionHelper.ThrowIndexOutOfRangeException();
            }

            var items = (Array)list[0];

            return (T)items.GetValue(index);
        }

        public static void SetValue(object[] list, int index, object item)
        {
            var size = (int)list[1];

            if ((uint)index >= (uint)size)
            {
                ExceptionHelper.ThrowIndexOutOfRangeException();
            }

            var items = (Array)list[0];

            items.SetValue(item, index);
        }

        public static void Add(object[] list, object item)
        {
            var items = (Array)list[0];
            var size = (int)list[1];

            if ((uint)size >= (uint)items.Length)
            {
                Grow(list, size + 1);
                items = (Array)list[0];
            }

            items.SetValue(item, size);
            list[1] = size + 1;
        }

        public static void AddRange(object[] list, Array collection)
        {
            if (collection == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(collection));
            }

            if (collection.Length == 0)
            {
                return;
            }

            var items = (Array)list[0];
            var size = (int)list[1];

            if (items.Length - size < collection.Length)
            {
                Grow(list, size + collection.Length);
                items = (Array)list[0];
            }

            Array.Copy(collection, 0, items, size, collection.Length);
            list[1] = size + collection.Length;
        }

        public static void Clear(object[] list)
        {
            list[1] = 0;
        }

        public static bool Contains(object[] list, object item)
        {
            var size = (int)list[1];

            return size != 0 && IndexOf(list, item) >= 0;
        }

        public static void CopyTo(object[] list, Array array)
        {
            CopyTo(list, array, 0);
        }

        public static void CopyTo(object[] list, int index, Array array, int arrayIndex, int count)
        {
            if (array == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(array));
            }

            var size = (int)list[1];

            if (size - index < count)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            Array.Copy(items, index, array, arrayIndex, count);
        }

        public static void CopyTo(object[] list, Array array, int arrayIndex)
        {
            if (array == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(array));
            }

            var items = (Array)list[0];
            var size = (int)list[1];

            Array.Copy(items, 0, array, arrayIndex, size);
        }

        public static int EnsureCapacity(object[] list, int capacity)
        {
            if (capacity < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            if (items.Length < capacity)
            {
                Grow(list, capacity);
                items = (Array)list[0];
            }

            return items.Length;
        }

        private static void Grow(object[] list, int capacity)
        {
            var items = (Array)list[0];

            if (items.Length < capacity)
            {
                int newCapacity = items.Length == 0 ? DEFAULT_CAPACITY : 2 * items.Length;

                if (newCapacity < capacity)
                {
                    newCapacity = capacity;
                }

                SetCapacity(list, newCapacity);
            }
        }

        public static object[] GetRange(object[] list, int index, int count)
        {
            if (index < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var size = (int)list[1];

            if (size - index < count)
            {
                ExceptionHelper.ThrowArgumentException();
            }

            var items = (Array)list[0];
            var type = (Type)list[2];

            var newItems = Array.CreateInstance(type, count);
            Array.Copy(items, index, newItems, 0, count);

            return new object[] { newItems, count, type };
        }

        public static int IndexOf(object[] list, object item)
        {
            var items = (Array)list[0];
            var size = (int)list[1];

            return Array.IndexOf(items, item, 0, size);
        }

        public static int IndexOf(object[] list, object item, int index)
        {
            var size = (int)list[1];

            if (index > size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            return Array.IndexOf(items, item, index, size - index);
        }

        public static int IndexOf(object[] list, object item, int index, int count)
        {
            var size = (int)list[1];

            if (index > size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count < 0 || index > size - count)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            return Array.IndexOf(items, item, index, count);
        }

        public static void Insert(object[] list, int index, object item)
        {
            var size = (int)list[1];

            if ((uint)index > (uint)size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            if (size == items.Length)
            {
                Grow(list, size + 1);
                items = (Array)list[0];
            }

            if (index < size)
            {
                Array.Copy(items, index, items, index + 1, size - index);
            }

            items.SetValue(item, index);
            list[1] = size + 1;
        }

        public static void InsertRange(object[] list, int index, Array collection)
        {
            if (collection == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(collection));
            }

            var size = (int)list[1];

            if ((uint)index > (uint)size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (collection.Length == 0)
            {
                return;
            }

            var items = (Array)list[0];

            if (items.Length - size < collection.Length)
            {
                Grow(list, size + collection.Length);
                items = (Array)list[0];
            }

            if (index < size)
            {
                Array.Copy(items, index, items, index + collection.Length, size - index);
            }

            if (object.Equals(list, collection))
            {
                Array.Copy(items, 0, items, index, index);
                Array.Copy(items, index + collection.Length, items, index * 2, size - index);
            }
            else
            {
                collection.CopyTo(items, index);
            }

            list[1] = size + collection.Length;
        }

        public static int LastIndexOf(object[] list, object item)
        {
            var size = (int)list[1];
            if (size == 0)
            {
                return -1;
            }

            return LastIndexOf(list, item, size - 1, size);
        }

        public static int LastIndexOf<T>(object[] list, T item, int index)
        {
            var size = (int)list[1];

            if (index >= size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            return LastIndexOf(list, item, index, index + 1);
        }

        public static int LastIndexOf(object[] list, object item, int index, int count)
        {
            var size = (int)list[1];

            if ((size != 0) && (index < 0))
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if ((size != 0) && (count < 0))
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (size == 0)
            {
                return -1;
            }

            if (index >= size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count > index + 1)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            return Array.LastIndexOf(items, item, index, count);
        }

        public static bool Remove(object[] list, object item)
        {
            var index = IndexOf(list, item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(list, index);
            return true;
        }

        public static void RemoveAt(object[] list, int index)
        {
            var size = (int)list[1];

            if ((uint)index >= (uint)size)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            size--;

            if (index < size)
            {
                var items = (Array)list[0];

                Array.Copy(items, index + 1, items, index, size - index);
            }

            list[1] = size;
        }

        public static void RemoveRange(object[] list, int index, int count)
        {
            if (index < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];
            var size = (int)list[1];

            if (size - index < count)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count == 0)
            {
                return;
            }

            size -= count;
            if (index < size)
            {
                Array.Copy(items, index + count, items, index, size - index);
            }

            list[1] = size;
        }

        public static void Reverse(object[] list)
        {
            var size = (int)list[1];

            Reverse(list, 0, size);
        }

        public static void Reverse(object[] list, int index, int count)
        {
            if (index < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var size = (int)list[1];
            if (size - index < count)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (Array)list[0];

            Array.Reverse(items, index, count);
        }

        public static void Sort<T>(object[] list) where T : IComparable
        {
            var items = (T[])list[0];
            var size = (int)list[1];

            HeapSort(items, size);
        }

        public static void Sort<T>(object[] list, int index, int count) where T : IComparable
        {
            if (index < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            if (count < 0)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var size = (int)list[1];
            if (size - index < count)
            {
                ExceptionHelper.ThrowArgumentOutOfRangeException();
            }

            var items = (T[])list[0];

            HeapSort(items, index, count);
        }

        public static T[] ToArray<T>(object[] list)
        {
            var items = (Array)list[0];
            var size = (int)list[1];

            if (size == 0)
            {
                return new T[0];
            }

            var array = new T[size];
            Array.Copy(items, array, size);

            return array;
        }

        public static void TrimExcess(object[] list)
        {
            var items = (Array)list[0];
            var size = (int)list[1];

            int threshold = (int)(((double)items.Length) * 0.9);

            if (size < threshold)
            {
                return;
            }

            SetCapacity(list, size);
        }

        private static Type GetElementType(Array array)
        {
            var typeFullName = array.GetType().FullName;
            typeFullName = typeFullName.Remove(typeFullName.Length - 2);

            var type = Type.GetType(typeFullName);
            if (type == null)
            {
                var assemblyName = typeFullName;
                while (true)
                {
                    type = Type.GetType($"{typeFullName}, {assemblyName}");
                    if (type != null)
                    {
                        break;
                    }

                    var dotIndex = assemblyName.LastIndexOf('.');
                    if (dotIndex != -1)
                    {
                        assemblyName = assemblyName.Remove(dotIndex);
                    }
                    else
                    {
                        // Error
                        ((object)null).ToString();
                    }
                }
            }

            return type;
        }

        private static void HeapSort<T>(T[] array, int index, int count) where T : IComparable
        {
            for (int i = index + 1, _t = index + count; i < _t; i++)
            {
                var j = i;
                while (j > index)
                {
                    var _j = (j - (index + 1)) / 2 + index;
                    var _p = array[_j];
                    var _c = array[j];
                    if (_c != null && _c.CompareTo(_p) > 0
                     || _p != null && _p.CompareTo(_c) < 0)
                    {
                        array[_j] = _c;
                        array[j] = _p;

                        j = _j;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (var i = index + count - 1; i > index; i--)
            {
                {
                    var tmp = array[index];
                    array[index] = array[i];
                    array[i] = tmp;
                }

                var j = index;
                var k = index;

                while (true)
                {
                    var left = 2 * (j - index) + 1 + index;
                    var right = left + 1;

                    if (left >= i)
                    {
                        break;
                    }

                    {
                        var _p = array[left];
                        var _c = array[k];
                        if (_c != null && _c.CompareTo(_p) < 0
                         || _p != null && _p.CompareTo(_c) > 0)
                        {
                            k = left;
                        }
                    }

                    if (right < i)
                    {
                        var _p = array[right];
                        var _c = array[k];
                        if (_c != null && _c.CompareTo(_p) < 0
                         || _p != null && _p.CompareTo(_c) > 0)
                        {
                            k = right;
                        }
                    }

                    if (k == j)
                    {
                        break;
                    }

                    {
                        var tmp = array[k];
                        array[k] = array[j];
                        array[j] = tmp;
                    }

                    j = k;
                }
            }
        }

        private static void HeapSort<T>(T[] array, int count) where T : IComparable
        {
            for (int i = 1; i < count; i++)
            {
                var j = i;
                while (j > 0)
                {
                    var _j = (j - 1) / 2;
                    var _p = array[_j];
                    var _c = array[j];
                    if (_c != null && _c.CompareTo(_p) > 0
                     || _p != null && _p.CompareTo(_c) < 0)
                    {
                        array[_j] = _c;
                        array[j] = _p;

                        j = _j;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (var i = count - 1; i > 0; i--)
            {
                {
                    var tmp = array[0];
                    array[0] = array[i];
                    array[i] = tmp;
                }

                var j = 0;
                var k = 0;

                while (true)
                {
                    var left = 2 * j + 1;
                    var right = left + 1;

                    if (left >= i)
                    {
                        break;
                    }

                    {
                        var _p = array[left];
                        var _c = array[k];
                        if (_c != null && _c.CompareTo(_p) < 0
                         || _p != null && _p.CompareTo(_c) > 0)
                        {
                            k = left;
                        }
                    }

                    if (right < i)
                    {
                        var _p = array[right];
                        var _c = array[k];
                        if (_c != null && _c.CompareTo(_p) < 0
                         || _p != null && _p.CompareTo(_c) > 0)
                        {
                            k = right;
                        }
                    }

                    if (k == j)
                    {
                        break;
                    }

                    {
                        var tmp = array[k];
                        array[k] = array[j];
                        array[j] = tmp;
                    }

                    j = k;
                }
            }
        }
    }
}
