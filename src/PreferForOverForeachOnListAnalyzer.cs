using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LoopAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferForOverForeachOnListAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ForEachLoopRule = new DiagnosticDescriptor(
        id: "LoopAnalyzer002",
        title: "Foreach loop usage recommendation",
        messageFormat: "Prefer 'for' over 'foreach' for 'List' iteration",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ForEachLoopRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeForEachLoop, SyntaxKind.ForEachStatement);
    }

    private static void AnalyzeForEachLoop(SyntaxNodeAnalysisContext context)
    {
        var forEachStatement = context.Node as ForEachStatementSyntax;

        var collectionTypeInfo = context.SemanticModel.GetTypeInfo(forEachStatement.Expression).Type;

        if (collectionTypeInfo?.OriginalDefinition.ToString() != "System.Collections.Generic.List<T>")
        {
            return;
        }

        var diagnostic = Diagnostic.Create(ForEachLoopRule, forEachStatement.ForEachKeyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
