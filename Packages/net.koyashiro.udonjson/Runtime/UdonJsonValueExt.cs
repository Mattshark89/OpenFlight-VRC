using Koyashiro.UdonJson.Internal;

namespace Koyashiro.UdonJson
{
    using Koyashiro.UdonList;
    using Koyashiro.UdonDictionary;

    public static class UdonJsonValueExt
    {
        private const string ERR_INVALID_KIND = "Invalid kind";

        public static UdonJsonValueKind GetKind(this UdonJsonValue value)
        {
            return (UdonJsonValueKind)(((object[])(object)value)[0]);
        }

        public static string AsString(this UdonJsonValue v)
        {
            if (v.GetKind() != UdonJsonValueKind.String)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return (string)v.GetValueUnchecked();
        }

        public static double AsNumber(this UdonJsonValue v)
        {
            if (v.GetKind() != UdonJsonValueKind.Number)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return (double)v.GetValueUnchecked();
        }

        public static bool AsBool(this UdonJsonValue v)
        {
            if (v.GetKind() != UdonJsonValueKind.True && v.GetKind() != UdonJsonValueKind.False)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return (bool)v.GetValueUnchecked();
        }

        public static object AsNull(this UdonJsonValue v)
        {
            if (v.GetKind() != UdonJsonValueKind.Null)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return null;
        }

        public static int Count(this UdonJsonValue v)
        {
            if (v.GetKind() == UdonJsonValueKind.Object)
            {
                return v.AsDictionary().Count();
            }

            if (v.GetKind() == UdonJsonValueKind.Array)
            {
                return v.AsList().Count();
            }

            ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            return default;
        }

        public static string[] Keys(this UdonJsonValue v)
        {
            if (v.GetKind() != UdonJsonValueKind.Object)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return v.AsDictionary().Keys();
        }

        public static UdonJsonValue GetValue(this UdonJsonValue v, string key)
        {
            if (v.GetKind() != UdonJsonValueKind.Object)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return v.AsDictionary().GetValue(key);
        }

        public static UdonJsonValue GetValue(this UdonJsonValue v, int index)
        {
            if (v.GetKind() != UdonJsonValueKind.Array)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            return (UdonJsonValue)(v.AsList().GetValue(index));
        }

        public static bool TryGetValue(this UdonJsonValue v, string key, out UdonJsonValue value)
        {
            if (v.GetKind() != UdonJsonValueKind.Object)
            {
                value = default;
                return false;
            }

            return v.AsDictionary().TryGetValue(key, out value);
        }

        public static void SetValue(this UdonJsonValue v, string key, UdonJsonValue value)
        {
            if (v.GetKind() != UdonJsonValueKind.Object)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            v.AsDictionary().SetValue(key, value);
        }

        public static void SetValue(this UdonJsonValue v, int key, UdonJsonValue value)
        {
            if (v.GetKind() != UdonJsonValueKind.Array)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            v.AsList().SetValue(key, value);
        }

        public static void SetValue(this UdonJsonValue v, string key, string value)
        {
            v.SetValue(key, UdonJsonValue.NewString(value));
        }

        public static void SetValue(this UdonJsonValue v, int index, string value)
        {
            v.SetValue(index, UdonJsonValue.NewString(value));
        }

        public static void SetValue(this UdonJsonValue v, string key, double value)
        {
            v.SetValue(key, UdonJsonValue.NewNumber(value));
        }

        public static void SetValue(this UdonJsonValue v, int index, double value)
        {
            v.SetValue(index, UdonJsonValue.NewNumber(value));
        }

        public static void SetValue(this UdonJsonValue v, string key, bool value)
        {
            if (value)
            {
                v.SetValue(key, UdonJsonValue.NewTrue());
            }
            else
            {
                v.SetValue(key, UdonJsonValue.NewFalse());
            }
        }

        public static void SetValue(this UdonJsonValue v, int index, bool value)
        {
            if (value)
            {
                v.SetValue(index, UdonJsonValue.NewTrue());
            }
            else
            {
                v.SetValue(index, UdonJsonValue.NewFalse());
            }
        }

        public static void SetNullValue(this UdonJsonValue v, string key)
        {
            v.SetValue(key, UdonJsonValue.NewNull());
        }

        public static void SetNullValue(this UdonJsonValue v, int index)
        {
            v.SetValue(index, UdonJsonValue.NewNull());
        }

        public static void AddValue(this UdonJsonValue v, UdonJsonValue value)
        {
            if (v.GetKind() != UdonJsonValueKind.Array)
            {
                ExceptionHelper.ThrowArgumentException(ERR_INVALID_KIND);
            }

            v.AsList().Add(value);
        }

        public static void AddValue(this UdonJsonValue v, string value)
        {
            v.AddValue(UdonJsonValue.NewString(value));
        }

        public static void AddValue(this UdonJsonValue v, double value)
        {
            v.AddValue(UdonJsonValue.NewNumber(value));
        }

        public static void AddValue(this UdonJsonValue v, bool value)
        {
            if (value)
            {
                v.AddValue(UdonJsonValue.NewTrue());
            }
            else
            {
                v.AddValue(UdonJsonValue.NewFalse());
            }
        }

        public static void AddNullValue(this UdonJsonValue v)
        {
            v.AddValue(UdonJsonValue.NewNull());
        }

        private static UdonDictionary<string, UdonJsonValue> AsDictionary(this UdonJsonValue v)
        {
            return (UdonDictionary<string, UdonJsonValue>)v.GetValueUnchecked();
        }

        private static UdonList<UdonJsonValue> AsList(this UdonJsonValue v)
        {
            return (UdonList<UdonJsonValue>)v.GetValueUnchecked();
        }

        private static object GetValueUnchecked(this UdonJsonValue v)
        {
            return (UdonJsonValueKind)(((object[])(object)v)[1]);
        }
    }
}
