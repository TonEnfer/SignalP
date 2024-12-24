using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalP.Protocol.Protobuf.IntegrationTests.Helpers;
using SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;
using Xunit;

namespace SignalP.Protocol.Protobuf.IntegrationTests;

public class ClientCallTests(WebAppFactory app): IClassFixture<WebAppFactory>
{
    private WebAppFactory App { get; } = app;

    [Fact]
    public async Task SingleStringToClient()
    {
        var hub = App.Services.GetRequiredService<IHubContext<TestHub, ITestHub>>();
        await using var hubClient = App.CreateHubClient();


        const string data = "testString";

        hubClient.On<string>(nameof(ITestHub.SingleStringToClient), s => s.Should().Be(data));

        await hubClient.StartAsync();

        await hub.Clients.All.SingleStringToClient(data);

        await hubClient.StopAsync();
    }

    [Fact]
    public async Task SingleProtobufNetObjectToClient()
    {
        var hub = App.Services.GetRequiredService<IHubContext<TestHub, ITestHub>>();
        await using var hubClient = App.CreateHubClient();

        var data = new TestObject
        {
            Field1 = 42,
            Field2 = "testString\n\r%^@*",
            Field3 = DateTime.UtcNow,
            Field4 = ["t1", "42"],
            Field5 = new TestObject
            {
                Field1 = int.MaxValue,
                Field2 = "testString\n\r%^@*",
                Field3 = DateTime.Today,
                Field4 = ["t1", "42"]
            }
        };

        hubClient.On<TestObject>(nameof(ITestHub.SignleProtobufNetObjectToClient), s => s.Should().BeEquivalentTo(data));
        await hubClient.StartAsync();

        await hub.Clients.All.SignleProtobufNetObjectToClient(data);

        await hubClient.StopAsync();
    }

    [Fact]
    public async Task SingleGoogleProtobufObjectToClient()
    {
        var hub = App.Services.GetRequiredService<IHubContext<TestHub, ITestHub>>();
        await using var hubClient = App.CreateHubClient();

        var data = new GoogleProtobufMessage
        {
            Field1 = int.MaxValue,
            Field2 = Guid.NewGuid().ToString()
        };

        hubClient.On<GoogleProtobufMessage>(nameof(ITestHub.SignleGoogleProtobufObjectsToClient), s => s.Should().BeEquivalentTo(data));

        await hubClient.StartAsync();

        await hub.Clients.All.SignleGoogleProtobufObjectsToClient(data);

        await hubClient.StopAsync();
    }

    [Fact]
    public async Task MultipleObjectToClient()
    {
        var hub = App.Services.GetRequiredService<IHubContext<TestHub, ITestHub>>();
        await using var hubClient = App.CreateHubClient();

        var stringParam = "testParam";

        var protobufNetParam = new TestObject
        {
            Field1 = 42,
            Field2 = "testString\n\r%^@*",
            Field3 = DateTime.UtcNow,
            Field4 = ["t1", "42"],
            Field5 = new TestObject
            {
                Field1 = int.MaxValue,
                Field2 = "testString\n\r%^@*",
                Field3 = DateTime.Today,
                Field4 = ["t1", "42"]
            }
        };

        var googleProtobufParam = new GoogleProtobufMessage
        {
            Field1 = int.MaxValue,
            Field2 = Guid.NewGuid().ToString()
        };

        var listParam = new List<DateTime>
        {
            DateTime.MaxValue,
            DateTime.MinValue,
            DateTime.Today,
            DateTime.UtcNow
        };

        hubClient.On<string, TestObject, GoogleProtobufMessage, List<DateTime>>(
            nameof(ITestHub.WithMultipleParameters),
            (s, o, arg3, arg4) =>
            {
                s.Should().Be(stringParam);
                o.Should().BeEquivalentTo(protobufNetParam);
                arg3.Should().BeEquivalentTo(googleProtobufParam);
                arg4.Should().BeEquivalentTo(listParam);
            }
        );

        await hubClient.StartAsync();

        await hub.Clients.All.WithMultipleParameters(stringParam, protobufNetParam, googleProtobufParam, listParam);

        await hubClient.StopAsync();
    }
}