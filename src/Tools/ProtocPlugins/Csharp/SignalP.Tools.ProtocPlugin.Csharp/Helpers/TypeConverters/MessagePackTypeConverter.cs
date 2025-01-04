namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers.TypeConverters;

internal class MessagePackTypeConverter: TypeConverterBase
{
    protected override string GetAnyTypeName()
    {
        return "global::System.Dynamic.ExpandoObject";
    }
}