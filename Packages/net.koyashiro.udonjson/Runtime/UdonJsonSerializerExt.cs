using UdonSharp;

namespace Koyashiro.UdonJson
{
    using Koyashiro.UdonList;

    static public class UdonJsonSerializerExt
    {
        public static void Serialize(this UdonJsonSerializer ser)
        {
            var input = ser.GetInput();
            ser.Write(input);
        }

        public static string GetOutput(this UdonJsonSerializer ser)
        {
            var chars = ser.GetBuf().ToArray();
            return new string(chars);
        }

        private static UdonJsonValue GetInput(this UdonJsonSerializer ser)
        {
            return (UdonJsonValue)(((object[])(object)ser)[0]);
        }

        private static UdonList<char> GetBuf(this UdonJsonSerializer ser)
        {
            return (UdonList<char>)(((object[])(object)ser)[1]);
        }

        [RecursiveMethod]
        private static void Write(this UdonJsonSerializer ser, UdonJsonValue v)
        {
            switch (v.GetKind())
            {
                case UdonJsonValueKind.String:
                    ser.Write('"');
                    var s = v.AsString().ToCharArray();
                    var start = 0;
                    for (var i = 0; i < s.Length; i++)
                    {
                        var c = s[i];
                        switch (c)
                        {
                            case '"':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\\"");
                                start = i + 1;
                                continue;
                            case '\\':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\\\");
                                start = i + 1;
                                continue;
                            case '\b':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\b");
                                start = i + 1;
                                continue;
                            case '\f':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\f");
                                start = i + 1;
                                continue;
                            case '\n':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\n");
                                start = i + 1;
                                continue;
                            case '\r':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\r");
                                start = i + 1;
                                continue;
                            case '\t':
                                ser.Write(new string(s, start, i - start));
                                ser.Write("\\t");
                                start = i + 1;
                                continue;
                        }

                        if (0x00 <= c && c <= 0x001f)
                        {
                            ser.Write(new string(s, start, i - start));
                            ser.Write($"\\u{((ushort)c):x4}");
                            start = i + 1;
                            continue;
                        }
                    }
                    ser.Write(new string(s, start, s.Length - start));
                    ser.Write('"');
                    break;
                case UdonJsonValueKind.Number:
                    ser.Write(v.AsNumber().ToString());
                    break;
                case UdonJsonValueKind.Object:
                    ser.Write('{');
                    var keys = v.Keys();
                    for (var i = 0; i < v.Count(); i++)
                    {
                        var key = keys[i];
                        ser.Write('"');
                        ser.Write(key);
                        ser.Write('"');
                        ser.Write(':');

                        var value = v.GetValue(key);
                        ser.Write(value);

                        if (i != v.Count() - 1)
                        {
                            ser.Write(',');
                        }
                    }
                    ser.Write('}');
                    break;
                case UdonJsonValueKind.Array:
                    ser.Write('[');
                    for (var i = 0; i < v.Count(); i++)
                    {
                        var value = v.GetValue(i);
                        ser.Write(value);

                        if (i != v.Count() - 1)
                        {
                            ser.Write(',');
                        }
                    }
                    ser.Write(']');
                    break;
                case UdonJsonValueKind.True:
                    ser.Write("true");
                    break;
                case UdonJsonValueKind.False:
                    ser.Write("false");
                    break;
                case UdonJsonValueKind.Null:
                    ser.Write("null");
                    break;
            }
        }

        private static void Write(this UdonJsonSerializer ser, char c)
        {
            ser.GetBuf().Add(c);
        }

        private static void Write(this UdonJsonSerializer ser, string s)
        {
            ser.GetBuf().AddRange(s.ToCharArray());
        }
    }
}
