using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;

namespace SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;

[SuppressMessage("ReSharper", "AsyncApostle.AsyncMethodNamingHighlighting")]
public class TestHub: Hub<ITestHub>
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<string> StreamStringsToClient()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (var i = 0; i < 10; i++)
        {
            yield return i.ToString();
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<TestObject> SteamProtobufNetObjectsToClient(TestObject @object)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (var i = 0; i < 10; i++)
        {
            yield return @object;
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<GoogleProtobufMessage> SteamGoogleProtobufObjectsToClient(GoogleProtobufMessage message)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (var i = 0; i < 10; i++)
        {
            yield return message;
        }
    }

    public async IAsyncEnumerable<string> BidirectionalStringsStream(IAsyncEnumerable<string> strings)
    {
        await foreach (var se in strings)
        {
            yield return se;
        }
    }

    public async IAsyncEnumerable<TestObject> BidirectionalProtobufNetStream(IAsyncEnumerable<TestObject> strings)
    {
        await foreach (var se in strings)
        {
            yield return se;
        }
    }

    public async IAsyncEnumerable<GoogleProtobufMessage> BidirectionalGoogleProtobufStream(IAsyncEnumerable<GoogleProtobufMessage> strings)
    {
        await foreach (var se in strings)
        {
            yield return se;
        }
    }

    public async Task<List<string>> StringStreamFromClient(IAsyncEnumerable<string> strings)
    {
        var result = await strings.ToListAsync();
        return result;
    }

    public Task<List<TestObject>> ProtobufNetStreamFromClient(IAsyncEnumerable<TestObject> objects)
    {
        return objects.ToListAsync().AsTask();
    }

    public async Task<ListOfGoogleProtobufMessages> GoogleProtobufStreamFromClient(IAsyncEnumerable<GoogleProtobufMessage> objects)
    {
        return await objects.AggregateAsync(
            new ListOfGoogleProtobufMessages(),
            (list, message) =>
            {
                list.Messages.Add(message);
                return list;
            }
        );
    }
}

[SuppressMessage("ReSharper", "AsyncApostle.AsyncMethodNamingHighlighting")]
public interface ITestHub
{
    public Task SingleStringToClient(string message);

    public Task SignleProtobufNetObjectToClient(TestObject message);

    public Task SignleGoogleProtobufObjectsToClient(GoogleProtobufMessage message);

    public Task WithMultipleParameters(
        string message,
        TestObject protobufNetObject,
        GoogleProtobufMessage googleProtobufMessage,
        List<DateTime> list
    );
}