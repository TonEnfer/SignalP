using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SignalP.Protocol.Protobuf.IntegrationTests.Helpers;
using SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;
using Xunit;

namespace SignalP.Protocol.Protobuf.IntegrationTests;

public class ServerStreamingTests(WebAppFactory app): IClassFixture<WebAppFactory>
{
    private WebAppFactory App { get; } = app;

    private HttpClient Client { get; } = app.CreateClient();

    [Fact]
    public async Task GetStringStreamFromServer()
    {
        await using var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();

        var received = hubClient.StreamAsync<string>(nameof(TestHub.StreamStringsToClient));

        (await received.ToListAsync()).Should().BeEquivalentTo(Enumerable.Range(0, 10).Select(x => x.ToString()));
        await hubClient.StopAsync();
    }

    [Fact]
    public async Task GetProtobufNetObjectsStreamFromServer()
    {
        await using var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();

        var testObj = new TestObject
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

        var received = hubClient.StreamAsync<TestObject>(
            nameof(TestHub.SteamProtobufNetObjectsToClient),
            testObj
        );

        (await received.ToListAsync())
            .Should()
            .HaveCount(10)
            .And
            .AllSatisfy(x => x.Should().BeEquivalentTo(testObj));

        await hubClient.StopAsync();
    }

    [Fact]
    public async Task GetGoogleProtobufObjectsStreamFromServer()
    {
        await using var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();

        var testObj = new GoogleProtobufMessage
        {
            Field1 = 42,
            Field2 = "testString\n\r%^@*"
        };

        var received = hubClient.StreamAsync<GoogleProtobufMessage>(
            nameof(TestHub.SteamGoogleProtobufObjectsToClient),
            testObj
        );

        (await received.ToListAsync())
            .Should()
            .HaveCount(10)
            .And
            .AllSatisfy(x => x.Should().BeEquivalentTo(testObj));

        await hubClient.StopAsync();
    }
}