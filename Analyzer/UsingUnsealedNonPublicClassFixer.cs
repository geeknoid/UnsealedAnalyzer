// © Microsoft Corporation. All rights reserved.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer
{
    /// <summary>
    /// Seal classes that could be, for better perf.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingUnsealedNonPublicClassFixer))]
    [Shared]
    public class UsingUnsealedNonPublicClassFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagDescriptors.UsingUnsealedNonPublicClass.Id);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root!.FindNode(context.Span);
            var declaration = editor.Generator.GetDeclaration(node);

            if (declaration != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.SealClass,
                        createChangedDocument: async (cancellationToken) => await MakeSealedAsync(editor, declaration).ConfigureAwait(false),
                        equivalenceKey: nameof(Resources.SealClass)),
                    context.Diagnostics);
            }
        }

        private static Task<Document> MakeSealedAsync(DocumentEditor editor, SyntaxNode declaration)
        {
            DeclarationModifiers modifiers = editor.Generator.GetModifiers(declaration);
            editor.SetModifiers(declaration, modifiers + DeclarationModifiers.Sealed);
            return Task.FromResult(editor.GetChangedDocument());
        }
    }
}
