namespace Koyashiro.UdonJson
{
    public static class UdonJsonValueKindExt
    {
        public static string ToKindString(this UdonJsonValueKind kind)
        {
            switch (kind)
            {
                case UdonJsonValueKind.String:
                    return nameof(UdonJsonValueKind.String);
                case UdonJsonValueKind.Number:
                    return nameof(UdonJsonValueKind.Number);
                case UdonJsonValueKind.Object:
                    return nameof(UdonJsonValueKind.Object);
                case UdonJsonValueKind.Array:
                    return nameof(UdonJsonValueKind.Array);
                case UdonJsonValueKind.True:
                    return nameof(UdonJsonValueKind.True);
                case UdonJsonValueKind.False:
                    return nameof(UdonJsonValueKind.False);
                case UdonJsonValueKind.Null:
                    return nameof(UdonJsonValueKind.Null);
                default:
                    return string.Empty;
            }
        }
    }
}
