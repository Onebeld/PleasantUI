using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace PleasantUI.Localization.Analyzer.Tests;

public static class LocalizationAnalyzerVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    // Имитация инфраструктуры локализации внутри тестовой компиляции
    private const string LocalizerInfrastructure = """
                                                   namespace PleasantUI.Core.Localization
                                                   {
                                                       public enum LocKey
                                                       {
                                                           CardTitle__PinCode,
                                                           CardTitle__Button,
                                                           CardTitle__PleasantSnackbar,
                                                           Card__PleasantSnackbar
                                                       }

                                                       public class Localizer
                                                       {
                                                           public static Localizer Instance { get; } = new Localizer();
                                                           public string this[string key] => key;
                                                           public bool TryGetString(string key, out string value)
                                                           {
                                                               value = key;
                                                               return true;
                                                           }
                                                           public static string Tr(object? key, string? context = null, params object[] args) => key.ToString();
                                                           public static string TrDefault(object? key, string? defaultString = null, string? context = null, params object[] args) => key.ToString();
                                                           public static string Tr(LocKey key) => key.ToString();
                                                       }
                                                   }
                                                   """;

    public static async Task VerifyAnalyzerAsync(string source, string resxContent, List<MetadataReference> collection, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source + "\n" + LocalizerInfrastructure,
        };

        // Добавляем тестовый .resx файл как AdditionalText
        test.TestState.AdditionalFiles.Add(("Resources.resx", resxContent));
        test.TestState.AdditionalReferences.AddRange(collection);
        test.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}