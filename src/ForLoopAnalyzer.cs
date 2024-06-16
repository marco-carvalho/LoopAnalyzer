using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LoopAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ForLoopAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ForLoopRule = new DiagnosticDescriptor(
        id: "LoopAnalyzer001",
        title: "For loop usage recommendation",
        messageFormat: "Prefer 'foreach' over 'for' for 'Array' iteration",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
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

        if (context.SemanticModel.GetTypeInfo(identifier).Type is not IArrayTypeSymbol)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(ForLoopRule, forStatement.ForKeyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
