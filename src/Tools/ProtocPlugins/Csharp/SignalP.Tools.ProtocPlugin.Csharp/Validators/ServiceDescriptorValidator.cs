using FluentValidation;
using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Validators;

internal class ServiceDescriptorValidator: AbstractValidator<ServiceDescriptor>
{
    public ServiceDescriptorValidator(PluginOptions options)
    {
        RuleForEach(x => x.Methods).SetValidator(_ => new MethodDescriptorValidator(options));
    }
}