public static class StringExt
{
    public static string TrimEnd(this string source, string value)
        => source.EndsWith(value) ? source.Remove(source.LastIndexOf(value)) : source;
}
