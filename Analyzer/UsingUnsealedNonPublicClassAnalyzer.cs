// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingUnsealedNonPublicClassAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagDescriptors.UsingUnsealedNonPublicClass);

        public override void Initialize(AnalysisContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
#pragma warning disable RS1024 // Compare symbols correctly
#pragma warning disable CPR121 // Specify 'concurrencyLevel' and 'capacity' in the ConcurrentDictionary ctor.
                var candidates = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
                var baseTypes = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
#pragma warning restore CPR121 // Specify 'concurrencyLevel' and 'capacity' in the ConcurrentDictionary ctor.
#pragma warning restore RS1024 // Compare symbols correctly

                // The strategy is simple:
                //
                //   * Accumulate all the classes which are potential candidates to be sealed based on their type definition
                //   * For all classes in the compilation, accumulate all the base types for these classes
                //   * Once the above is all done, produce a diagnostic for any candidate found, which is not used a base type

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    if (symbolAnalysisContext.Symbol is INamedTypeSymbol t)
                    {
                        if (!t.IsSealed && !t.IsAbstract && !t.IsStatic)
                        {
                            if (t.IsExternallyVisible())
                            {
                                // if all constructors aren't externally visible, it's a candidate
                                bool visibleCtor = false;
                                foreach (var c in t.Constructors)
                                {
                                    if (c.IsExternallyVisible())
                                    {
                                        visibleCtor = true;
                                    }
                                }

                                if (!visibleCtor)
                                {
                                    _ = candidates.TryAdd(t, 0);
                                }
                            }
                            else
                            {
                                _ = candidates.TryAdd(t, 0);
                            }
                        }

                        // for any given class, capture all the base types it has
                        var b = t.BaseType;
                        while (b != null)
                        {
                            b = b.ConstructedFrom;
                            _ = baseTypes.TryAdd(b, 0);
                            b = b.BaseType;
                        }
                    }
                }, SymbolKind.NamedType);

                compilationStartAnalysisContext.RegisterCompilationEndAction(compilationAnalysisContext =>
                {
                    foreach (var kvp in candidates)
                    {
                        if (!baseTypes.ContainsKey(kvp.Key))
                        {
                            // any candidate that isn't used as a base type can be sealed
                            var diagnostic = Diagnostic.Create(
                                DiagDescriptors.UsingUnsealedNonPublicClass,
                                kvp.Key.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(),
                                kvp.Key.Name);

                            compilationAnalysisContext.ReportDiagnostic(diagnostic);
                        }
                    }
                });
            });
        }
    }
}
