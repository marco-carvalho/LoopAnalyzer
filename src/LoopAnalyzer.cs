using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoopAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LoopAnalyzer";
    private static readonly LocalizableString Title = "Loop usage recommendation";
    private static readonly LocalizableString MessageFormat = "Consider using {0} loop for {1}";
    private const string Category = "Performance";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ForStatement, SyntaxKind.ForEachStatement);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        var semanticModel = context.SemanticModel;

        switch (node)
        {
            case ForStatementSyntax forStatement:
                {
                    var info = semanticModel.GetTypeInfo(forStatement.Declaration.Type);

                    if (info.Type != null && info.Type.OriginalDefinition.ToString() == "System.Collections.Generic.List<T>")
                    {
                        var diagnostic = Diagnostic.Create(Rule, forStatement.GetLocation(), "foreach", "List");
                        context.ReportDiagnostic(diagnostic);
                    }

                    break;
                }

            case ForEachStatementSyntax forEachStatement:
                {
                    var info = semanticModel.GetTypeInfo(forEachStatement.Expression);

                    if (info.Type != null && info.Type.TypeKind == TypeKind.Array)
                    {
                        var diagnostic = Diagnostic.Create(Rule, forEachStatement.GetLocation(), "for", "array");
                        context.ReportDiagnostic(diagnostic);
                    }

                    break;
                }
        }
    }
}
