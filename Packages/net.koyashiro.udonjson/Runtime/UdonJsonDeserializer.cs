using UdonSharp;

namespace Koyashiro.UdonJson
{
    [UnityEngine.AddComponentMenu("")]
    public class UdonJsonDeserializer : UdonSharpBehaviour
    {
        private static UdonJsonDeserializer New(string s)
        {
            var input = s.ToCharArray();
            var pos = 0;
            object output = default;
            object error = default;
            return (UdonJsonDeserializer)(object)(new object[] { input, pos, output, error });
        }

        public static bool TryDeserialize(string input, out UdonJsonValue output)
        {
            return TryDeserialize(input, out output, out var _error);
        }

        public static bool TryDeserialize(string input, out UdonJsonValue output, out string error)
        {
            var des = UdonJsonDeserializer.New(input);

            if (des.TryDeserializeValue())
            {
                output = (UdonJsonValue)des.GetOutput();
                error = default;
                return true;
            }
            else
            {
                output = default;
                error = $"{des.GetError()}: input: \"{new string(des.GetInput()).Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")}\", pos: {des.GetPos()}";
                return false;
            }
        }
    }
}
