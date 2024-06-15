using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LoopAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoopAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: "LoopAnalyzer",
        title: "Loop usage recommendation",
        messageFormat: "Prefer '{0}' over '{1}' for '{2}' iteration",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeForLoop, SyntaxKind.ForStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForEachLoop, SyntaxKind.ForEachStatement);
    }

    private static void AnalyzeForLoop(SyntaxNodeAnalysisContext context)
    {
        var forStatement = context.Node as ForStatementSyntax;

        if (forStatement.Condition is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (binaryExpression.Right is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Expression is not IdentifierNameSyntax identifier)
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(identifier).Type is not IArrayTypeSymbol)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, forStatement.ForKeyword.GetLocation(), "foreach", "for", "array");
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeForEachLoop(SyntaxNodeAnalysisContext context)
    {
        var forEachStatement = context.Node as ForEachStatementSyntax;

        var collectionTypeInfo = context.SemanticModel.GetTypeInfo(forEachStatement.Expression).Type;

        if (collectionTypeInfo?.OriginalDefinition.ToString() != "System.Collections.Generic.List<T>")
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, forEachStatement.ForEachKeyword.GetLocation(), "for", "foreach", "list");
        context.ReportDiagnostic(diagnostic);
    }
}
