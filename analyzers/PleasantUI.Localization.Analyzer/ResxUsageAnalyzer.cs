using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using TypeInfo = Microsoft.CodeAnalysis.TypeInfo;

namespace PleasantUI.Localization.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResxUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "PUILOC101",
        "Unused localization key",
        "Localization key '{0}' from file '{1}' is not used in the project",
        "Localization",
        DiagnosticSeverity.Warning,
        true,
        customTags: ["CompilationEnd"]
    );

    private static readonly Regex CultureSuffixRegex = new(@"\.[a-z]{2}(-[A-Z]{2,4})?(-[A-Z][a-z]{3})?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly ConcurrentHashSet<string> LibraryExclusions = LoadEmbeddedExclusions();

    private enum TargetMethodType
    {
        Tr,
        TrDefault,
        TryGetString,
        BindingCreate,
        Indexer
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            AnalyzerConfigOptions globalOptions = compilationContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
            
            if (globalOptions.TryGetValue("build_property.PleasantUIDisableResxAnalyzer", out string? msbuildValue) &&
                string.Equals(msbuildValue.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                return;
            
            Compilation compilation = compilationContext.Compilation;

            List<AdditionalText> resxFiles = compilationContext.Options.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".resx", StringComparison.OrdinalIgnoreCase))
                .Where(f => !IsLocalizationCultureFile(f.Path))
                .ToList();

            List<AdditionalText> axamlFiles = compilationContext.Options.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".axaml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!resxFiles.Any()) return;

            ConcurrentDictionary<string, (string FileName, AdditionalText FileText, int LineNumber)>
                trackedKeys = new();
            ConcurrentHashSet<string> normalizedTrackedKeys = new();
            ConcurrentHashSet<string> usedNormalizedKeys = new();

            foreach (AdditionalText? file in resxFiles)
            {
                SourceText? sourceText = file.GetText(compilationContext.CancellationToken);
                if (sourceText == null) continue;

                try
                {
                    XDocument doc = XDocument.Parse(sourceText.ToString(), LoadOptions.SetLineInfo);
                    IEnumerable<XElement>? dataElements = doc.Root?.Elements("data");
                    if (dataElements == null) continue;

                    foreach (XElement element in dataElements)
                    {
                        string? keyName = element.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(keyName)) continue;

                        if (ShouldIgnoreKey(element)) continue;

                        IXmlLineInfo lineInfo = element;
                        int line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 1;

                        if (trackedKeys.TryAdd(keyName ?? string.Empty, (Path.GetFileName(file.Path), file, line)))
                            normalizedTrackedKeys.Add(NormalizeKey(keyName));
                    }
                }
                catch
                {
                    // ignored
                }
            }

            if (!trackedKeys.Any()) return;

            foreach (AdditionalText axamlText in axamlFiles)
            {
                AnalyzeAxamlFile(axamlText, normalizedTrackedKeys, usedNormalizedKeys,
                    compilationContext.CancellationToken);
            }

            foreach (string exclusion in LibraryExclusions.ToEnumerable())
            {
                if (normalizedTrackedKeys.Contains(exclusion))
                    usedNormalizedKeys.Add(exclusion);
            }

            compilationContext.RegisterSemanticModelAction(semanticContext =>
            {
                AnalyzeLocalizerCalls(
                    semanticContext.SemanticModel,
                    compilation,
                    normalizedTrackedKeys,
                    usedNormalizedKeys,
                    semanticContext.CancellationToken);
            });

            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var keyPair in trackedKeys)
                {
                    string originalKey = keyPair.Key;
                    string normalizedKey = NormalizeKey(originalKey);

                    if (usedNormalizedKeys.Contains(normalizedKey))
                        continue;
                    
                    (string? fileName, AdditionalText? additionalText, int lineNumber) = keyPair.Value;
                    SourceText? sourceText = additionalText.GetText(endContext.CancellationToken);

                    Location location;
                    if (sourceText != null)
                    {
                        TextSpan? textSpan = FindKeySpan(sourceText, originalKey, lineNumber);
                        location = textSpan.HasValue
                            ? Location.Create(additionalText.Path, textSpan.Value,
                                sourceText.Lines.GetLinePositionSpan(textSpan.Value))
                            : Location.Create(additionalText.Path, new TextSpan(0, 0),
                                new LinePositionSpan(new LinePosition(Math.Max(0, lineNumber - 1), 0),
                                    new LinePosition(Math.Max(0, lineNumber - 1), 0)));
                    }
                    else
                    {
                        location = Location.Create(additionalText.Path, new TextSpan(0, 0),
                            new LinePositionSpan(new LinePosition(Math.Max(0, lineNumber - 1), 0),
                                new LinePosition(Math.Max(0, lineNumber - 1), 0)));
                    }

                    Diagnostic diagnostic = Diagnostic.Create(Rule, location, originalKey, fileName);
                    endContext.ReportDiagnostic(diagnostic);
                }
            });
        });
    }

    private static void AnalyzeLocalizerCalls(
        SemanticModel model,
        Compilation compilation,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = model.SyntaxTree.GetRoot(cancellationToken);

        foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.ArgumentList.Arguments.Count == 0) continue;

            ISymbol? symbol = model.GetSymbolInfo(invocation, cancellationToken).Symbol
                              ?? model.GetSymbolInfo(invocation.Expression, cancellationToken).Symbol;

            if (symbol is not IMethodSymbol methodSymbol || !IsLocalizationMethod(methodSymbol, out TargetMethodType methodType))
                continue;
            
            SeparatedSyntaxList<ArgumentSyntax> args = invocation.ArgumentList.Arguments;
            ProcessKeyExpression(args[0].Expression, args, methodType, model, compilation, normalizedTrackedKeys,
                usedNormalizedKeys, cancellationToken);
        }

        foreach (ElementAccessExpressionSyntax elementAccess in root.DescendantNodes()
                     .OfType<ElementAccessExpressionSyntax>())
        {
            if (elementAccess.ArgumentList.Arguments.Count == 0) continue;

            ISymbol? symbol = model.GetSymbolInfo(elementAccess, cancellationToken).Symbol;
            
            if (symbol is not IPropertySymbol { IsIndexer: true } propertySymbol || propertySymbol.ContainingType?.Name != "Localizer")
                continue;
            
            SeparatedSyntaxList<ArgumentSyntax> args = elementAccess.ArgumentList.Arguments;
            ProcessKeyExpression(args[0].Expression, args, TargetMethodType.Indexer, model, compilation,
                normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);
        }

        foreach (var creation in root.DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (creation.ArgumentList == null || creation.ArgumentList.Arguments.Count == 0) continue;

            SymbolInfo symbolInfo = model.GetSymbolInfo(creation, cancellationToken);
            INamedTypeSymbol? targetType = null;

            if (symbolInfo.Symbol is IMethodSymbol ctorSymbol)
                targetType = ctorSymbol.ContainingType;
            else if (creation is ImplicitObjectCreationExpressionSyntax)
                targetType = model.GetTypeInfo(creation, cancellationToken).Type as INamedTypeSymbol;

            if (targetType == null) continue;

            for (int i = 0; i < creation.ArgumentList.Arguments.Count; i++)
            {
                if (!TryTraceConstructorParameterToProperty(targetType, i))
                    continue;
                
                ExpressionSyntax argExpr = creation.ArgumentList.Arguments[i].Expression;
                string? incKey = IsLocKeyMember(argExpr, model, cancellationToken, out string? enumName)
                    ? enumName
                    : TryGetStringArgument(model, argExpr);

                if (!string.IsNullOrEmpty(incKey))
                    MarkAsUsed(incKey, normalizedTrackedKeys, usedNormalizedKeys);
            }
        }
    }

    private static void ProcessKeyExpression(
        ExpressionSyntax keyExpression,
        SeparatedSyntaxList<ArgumentSyntax> originalArgs,
        TargetMethodType methodType,
        SemanticModel model,
        Compilation compilation,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys,
        CancellationToken cancellationToken)
    {
        if (IsLocKeyMember(keyExpression, model, cancellationToken, out string? directKey))
        {
            MarkAsUsed(directKey, normalizedTrackedKeys, usedNormalizedKeys);
            return;
        }

        string? key = TryGetStringArgument(model, keyExpression);

        if (string.IsNullOrEmpty(key) && keyExpression is IdentifierNameSyntax or MemberAccessExpressionSyntax)
        {
            ISymbol? symbol = model.GetSymbolInfo(keyExpression, cancellationToken).Symbol;
            if (symbol is IPropertySymbol || symbol is IFieldSymbol)
            {
                INamedTypeSymbol? containingType = symbol.ContainingType;
                if (containingType != null)
                {
                    if (TryTracePropertyToConstructorParameter(symbol, containingType, out int paramIndex))
                        ProcessConstructorArguments(containingType, paramIndex, originalArgs, methodType, model,
                            normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);

                    ProcessInheritedAndLocalProperties(symbol, containingType, originalArgs, methodType, model,
                        compilation, normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);
                    
                    return;
                }
            }
        }

        if (string.IsNullOrEmpty(key) && keyExpression is IdentifierNameSyntax identifier)
        {
            ISymbol? localSymbol = model.GetSymbolInfo(identifier, cancellationToken).Symbol;
            if (localSymbol is IParameterSymbol { ContainingSymbol: IMethodSymbol containingMethod } parameterSymbol)
            {
                int paramIndex = parameterSymbol.Ordinal;

                if (containingMethod.MethodKind == MethodKind.Constructor)
                {
                    ProcessConstructorArguments(containingMethod.ContainingType, paramIndex, originalArgs, methodType,
                        model, normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);
                    return;
                }

                ProcessMethodWrapperArguments(containingMethod, paramIndex, originalArgs, methodType, model,
                    compilation, normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);
                
                return;
            }
        }

        if (string.IsNullOrEmpty(key))
            return;
        
        string? context = ExtractContext(originalArgs, methodType, model);
        MarkAsUsed(context != null ? $"{context}/{key}" : key, normalizedTrackedKeys, usedNormalizedKeys);
    }

    private static void ProcessMethodWrapperArguments(
        IMethodSymbol wrapperMethod,
        int parameterIndex,
        SeparatedSyntaxList<ArgumentSyntax> originalArgs,
        TargetMethodType methodType,
        SemanticModel model,
        Compilation compilation,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = model.SyntaxTree.GetRoot(cancellationToken);

        foreach (InvocationExpressionSyntax? invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.ArgumentList.Arguments.Count <= parameterIndex) continue;

            ISymbol? symbol = model.GetSymbolInfo(invocation, cancellationToken).Symbol;
            
            if (!SymbolEqualityComparer.Default.Equals(symbol, wrapperMethod))
                continue;
            
            ExpressionSyntax incomingArgExpr = invocation.ArgumentList.Arguments[parameterIndex].Expression;

            ProcessKeyExpression(incomingArgExpr, originalArgs, methodType, model, compilation,
                normalizedTrackedKeys, usedNormalizedKeys, cancellationToken);
        }
    }

    private static void ProcessConstructorArguments(
        INamedTypeSymbol targetClassType,
        int parameterIndex,
        SeparatedSyntaxList<ArgumentSyntax> originalArgs,
        TargetMethodType methodType,
        SemanticModel model,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys,
        CancellationToken cancellationToken)
    {
        string targetTypeName = targetClassType.Name;
        SyntaxNode root = model.SyntaxTree.GetRoot(cancellationToken);

        foreach (BaseObjectCreationExpressionSyntax? creation in root.DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (creation.ArgumentList == null || creation.ArgumentList.Arguments.Count <= parameterIndex) continue;

            bool isTargetCtor = false;
            SymbolInfo symbolInfo = model.GetSymbolInfo(creation, cancellationToken);

            if (symbolInfo.Symbol is IMethodSymbol ctorSymbol)
                isTargetCtor = SymbolEqualityComparer.Default.Equals(ctorSymbol.ContainingType, targetClassType);
            else if (creation is ImplicitObjectCreationExpressionSyntax)
            {
                TypeInfo typeInfo = model.GetTypeInfo(creation, cancellationToken);
                isTargetCtor = typeInfo.Type?.Name == targetTypeName;
            }

            if (!isTargetCtor)
                continue;
            
            ExpressionSyntax argExpr = creation.ArgumentList.Arguments[parameterIndex].Expression;
            string? incKey = IsLocKeyMember(argExpr, model, cancellationToken, out string? enumName)
                ? enumName
                : TryGetStringArgument(model, argExpr);

            if (string.IsNullOrEmpty(incKey))
                continue;
            
            string? context = ExtractContext(originalArgs, methodType, model);
            MarkAsUsed(context != null ? $"{context}/{incKey}" : incKey, normalizedTrackedKeys,
                usedNormalizedKeys);
        }
    }

    private static void ProcessInheritedAndLocalProperties(
        ISymbol baseMember,
        INamedTypeSymbol baseType,
        SeparatedSyntaxList<ArgumentSyntax> originalArgs,
        TargetMethodType methodType,
        SemanticModel baseModel,
        Compilation compilation,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys,
        CancellationToken cancellationToken)
    {
        IEnumerable<INamedTypeSymbol> allTypes = GetAllTypes(compilation.GlobalNamespace)
            .Where(t => SymbolEqualityComparer.Default.Equals(t, baseType) || IsSubclassOf(t, baseType));

        foreach (INamedTypeSymbol? typeSymbol in allTypes)
        {
            ISymbol? member = typeSymbol.GetMembers().FirstOrDefault(m => m.Name == baseMember.Name);
            if (member == null) continue;

            foreach (SyntaxReference? reference in member.DeclaringSyntaxReferences)
            {
                SyntaxNode syntax = reference.GetSyntax(cancellationToken);
                string? val = null;

                switch (syntax)
                {
                    case PropertyDeclarationSyntax { Initializer: not null } prop:
                        val = TryGetStringFromExpression(prop.Initializer.Value, baseModel,
                            cancellationToken);
                        break;
                    case PropertyDeclarationSyntax prop:
                    {
                        if (prop.ExpressionBody != null)
                            val = TryGetStringFromExpression(prop.ExpressionBody.Expression, baseModel,
                                cancellationToken);
                        break;
                    }
                    case VariableDeclaratorSyntax { Initializer: not null } field:
                        val = TryGetStringFromExpression(field.Initializer.Value, baseModel,
                            cancellationToken);
                        break;
                }

                if (string.IsNullOrEmpty(val))
                    continue;
                
                string? context = ExtractContext(originalArgs, methodType, baseModel);
                MarkAsUsed(context != null ? $"{context}/{val}" : val, normalizedTrackedKeys, usedNormalizedKeys);
            }
        }
    }

    private static bool TryTraceConstructorParameterToProperty(
        INamedTypeSymbol containingType,
        int parameterIndex)
    {
        foreach (IMethodSymbol? ctor in containingType.Constructors)
        {
            if (ctor.Parameters.Length <= parameterIndex) continue;
            IParameterSymbol targetParam = ctor.Parameters[parameterIndex];

            foreach (SyntaxNode syntax in ctor.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()))
            {
                if (syntax is not ConstructorDeclarationSyntax ctorDecl || ctorDecl.Body == null)
                    continue;
                
                foreach (AssignmentExpressionSyntax? assignment in ctorDecl.Body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if (assignment.Right is not IdentifierNameSyntax rightId || rightId.Identifier.ValueText != targetParam.Name)
                        continue;
                    
                    string memberName = assignment.Left switch
                    {
                        IdentifierNameSyntax leftId => leftId.Identifier.ValueText,
                        MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } ma => ma.Name
                            .Identifier.ValueText,
                        _ => string.Empty
                    };

                    if (string.IsNullOrEmpty(memberName))
                        continue;
                    
                    containingType.GetMembers().FirstOrDefault(m => m.Name == memberName);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryTracePropertyToConstructorParameter(
        ISymbol propertyOrFieldSymbol,
        INamedTypeSymbol containingType,
        out int parameterIndex)
    {
        parameterIndex = -1;

        foreach (IMethodSymbol? ctor in containingType.Constructors)
        {
            foreach (SyntaxNode syntax in ctor.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()))
            {
                if (syntax is not ConstructorDeclarationSyntax ctorDecl || ctorDecl.Body == null)
                    continue;
                
                foreach (AssignmentExpressionSyntax? assignment in ctorDecl.Body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if ((assignment.Left is not IdentifierNameSyntax leftId ||
                         leftId.Identifier.ValueText != propertyOrFieldSymbol.Name) &&
                        (assignment.Left is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } ma ||
                         ma.Name.Identifier.ValueText != propertyOrFieldSymbol.Name))
                        continue;

                    if (assignment.Right is not IdentifierNameSyntax rightId) continue;
                    
                    IParameterSymbol? param = ctor.Parameters.FirstOrDefault(p => p.Name == rightId.Identifier.ValueText);
                    
                    if (param == null) continue;
                    
                    parameterIndex = param.Ordinal;
                    return true;
                }
            }
        }

        return false;
    }

    private static void MarkAsUsed(string? rawKey, ConcurrentHashSet<string> normalizedTrackedKeys, ConcurrentHashSet<string> usedNormalizedKeys)
    {
        if (string.IsNullOrEmpty(rawKey)) return;
        string normalized = NormalizeKey(rawKey);

        if (normalizedTrackedKeys.Contains(normalized))
            usedNormalizedKeys.Add(normalized);
    }

    private static string? ExtractContext(SeparatedSyntaxList<ArgumentSyntax> args, TargetMethodType methodType,
        SemanticModel model)
    {
        return methodType switch
        {
            TargetMethodType.BindingCreate => args.Count > 1 ? TryGetStringArgument(model, args[1].Expression) : null,
            TargetMethodType.Tr => args.Count > 1 ? TryGetStringArgument(model, args[1].Expression) : null,
            TargetMethodType.TrDefault => args.Count > 2 ? TryGetStringArgument(model, args[2].Expression) : null,
            _ => null
        };
    }

    private static bool IsLocalizationMethod(IMethodSymbol method, out TargetMethodType methodType)
    {
        methodType = default;
        INamedTypeSymbol? containingType = method.ContainingType;
        if (containingType == null) return false;

        switch (containingType.Name)
        {
            case "LocalizeBinding" when method.Name == "Create":
                methodType = TargetMethodType.BindingCreate;
                return true;
            case "Localizer" when method.Name == "Tr":
                methodType = TargetMethodType.Tr;
                return true;
            case "Localizer" when method.Name == "TrDefault":
                methodType = TargetMethodType.TrDefault;
                return true;
            case "Localizer" when method.Name == "TryGetString":
                methodType = TargetMethodType.TryGetString;
                return true;
            default:
                return false;
        }
    }

    private static bool IsLocKeyMember(ExpressionSyntax expr, SemanticModel model, CancellationToken token,
        out string? keyName)
    {
        keyName = null;

        ISymbol? symbol = expr switch
        {
            MemberAccessExpressionSyntax memberAccess => model.GetSymbolInfo(memberAccess, token).Symbol,
            IdentifierNameSyntax identifier => model.GetSymbolInfo(identifier, token).Symbol,
            _ => null
        };

        if (symbol == null || (symbol is not IFieldSymbol && symbol is not IPropertySymbol))
            return false;

        if (symbol.ContainingType?.Name != "LocKey")
            return false;
        
        keyName = symbol.Name;
        return true;

    }

    private static string? TryGetStringArgument(SemanticModel model, ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal) return literal.Token.ValueText;
        Optional<object?> constant = model.GetConstantValue(expression);
        return constant.HasValue ? constant.Value?.ToString() : null;
    }

    private static string? TryGetStringFromExpression(ExpressionSyntax expr, SemanticModel model, CancellationToken token)
    {
        if (expr is LiteralExpressionSyntax literal) return literal.Token.ValueText;

        if (IsLocKeyMember(expr, model, token, out string? name)) return name;

        Optional<object?> constant = model.GetConstantValue(expr, token);
        return constant.HasValue ? constant.Value?.ToString() : null;
    }

    private static TextSpan? FindKeySpan(SourceText sourceText, string keyName, int approximateLine)
    {
        int startLine = Math.Max(0, approximateLine - 3);
        int endLine = Math.Min(sourceText.Lines.Count - 1, approximateLine + 3);
        string searchPattern = $"name=\"{keyName}\"";

        for (int i = startLine; i <= endLine; i++)
        {
            TextLine line = sourceText.Lines[i];
            string lineText = line.ToString();
            int index = lineText.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return new TextSpan(line.Start + index + "name=\"".Length, keyName.Length);
        }

        return null;
    }

    private static string NormalizeKey(string? key) =>
        string.IsNullOrEmpty(key) ? string.Empty : key?.Replace("__", "/").Trim() ?? string.Empty;

    private static bool IsSubclassOf(INamedTypeSymbol type, INamedTypeSymbol baseType) => type.BaseType != null &&
        (SymbolEqualityComparer.Default.Equals(type.BaseType, baseType) || IsSubclassOf(type.BaseType, baseType));

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns) =>
        ns.GetTypeMembers().Concat(ns.GetNamespaceMembers().SelectMany(GetAllTypes));

    private static bool IsLocalizationCultureFile(string filePath) =>
        CultureSuffixRegex.IsMatch(Path.GetFileNameWithoutExtension(filePath));

    private static bool ShouldIgnoreKey(XElement dataElement) => dataElement.Element("comment")?.Value.Trim()
        .Contains("PLUI-Unused", StringComparison.OrdinalIgnoreCase) == true;

    private static ConcurrentHashSet<string> LoadEmbeddedExclusions()
    {
        ConcurrentHashSet<string> exclusions = new();
        try
        {
            Assembly assembly = typeof(ResxUsageAnalyzer).Assembly;
            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("LocExclusions.txt", StringComparison.OrdinalIgnoreCase));
            
            if (string.IsNullOrEmpty(resourceName))
                return exclusions;
            
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null)
                return exclusions;
            
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } line)
                if (!string.IsNullOrWhiteSpace(line)) exclusions.Add(line.Trim());
        }
        catch
        {
            // ignored
        }

        return exclusions;
    }

    private static void AnalyzeAxamlFile(AdditionalText axamlFile, ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys, CancellationToken cancellationToken)
    {
        SourceText? sourceText = axamlFile.GetText(cancellationToken);
        if (sourceText == null) return;

        try
        {
            XDocument doc = XDocument.Parse(sourceText.ToString());

            if (doc.Root == null)
                return;

            foreach (XElement element in doc.DescendantNodes().OfType<XElement>())
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                foreach (XAttribute attribute in element.Attributes())
                {
                    string value = attribute.Value.Trim();

                    if (value.StartsWith("{", StringComparison.Ordinal) && value.Contains("Localize "))
                        ParseXamlMarkupExtension(value, normalizedTrackedKeys, usedNormalizedKeys);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void ParseXamlMarkupExtension(
        string extensionValue,
        ConcurrentHashSet<string> normalizedTrackedKeys,
        ConcurrentHashSet<string> usedNormalizedKeys)
    {
        string content = extensionValue.Trim('{', '}').Trim();

        int firstSpace = content.IndexOf(' ');
        if (firstSpace == -1) return;

        string argumentsPart = content.Substring(firstSpace + 1).Trim();

        List<string> arguments = SplitXamlArguments(argumentsPart);
        if (arguments.Count == 0) return;

        string? rawKey = null;
        string? context = null;

        string firstArg = arguments[0].Trim();

        if (firstArg.StartsWith("{", StringComparison.Ordinal) && firstArg.Contains("LocKey."))
        {
            int lastDot = firstArg.LastIndexOf('.');
            if (lastDot != -1)
                rawKey = firstArg.Substring(lastDot + 1).Trim('}', ' ', '\t', '\r', '\n');
        }
        else
            rawKey = firstArg.Trim('\'', '"');

        for (int i = 1; i < arguments.Count; i++)
        {
            string arg = arguments[i].Trim();
            
            if (!arg.StartsWith("Context", StringComparison.OrdinalIgnoreCase))
                continue;
            
            int eqIndex = arg.IndexOf('=');
            if (eqIndex != -1)
                context = arg.Substring(eqIndex + 1).Trim('\'', '"', ' ');
        }

        if (string.IsNullOrWhiteSpace(rawKey)) return;

        string? combinedKey = !string.IsNullOrEmpty(context) ? $"{context}/{rawKey}" : rawKey;
        string normalized = NormalizeKey(combinedKey);

        if (normalizedTrackedKeys.Contains(normalized))
            usedNormalizedKeys.Add(normalized);
    }

    private static List<string> SplitXamlArguments(string argumentsPart)
    {
        List<string> results = [];
        int braceCount = 0;
        int startIdx = 0;

        for (int i = 0; i < argumentsPart.Length; i++)
        {
            char c = argumentsPart[i];
            if (c == '{') braceCount++;
            else if (c == '}') braceCount--;
            else if (c == ',' && braceCount == 0)
            {
                results.Add(argumentsPart.Substring(startIdx, i - startIdx));
                startIdx = i + 1;
            }
        }

        if (startIdx < argumentsPart.Length)
            results.Add(argumentsPart.Substring(startIdx));

        return results;
    }

    private class ConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new();
        public void Add(T item) => _dictionary.TryAdd(item, 0);
        public bool Contains(T item) => _dictionary.ContainsKey(item);
        public IEnumerable<T> ToEnumerable() => _dictionary.Keys;
    }
}