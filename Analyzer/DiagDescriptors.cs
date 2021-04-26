// © Microsoft Corporation. All rights reserved.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]
[assembly: InternalsVisibleTo("Analyzer.Test")]

namespace Analyzer
{
    internal static class DiagDescriptors
    {
        public static DiagnosticDescriptor UsingUnsealedNonPublicClass { get; } = new (
            id: "A0001",
            messageFormat: Resources.UsingUnsealedNonPublicClassMessage,
            title: Resources.UsingUnsealedNonPublicClassTitle,
            category: "Performance",
            description: Resources.UsingUnsealedNonPublicClassDescription,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
