using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace PleasantUI.Localization.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingResxKeyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "PUILOC102",
        "Missing localization key",
        "Localization key '{0}' is used in code but is missing from the .resx files",
        "Localization",
        DiagnosticSeverity.Warning,
        true
    );

    private static readonly Regex CultureSuffixRegex = new(@"\.[a-z]{2}(-[A-Z]{2,4})?(-[A-Z][a-z]{3})?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private enum TargetMethodType
    {
        Tr,
        TrDefault,
        TryGetString,
        BindingCreate
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            AnalyzerConfigOptions globalOptions =
                compilationContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            if (globalOptions.TryGetValue("build_property.PleasantUIDisableResxAnalyzer", out string? msbuildValue) &&
                string.Equals(msbuildValue.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                return;

            HashSet<string?> existingKeys = [];

            List<AdditionalText> resxFiles = compilationContext.Options.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".resx", StringComparison.OrdinalIgnoreCase))
                .Where(f => !CultureSuffixRegex.IsMatch(Path.GetFileNameWithoutExtension(f.Path)))
                .ToList();

            foreach (AdditionalText? file in resxFiles)
            {
                SourceText? sourceText = file.GetText(compilationContext.CancellationToken);
                if (sourceText == null) continue;
                try
                {
                    XDocument doc = XDocument.Parse(sourceText.ToString());
                    IEnumerable<XElement>? dataElements = doc.Root?.Elements("data");

                    if (dataElements == null)
                        continue;

                    foreach (XElement? element in dataElements)
                    {
                        string? name = element.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(name)) existingKeys.Add(name?.Trim());
                    }
                }
                catch
                {
                    // ignored
                }
            }

            compilationContext.RegisterSemanticModelAction(semanticContext =>
            {
                AnalyzeLocalizerCalls(
                    semanticContext.SemanticModel,
                    existingKeys,
                    semanticContext.ReportDiagnostic,
                    semanticContext.CancellationToken);
            });

            compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                AnalyzePropertyDeclaration(
                    (PropertyDeclarationSyntax)syntaxContext.Node,
                    syntaxContext.SemanticModel,
                    existingKeys,
                    syntaxContext.ReportDiagnostic,
                    syntaxContext.CancellationToken);
            }, SyntaxKind.PropertyDeclaration);

            compilationContext.RegisterAdditionalFileAction(fileContext =>
            {
                if (Path.GetExtension(fileContext.AdditionalFile.Path)
                    .Equals(".axaml", StringComparison.OrdinalIgnoreCase))
                {
                    AnalyzeAxamlFile(
                        fileContext.AdditionalFile,
                        existingKeys,
                        fileContext.ReportDiagnostic,
                        fileContext.CancellationToken);
                }
            });
        });
    }

    private static void AnalyzeLocalizerCalls(
        SemanticModel model,
        HashSet<string?> existingKeys,
        Action<Diagnostic> reportDiagnostic,
        System.Threading.CancellationToken cancellationToken)
    {
        SyntaxNode root = model.SyntaxTree.GetRoot(cancellationToken);

        foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.ArgumentList.Arguments.Count == 0) continue;

            ISymbol? symbol = model.GetSymbolInfo(invocation, cancellationToken).Symbol
                              ?? model.GetSymbolInfo(invocation.Expression, cancellationToken).Symbol;

            if (symbol is not IMethodSymbol methodSymbol ||
                !IsTargetLocalizationMethod(methodSymbol, out TargetMethodType methodType))
                continue;

            SeparatedSyntaxList<ArgumentSyntax> args = invocation.ArgumentList.Arguments;
            ExpressionSyntax keyExpr = args[0].Expression;

            string? rawKey = TryGetStringArgument(model, keyExpr);
            if (string.IsNullOrEmpty(rawKey)) continue;

            string? context = ExtractContextFromArguments(args, methodType, model);
            string combinedKey = !string.IsNullOrEmpty(context) ? $"{context}/{rawKey}" : rawKey!;

            if (!existingKeys.Contains(combinedKey))
                reportDiagnostic(Diagnostic.Create(Rule, keyExpr.GetLocation(), combinedKey));
        }
    }

    private static void AnalyzePropertyDeclaration(
        PropertyDeclarationSyntax propDecl,
        SemanticModel model,
        HashSet<string?> existingKeys,
        Action<Diagnostic> reportDiagnostic,
        System.Threading.CancellationToken cancellationToken)
    {
        ExpressionSyntax? initializer = propDecl.Initializer?.Value ?? propDecl.ExpressionBody?.Expression;
        if (initializer == null) return;

        string? key = TryGetStringArgument(model, initializer);
        if (string.IsNullOrEmpty(key)) return;

        if (model.GetDeclaredSymbol(propDecl, cancellationToken) is not { } propertySymbol)
            return;

        bool isLocalizationProperty = false;

        if (propertySymbol.Name.Contains("TitleKey"))
            isLocalizationProperty = true;
        else if (propertySymbol.IsOverride)
        {
            IPropertySymbol? current = propertySymbol.OverriddenProperty;
            while (current != null)
            {
                if (current.Name.Contains("TitleKey"))
                {
                    isLocalizationProperty = true;
                    break;
                }

                current = current.OverriddenProperty;
            }
        }

        if (isLocalizationProperty && !existingKeys.Contains(key ?? string.Empty))
            reportDiagnostic(Diagnostic.Create(Rule, initializer.GetLocation(), key));
    }

    private static bool IsTargetLocalizationMethod(IMethodSymbol method, out TargetMethodType type)
    {
        type = default;
        string typeName = method.ContainingType?.Name ?? string.Empty;
        switch (typeName)
        {
            case "LocalizeBinding" when method.Name == "Create":
                type = TargetMethodType.BindingCreate;
                return true;
            case "Localizer":
                switch (method.Name)
                {
                    case "Tr":
                        type = TargetMethodType.Tr;
                        return true;
                    case "TrDefault":
                        type = TargetMethodType.TrDefault;
                        return true;
                    case "TryGetString":
                        type = TargetMethodType.TryGetString;
                        return true;
                }

                break;
        }

        return false;
    }

    private static string? ExtractContextFromArguments(SeparatedSyntaxList<ArgumentSyntax> args,
        TargetMethodType methodType, SemanticModel model)
    {
        if (methodType == TargetMethodType.BindingCreate && args.Count > 1)
            return TryGetStringArgument(model, args[1].Expression);
        
        ArgumentSyntax? contextArg = args.FirstOrDefault(a => a.NameColon?.Name.Identifier.ValueText == "context");
        
        if (contextArg != null)
            return TryGetStringArgument(model, contextArg.Expression);
        
        return methodType switch
        {
            TargetMethodType.Tr when args.Count > 1 && args[1].NameColon == null => TryGetStringArgument(model,
                args[1].Expression),
            TargetMethodType.TrDefault when args.Count > 2 && args[2].NameColon == null => TryGetStringArgument(model,
                args[2].Expression),
            _ => null
        };
    }

    private static void AnalyzeAxamlFile(AdditionalText axamlFile, HashSet<string?> existingKeys,
        Action<Diagnostic> reportDiagnostic, System.Threading.CancellationToken cancellationToken)
    {
        SourceText? sourceText = axamlFile.GetText(cancellationToken);
        if (sourceText == null) return;

        try
        {
            string textContent = sourceText.ToString();
            XDocument doc = XDocument.Parse(textContent, LoadOptions.SetLineInfo);
            if (doc.Root == null) return;

            foreach (XElement element in doc.Descendants())
            {
                if (cancellationToken.IsCancellationRequested) return;

                foreach (XAttribute attribute in element.Attributes())
                {
                    string value = attribute.Value.Trim();
                    if (!value.StartsWith("{", StringComparison.Ordinal) || !value.Contains("Localize "))
                        continue;

                    (string Key, string? Context)? parsed = ExtractKeyFromExtension(value);

                    if (parsed == null)
                        continue;

                    string combinedKey = !string.IsNullOrEmpty(parsed.Value.Context)
                        ? $"{parsed.Value.Context}/{parsed.Value.Key}"
                        : parsed.Value.Key;

                    if (existingKeys.Contains(combinedKey))
                        continue;

                    System.Xml.IXmlLineInfo lineInfo = attribute;

                    if (!lineInfo.HasLineInfo())
                        continue;

                    int lineNumber = lineInfo.LineNumber - 1;

                    TextLine textLine = sourceText.Lines[lineNumber];
                    string lineText = textLine.ToString();

                    int indexInLine = lineText.IndexOf(attribute.Value, StringComparison.Ordinal);
                    if (indexInLine == -1) indexInLine = lineInfo.LinePosition - 1;

                    int startPosition = textLine.Start + indexInLine;
                    int length = attribute.Value.Length;

                    TextSpan span = new(startPosition, length);
                    LinePositionSpan lineSpan = new(
                        new LinePosition(lineNumber, indexInLine),
                        new LinePosition(lineNumber, indexInLine + length)
                    );

                    Location loc = Location.Create(axamlFile.Path, span, lineSpan);
                    reportDiagnostic(Diagnostic.Create(Rule, loc, combinedKey));
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static (string Key, string? Context)? ExtractKeyFromExtension(string extensionValue)
    {
        string content = extensionValue.Trim('{', '}').Trim();
        int firstSpace = content.IndexOf(' ');

        if (firstSpace == -1)
            return null;

        string argumentsPart = content.Substring(firstSpace + 1).Trim();
        List<string> arguments = SplitXamlArguments(argumentsPart);

        if (arguments.Count == 0)
            return null;

        string rawKey = arguments[0].Trim().Trim('\'', '"');

        if (rawKey.StartsWith("{", StringComparison.Ordinal) || rawKey.Contains("LocKey."))
            return null;

        string? context = null;
        for (int i = 1; i < arguments.Count; i++)
        {
            string arg = arguments[i].Trim();
            if (arg.StartsWith("Context", StringComparison.OrdinalIgnoreCase) && arg.Contains("="))
                context = arg.Substring(arg.IndexOf('=') + 1).Trim('\'', '"', ' ');
        }

        return (rawKey, context);
    }

    private static string? TryGetStringArgument(SemanticModel model, ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal)
            return literal.Token.ValueText;

        return model.GetConstantValue(expression).Value?.ToString();
    }

    private static List<string> SplitXamlArguments(string p)
    {
        List<string> r = [];
        int braceCount = 0;
        int startIdx = 0;

        for (int i = 0; i < p.Length; i++)
        {
            switch (p[i])
            {
                case '{':
                    braceCount++;
                    break;
                case '}':
                    braceCount--;
                    break;
                case ',' when braceCount == 0:
                    r.Add(p.Substring(startIdx, i - startIdx));
                    startIdx = i + 1;
                    break;
            }
        }

        if (startIdx < p.Length)
            r.Add(p.Substring(startIdx));

        return r;
    }
}