using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SignalP.Protocol.Protobuf;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ProtobufProtocolDependencyInjectionExtensions
{
#if !NETSTANDARD
    [RequiresUnreferencedCode("Protobuf does not currently support trimming or native AOT.", Url = "https://aka.ms/aspnet/trimming")]
#endif
    public static TBuilder AddProtobufProtocol<TBuilder>(this TBuilder builder)
        where TBuilder: ISignalRBuilder
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHubProtocol, ProtobufHubProtocol>());
        return builder;
    }
}