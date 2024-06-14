using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LoopAnalyzerCodeFixProvider)), Shared]
public class LoopAnalyzerCodeFixProvider : CodeFixProvider
{
    private const string title = "Use recommended loop";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return ImmutableArray.Create(LoopAnalyzer.DiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var node = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<StatementSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => UseRecommendedLoopAsync(context.Document, node, c),
                equivalenceKey: title),
            diagnostic);
    }

    private async Task<Document> UseRecommendedLoopAsync(Document document, StatementSyntax loopStatement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root;

        if (loopStatement is ForStatementSyntax forStatement)
        {
            var listIdentifier = forStatement.Declaration.Variables.First().Identifier.Text;
            var itemType = forStatement.Declaration.Type;
            var newForEach = SyntaxFactory.ForEachStatement(
                itemType,
                "item",
                SyntaxFactory.IdentifierName(listIdentifier),
                forStatement.Statement);

            newRoot = root.ReplaceNode(forStatement, newForEach.WithAdditionalAnnotations(Formatter.Annotation));
        }
        else if (loopStatement is ForEachStatementSyntax forEachStatement)
        {
            var arrayIdentifier = forEachStatement.Expression;
            var itemType = forEachStatement.Type;
            var newFor = SyntaxFactory.ForStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                    .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("i")).WithInitializer(
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))))),
                SyntaxFactory.SeparatedList<ExpressionSyntax>(),
                SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression,
                    SyntaxFactory.IdentifierName("i"),
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        arrayIdentifier,
                        SyntaxFactory.IdentifierName("Length"))),
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName("i"))),
                forEachStatement.Statement);

            newRoot = root.ReplaceNode(forEachStatement, newFor.WithAdditionalAnnotations(Formatter.Annotation));
        }

        var newDocument = document.WithSyntaxRoot(newRoot);
        return newDocument;
    }
}
