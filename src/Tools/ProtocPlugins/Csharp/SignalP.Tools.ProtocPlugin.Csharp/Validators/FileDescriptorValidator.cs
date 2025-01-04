using FluentValidation;
using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Validators;

internal class FileDescriptorValidator: AbstractValidator<FileDescriptor>
{
    public FileDescriptorValidator(PluginOptions options)
    {
        RuleForEach(x => x.Services).SetValidator(_ => new ServiceDescriptorValidator(options));
    }
}