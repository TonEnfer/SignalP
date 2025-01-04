namespace SignalP.Tools.ProtocPlugin.Csharp;

internal class PluginOptions
{
    public enum MessageStyles
    {
        Json = 0,
        MessagePack = 1,
        Protobuf = 2
    }

    [Flags]
    public enum GeneratedSide
    {
        None = 0,
        Server = 1,
        Client = 2,
        Bath = Server | Client
    }

    public MessageStyles MessageStyle { get; set; }

    public GeneratedSide Generate { get; set; }

    public bool InternalAccess { get; set; } = true;

    public static PluginOptions Parse(string options)
    {
        var optionsArray = options.Split(',');

        var result = new PluginOptions();

        foreach (var se in optionsArray)
        {
            var strings = se.Split('=');

            if (strings.Length is not 2)
            {
                continue;
            }

            switch (strings[0].ToLowerInvariant())
            {
                case "message_style":
                    result.MessageStyle = Enum.TryParse<MessageStyles>(strings[1], true, out var styles)
                        ? styles
                        : MessageStyles.Protobuf;

                    break;
                case "generate":
                    result.Generate = Enum.TryParse<GeneratedSide>(strings[1], true, out var generation)
                        ? generation
                        : GeneratedSide.Bath;

                    break;
                case "internal_access":
                    result.InternalAccess = !bool.TryParse(strings[1], out var @internal) || @internal;
                    break;
            }
        }

        return result;
    }
}