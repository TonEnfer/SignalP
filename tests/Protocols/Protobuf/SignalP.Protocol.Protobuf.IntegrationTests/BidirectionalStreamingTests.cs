using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using SignalP.Protocol.Protobuf.IntegrationTests.Helpers;
using SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;
using Xunit;

namespace SignalP.Protocol.Protobuf.IntegrationTests;

public class BidirectionalStreamingTests(WebAppFactory app): IClassFixture<WebAppFactory>
{
    private WebAppFactory App { get; } = app;

    [Fact]
    public async Task BidirectionalStringStreaming()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();
        var data = GetData();
        var result = hubClient.StreamAsync<string>(nameof(TestHub.BidirectionalStringsStream), data);
        (await result.ToListAsync()).Should().BeEquivalentTo(await GetData().ToListAsync());

        async IAsyncEnumerable<string> GetData()
        {
            foreach (var i in Enumerable.Range(0, 10))
            {
                yield return i.ToString();
                await Task.Delay(1);
            }
        }
    }

    [Fact]
    public async Task BidirectionalProtobufNetStreaming()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();
        var data = GetData();

        var result = hubClient.StreamAsync<TestObject>(
            nameof(TestHub.BidirectionalProtobufNetStream),
            data
        );

        (await result.ToListAsync()).Should().BeEquivalentTo(await GetData().ToListAsync());

        async IAsyncEnumerable<TestObject> GetData()
        {
            var data = new TestObject
            {
                Field1 = 42,
                Field2 = "testString\n\r%^@*",
                Field3 = DateTime.UnixEpoch,
                Field4 = ["t1", "42"],
                Field5 = new TestObject
                {
                    Field1 = int.MaxValue,
                    Field2 = "testString\n\r%^@*",
                    Field3 = DateTime.Today,
                    Field4 = ["t1", "42"]
                }
            };

            foreach (var i in Enumerable.Range(0, 10))
            {
                yield return data;
                await Task.Delay(1);
            }
        }
    }

    [Fact]
    public async Task BidirectionalGoogleProtobufStreaming()
    {
        var hubClient = App.CreateHubClient();
        await hubClient.StartAsync();
        var data = GetData();

        var result = hubClient.StreamAsync<GoogleProtobufMessage>(
            nameof(TestHub.BidirectionalGoogleProtobufStream),
            data
        );

        (await result.ToListAsync()).Should().BeEquivalentTo(await GetData().ToListAsync());

        async IAsyncEnumerable<GoogleProtobufMessage> GetData()
        {
            var data = new GoogleProtobufMessage
            {
                Field1 = 42,
                Field2 = "testString\n\r%^@*"
            };

            foreach (var i in Enumerable.Range(0, 10))
            {
                yield return data;
                await Task.Delay(1);
            }
        }
    }
}