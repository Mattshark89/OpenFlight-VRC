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
                    ser.Write(
                        v
                        .AsString()
                        .Replace("\"", "\\\"")
                        .Replace("\\\\", "\\")
                        .Replace("/", "\\/")
                        .Replace("\b", "\\b")
                        .Replace("\f", "\\f")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t")
                    );
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
