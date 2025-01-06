using FluentValidation;
using Google.Protobuf.Reflection;
using SignalP.Annotations;
using SignalP.Tools.ProtocPlugin.Csharp.Helpers;

namespace SignalP.Tools.ProtocPlugin.Csharp.Validators;

internal class MethodDescriptorValidator: AbstractValidator<MethodDescriptor>
{
    public MethodDescriptorValidator(
        PluginOptions options
    )
    {
        RuleFor(x => x.InputType)
            .SetValidator(descriptor => new MethodTypeValidator(descriptor, "input", options))
            .When(descriptor => descriptor.IsClientStreaming);

        RuleFor(x => x.OutputType)
            .SetValidator(descriptor => new MethodTypeValidator(descriptor, "output", options))
            .When(descriptor => descriptor.IsServerStreaming);

        // RuleFor(x => x.OutputType)
        //     .Must(x => x.IsEmptyType())
        //     .When(x => x.GetOptions()?.GetExtension(AnnotationsExtensions.Implementation)?.Side == Implementation.Types.Side.Client)
        //     .WithMessage(
        //         methodDescriptor => string.Format(
        //             "{0}({1}) : {2} in column={3} : {2} : {4}",
        //             methodDescriptor.File.Name,
        //             methodDescriptor.Declaration.StartLine,
        //             "warning",
        //             methodDescriptor.Declaration.StartColumn,
        //             "Return type for client implemented methods must be google.protobuf.Empty only. Other types are ignored"
        //         )
        //     )
        //     .WithSeverity(Severity.Warning);

        RuleFor(x => x.IsServerStreaming)
            .SetValidator(methodDescriptor => new MethodStreamingValidator(methodDescriptor));

        RuleFor(x => x.IsClientStreaming)
            .SetValidator(methodDescriptor => new MethodStreamingValidator(methodDescriptor));
    }

    private static string _invalidEmptyTypeMessageProvider(MethodDescriptor methodDescriptor, MessageDescriptor messageDescriptor, string direction)
    {
        return string.Format(
            "{0}({1}) : {2} in column={3} : {2} : {4}",
            methodDescriptor.File.Name,
            methodDescriptor.Declaration.StartLine,
            "error",
            methodDescriptor.Declaration.StartColumn,
            $"{messageDescriptor.FullName} is not a valid {direction} type for JSON and MessagePack in streaming methods"
        );
    }

    private class MethodTypeValidator: AbstractValidator<MessageDescriptor>
    {
        public MethodTypeValidator(MethodDescriptor method, string direction, PluginOptions options)
        {
            RuleFor(x => x)
                .Must(descriptor => !descriptor.IsEmptyType())
                .When(
                    _ =>
                        options.MessageStyle
                            is PluginOptions.MessageStyles.Json
                            or PluginOptions.MessageStyles.MessagePack
                )
                .WithMessage(messageDescriptor => _invalidEmptyTypeMessageProvider(method, messageDescriptor, direction))
                .WithSeverity(Severity.Error);
        }
    }

    private class MethodStreamingValidator: AbstractValidator<bool>
    {
        public MethodStreamingValidator(MethodDescriptor method)
        {
            RuleFor(x => x)
                .Equal(false)
                .When(_ => method.GetOptions().GetExtension(AnnotationsExtensions.Implementation)?.Side == Implementation.Types.Side.Client)
                .WithMessage(
                    $"{method.File.Name}({method.Declaration.StartLine}) : error in column={method.Declaration.StartColumn} : error : Streaming method must be implemented only in server-side"
                )
                .WithSeverity(Severity.Error);
        }
    }
}