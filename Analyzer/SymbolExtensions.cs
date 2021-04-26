// © Microsoft Corporation. All rights reserved.

using Microsoft.CodeAnalysis;

namespace Analyzer
{
    internal static class SymbolExtensions
    {
        /// <summary>
        /// True if the symbol is externally visible outside this assembly.
        /// </summary>
        public static bool IsExternallyVisible(this ISymbol symbol)
        {
            while (symbol.Kind != SymbolKind.Namespace)
            {
                switch (symbol.DeclaredAccessibility)
                {
                    // If we see anything private, then the symbol is private.
                    case Accessibility.NotApplicable:
                    case Accessibility.Private:
                        return false;

                    // If we see anything internal, then knock it down from public to
                    // internal.
                    case Accessibility.Internal:
                    case Accessibility.ProtectedAndInternal:
                        return false;
                }

                symbol = symbol.ContainingSymbol;
            }

            return true;
        }
    }
}
