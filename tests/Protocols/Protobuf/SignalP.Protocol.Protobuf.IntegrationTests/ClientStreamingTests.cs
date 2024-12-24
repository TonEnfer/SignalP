using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SignalP.Protocol.Protobuf.IntegrationTests.Helpers;
using SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;
using Xunit;

namespace SignalP.Protocol.Protobuf.IntegrationTests;

public class ClientStreamingTests(WebAppFactory app): IClassFixture<WebAppFactory>
{
    private WebAppFactory App { get; } = app;

    [Fact]
    public async Task ClientSendStringsStream()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();
        var data = Enumerable.Range(0, 10).Select(x => x.ToString()).ToList();
        var result = await hubClient.InvokeAsync<List<string>>(nameof(TestHub.StringStreamFromClient), data.ToAsyncEnumerable());
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task ClientSendProtobufNetStream()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();

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


        var result = await hubClient.InvokeAsync<List<TestObject>>(nameof(TestHub.ProtobufNetStreamFromClient), Enumerable.Repeat(data, 10).ToAsyncEnumerable());
        result.Should().BeEquivalentTo(Enumerable.Repeat(data, 10));
    }

    [Fact]
    public async Task ClientSendGoogleProtobufStream()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();

        var data = new GoogleProtobufMessage
        {
            Field1 = int.MaxValue,
            Field2 = Guid.NewGuid().ToString()
        };


        var result = await hubClient.InvokeAsync<ListOfGoogleProtobufMessages>(
            nameof(TestHub.GoogleProtobufStreamFromClient),
            Enumerable.Repeat(data, 10).ToAsyncEnumerable()
        );

        result.Messages.Should().BeEquivalentTo(Enumerable.Repeat(data, 10));
    }
}