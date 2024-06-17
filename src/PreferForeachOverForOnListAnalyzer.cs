using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LoopAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferForeachOverForOnListAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ForLoopRule = new DiagnosticDescriptor(
        id: "LoopAnalyzer004",
        title: "For loop usage recommendation",
        messageFormat: "Prefer 'foreach' over 'for' for 'List' iteration",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ForLoopRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeForLoop, SyntaxKind.ForStatement);
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

        var collectionTypeInfo = context.SemanticModel.GetTypeInfo(identifier).Type;
        if (collectionTypeInfo?.OriginalDefinition.ToString() != "System.Collections.Generic.List<T>")
        {
            return;
        }

        var arraySymbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol;
        if (arraySymbol == null)
        {
            return;
        }

        foreach (var descendantNode in forStatement.Statement.DescendantNodes())
        {
            if (descendantNode is not AssignmentExpressionSyntax assignment)
            {
                continue;
            }
            if (assignment.Left is not ElementAccessExpressionSyntax elementAccess)
            {
                continue;
            }
            if (context.SemanticModel.GetSymbolInfo(elementAccess.Expression).Symbol?.Equals(arraySymbol) == false)
            {
                continue;
            }
            return;
        }

        var diagnostic = Diagnostic.Create(ForLoopRule, forStatement.ForKeyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
