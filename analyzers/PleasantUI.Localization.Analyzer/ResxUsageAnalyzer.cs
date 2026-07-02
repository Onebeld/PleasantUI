using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

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
        true
    );
    
    private static readonly Regex CultureSuffixRegex = new(@"\.[a-z]{2}(-[A-Z]{2,4})?(-[A-Z][a-z]{3})?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly ConcurrentHashSet<string> LibraryExclusions = LoadEmbeddedExclusions();
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            List<AdditionalText> resxFiles = compilationContext.Options.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".resx", StringComparison.OrdinalIgnoreCase))
                .Where(f => !IsLocalizationCultureFile(f.Path))
                .ToList();
            
            if (!resxFiles.Any()) return;
            
            ConcurrentDictionary<string, (string FileName, AdditionalText FileText, int LineNumber)> trackedKeys = new();
            ConcurrentHashSet<string> normalizedTrackedKeys = new();
            
            foreach (var file in resxFiles)
            {
                SourceText? sourceText = file.GetText(compilationContext.CancellationToken);
                if (sourceText == null) continue;

                try
                {
                    XDocument doc = XDocument.Parse(sourceText.ToString(), LoadOptions.SetLineInfo);
                    IEnumerable<XElement>? dataElements = doc.Root?.Elements("data");
                    if (dataElements == null) continue;

                    foreach (XElement? element in dataElements)
                    {
                        string? keyName = element.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(keyName)) continue;

                        IXmlLineInfo lineInfo = element;
                        int line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0;

                        if (keyName != null && trackedKeys.TryAdd(keyName, (Path.GetFileName(file.Path), file, line)))
                            normalizedTrackedKeys.Add(NormalizeKey(keyName));
                    }
                }
                catch { /* Игнорируем некорректный XML */ }
            }
            
            if (!trackedKeys.Any()) return;
            
            ConcurrentHashSet<string> usedNormalizedKeys = new();
            
            foreach (string? exclusion in LibraryExclusions.ToEnumerable())
            {
                if (normalizedTrackedKeys.Contains(exclusion)) 
                    usedNormalizedKeys.Add(exclusion);
            }
            
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)nodeContext.Node;
                string memberName = memberAccess.Name.Identifier.ValueText;

                string normalized = NormalizeKey(memberName);
                if (normalizedTrackedKeys.Contains(normalized)) 
                    usedNormalizedKeys.Add(normalized);
            }, SyntaxKind.SimpleMemberAccessExpression);
            
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                LiteralExpressionSyntax stringLiteral = (LiteralExpressionSyntax)nodeContext.Node;
                string stringValue = stringLiteral.Token.ValueText;

                string normalized = NormalizeKey(stringValue);
                if (normalizedTrackedKeys.Contains(normalized)) 
                    usedNormalizedKeys.Add(normalized);
            }, SyntaxKind.StringLiteralExpression);
            
            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var keyPair in trackedKeys)
                {
                    string? originalKey = keyPair.Key;
                    string normalizedKey = NormalizeKey(originalKey);

                    if (!usedNormalizedKeys.Contains(normalizedKey))
                    {
                        var (fileName, additionalText, lineNumber) = keyPair.Value;

                        // Получаем SourceText файла
                        SourceText? sourceText = additionalText.GetText(endContext.CancellationToken);
                        if (sourceText == null) continue;

                        // Ищем точную позицию ключа в тексте (лучше всего искать строку с name="Ключ")
                        TextSpan? textSpan = FindKeySpan(sourceText, originalKey, lineNumber);

                        Location location = textSpan.HasValue 
                            ? Location.Create(additionalText.Path, textSpan.Value, sourceText.Lines.GetLinePositionSpan(textSpan.Value))
                            : Location.Create(additionalText.Path, new TextSpan(0, 0), new LinePositionSpan(new LinePosition(Math.Max(0, lineNumber - 1), 0), new LinePosition(Math.Max(0, lineNumber - 1), 0)));

                        Diagnostic diagnostic = Diagnostic.Create(Rule, location, originalKey, fileName);
                        endContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        });
    }

    private static ConcurrentHashSet<string> LoadEmbeddedExclusions()
    {
        ConcurrentHashSet<string> exclusions = new();
        Assembly assembly = typeof(ResxUsageAnalyzer).Assembly;

        string? resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("LocExclusions.txt", StringComparison.OrdinalIgnoreCase));
        
        if (string.IsNullOrEmpty(resourceName)) return exclusions;

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return exclusions;

        using StreamReader reader = new(stream);

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            exclusions.Add(line);
        }
        
        return exclusions;
    }
    
    private static TextSpan? FindKeySpan(SourceText sourceText, string keyName, int approximateLine)
    {
        // Ищем в районе ожидаемой строки + несколько строк вокруг
        int startLine = Math.Max(0, approximateLine - 5);
        int endLine = Math.Min(sourceText.Lines.Count - 1, approximateLine + 5);

        string searchPattern = $"name=\"{keyName}\""; // или name='{keyName}'

        for (int i = startLine; i <= endLine; i++)
        {
            var line = sourceText.Lines[i];
            string lineText = line.ToString();

            int index = lineText.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int absolutePosition = line.Start + index;
                
                int keyStart = absolutePosition + "name=\"".Length;
                return new TextSpan(keyStart, keyName.Length);
            }
        }

        // Fallback — вся строка data
        return null;
    }

    private static bool IsLocalizationCultureFile(string filePath)
    {
        string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        return CultureSuffixRegex.IsMatch(fileNameWithoutExtension);
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;
        
        return key.Replace("__", "/").Trim();
    }
    
    private class ConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new();
        
        public void Add(T item) => _dictionary.TryAdd(item, 0);
        
        public bool Contains(T item) => _dictionary.ContainsKey(item);
        
        public IEnumerable<T> ToEnumerable() => _dictionary.Keys;
    }
}

