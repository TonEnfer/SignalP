using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalP.Protocol.Protobuf.IntegrationTests.Helpers;

internal static class HubClientFactory
{
    public static HubConnection CreateHubClient(this WebAppFactory appFactory)
    {
        using var httpClient = appFactory.CreateClient();

        return new HubConnectionBuilder()
            .AddProtobufProtocol()
            .WithUrl(
                new Uri(httpClient.BaseAddress!, "/test-hub"),
                options => { options.HttpMessageHandlerFactory = _ => appFactory.Server.CreateHandler(); }
            )
            .WithAutomaticReconnect()
            .Build();
    }
}