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
        messageFormat: "Consider using '{0}' loop when iterating over a '{1}'",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ForStatement, SyntaxKind.ForEachStatement);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        switch (context.Node)
        {
            case ForEachStatementSyntax forEachStatement:
                {
                    var info = context.SemanticModel.GetTypeInfo(forEachStatement.Expression);

                    if (info.Type != null && info.Type.TypeKind == TypeKind.Array)
                    {
                        var diagnostic = Diagnostic.Create(Rule, forEachStatement.GetLocation(), "for", "Array");
                        context.ReportDiagnostic(diagnostic);
                    }

                    break;
                }

            case ForStatementSyntax forStatement:
                {
                    //TODO
                    break;
                }
        }
    }
}