namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers.TypeConverters;

internal class JsonTypeConverter: TypeConverterBase
{
    protected override string GetAnyTypeName()
    {
        return "global::System.Text.Json.JsonDocument";
    }
}