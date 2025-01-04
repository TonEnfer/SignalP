using FluentValidation;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using SignalP.Annotations;
using SignalP.Tools.ProtocPlugin.Csharp.Generators;
using SignalP.Tools.ProtocPlugin.Csharp.Generators.Models;
using SignalP.Tools.ProtocPlugin.Csharp.Validators;
using System.Diagnostics;
using System.Text;

namespace SignalP.Tools.ProtocPlugin.Csharp;

internal static class Program
{
    private static void Main()
    {
#if DEBUG
        Debugger.Launch();
#endif

        using var input = Console.OpenStandardInput();

        var extensions = new ExtensionRegistry
        {
            AnnotationsExtensions.Implementation,
            AnnotationsExtensions.HubRoute
        };

        var request = CodeGeneratorRequest
            .Parser.WithExtensionRegistry(extensions)
            .ParseFrom(input);

        var opt = PluginOptions.Parse(request.Parameter);

        var validator = new FileDescriptorValidator(opt);

        var files = FileDescriptor.BuildFromByteStrings(request.ProtoFile.Select(x => x.ToByteString()), extensions);
        var stringBuilder = new StringBuilder();
        var hasError = false;

        foreach (var file in files.Where(x => request.FileToGenerate.Contains(x.Name)))
        {
            var validationResult = validator.Validate(file);

            if (validationResult.IsValid)
            {
                continue;
            }

            using var errorStream = Console.OpenStandardError();
            using var stringWriter = new StreamWriter(errorStream);

            foreach (var validationFailure in validationResult.Errors.Where(x => x.Severity != Severity.Error))
            {
                stringWriter.WriteLine(validationFailure);
            }

            hasError |= validationResult.Errors.Any(x => x.Severity == Severity.Error);
            stringBuilder.AppendJoin('\n', validationResult.Errors.Where(x => x.Severity == Severity.Error));
            stringBuilder.AppendLine();
        }

        using var stdOut = Console.OpenStandardOutput();
        using var outStream = new CodedOutputStream(stdOut);
        var response = new CodeGeneratorResponse();

        var errors = stringBuilder.ToString();

        if (!string.IsNullOrWhiteSpace(errors))
        {
            response.Error = errors;
            response.WriteTo(outStream);

            if (hasError)
            {
                return;
            }
        }

        var generator = new ServiceGenerator(opt);


        foreach (var protoFile in files.Where(x => request.FileToGenerate.Contains(x.Name)))
        {
            response.File.Add(
                new CodeGeneratorResponse.Types.File
                {
                    Content = generator.GenerateServices(protoFile),
                    Name = protoFile.Name.Replace(".proto", ".SignalP.g.cs")
                }
            );
        }

        switch (opt.MessageStyle)
        {
            case PluginOptions.MessageStyles.Json:
                // var jsonMessageGenerator = new 
                break;
            case PluginOptions.MessageStyles.MessagePack:
                var msgPackMessageGenerator = new MessagePackModelGenerator(opt);

                foreach (var protoFile in files.Where(x => request.FileToGenerate.Contains(x.Name)))
                {
                    response.File.Add(
                        new CodeGeneratorResponse.Types.File
                        {
                            Content = msgPackMessageGenerator.GenerateModels(protoFile),
                            Name = protoFile.Name.Replace(".proto", ".g.cs")
                        }
                    );
                }

                break;
            case PluginOptions.MessageStyles.Protobuf:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        response.WriteTo(outStream);
    }
}