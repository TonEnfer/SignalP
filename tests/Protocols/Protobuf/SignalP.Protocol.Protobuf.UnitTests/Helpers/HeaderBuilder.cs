namespace SignalP.Protocol.Protobuf.UnitTests.Helpers;

internal static class HeaderBuilder
{
    public static IDictionary<string, string> ToHeadersDictionary(this IList<string> headers)
    {
        var result = new Dictionary<string, string>(headers.Count / 2);

        for (var i = 0; i < headers.Count; i += 2)
        {
            result.Add(headers[i], headers[i + 1]);
        }

        return result;
    }
}