using SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;

namespace SignalP.Protocol.Protobuf.IntegrationTests.Program;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR().AddProtobufProtocol();
        var app = builder.Build();

        app.MapHub<TestHub>("/test-hub");

        app.Run();
    }
}