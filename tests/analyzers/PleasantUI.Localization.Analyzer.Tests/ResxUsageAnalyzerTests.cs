using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;
using Xunit;

namespace PleasantUI.Localization.Analyzer.Tests;

public class ResxUsageAnalyzerTests
{
    private const string DiagnosticId = "PUILOC101";
    
    private const string TestResxContent = """
                                           <?xml version='1.0' encoding='utf-8'?>
                                           <root>
                                             <data name='App/CardTitle/PinCode'><value>Pin Code</value></data>
                                           </root>
                                           """;

    private static readonly ReferenceAssemblies NetVersion = ReferenceAssemblies.Net.Net100;
    
    private static DiagnosticResult GetExpectedDiagnostic(string key, string fileName, int line, int column = 1)
    {
        return new DiagnosticResult(DiagnosticId, DiagnosticSeverity.Warning)
            .WithMessage($"Localization key '{key}' from file '{fileName}' is not used in the project")
            .WithLocation(fileName, line, column);
    }
    
    [Fact]
    public async Task UnusedKeyInResx_ReportsDiagnostic()
    {
        var resxContent = """
                          <root>
                            <data name="UnusedKey" xml:space="preserve">
                              <value>Unused text</value>
                            </data>
                            <data name="UsedKey" xml:space="preserve">
                              <value>Used text</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var x = Localizer.Tr("UsedKey");
                             }
                         }
                         """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = sourceCode,
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent)
                },
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(Localizer).Assembly.Location)
                },
                ReferenceAssemblies = NetVersion,
            }
        };

        test.ExpectedDiagnostics.Add(GetExpectedDiagnostic("UnusedKey", "Resources.resx", 2, 15));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr("Save", "ButtonPage");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_Multiline_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr(
                                      "Save", 
                                      "ButtonPage");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithoutContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr("ButtonPage/Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithMultiContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="App/ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr("App/ButtonPage/Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithContext_WithMultiContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="App/ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr("ButtonPage/Save", "App");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithContext_WithMultiContext_ConstantContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="App/ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             const string MyAnother = "App";

                             public void Method() 
                             {
                                 var text = Localizer.Tr("ButtonPage/Save", MyAnother);
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithUnusedKeyInComment_WithoutContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Cancel" xml:space="preserve">
                                <value>Save</value>
                                <comment>PLUI-Unused</comment>
                            </data>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                              <comment>Save</comment>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr("ButtonPage/Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_WithLocKey_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         // autogenerated code
                         public enum LocKey {
                             ButtonPage__Save
                         }

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 var text = Localizer.Tr(LocKey.ButtonPage__Save);
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_AnotherMethod_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             private static string T(string key, string fallback) =>
                                  Localizer.TrDefault(key, fallback);

                             public void Method() 
                             {
                                 var text = T("ButtonPage/Save", "Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_AnotherMethod_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             private static string T(string key, string fallback) =>
                                  Localizer.TrDefault(key, fallback, "ButtonPage");

                             public void Method() 
                             {
                                 var text = T("Save", "Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_AnotherMethod_WithContext_ObjectKey_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         public class TestClass 
                         {
                             private static string T(object key, string fallback) =>
                                  Localizer.TrDefault(key, fallback, "ButtonPage");

                             public void Method() 
                             {
                                 var text = T("Save", "Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInTr_AnotherMethod_WithLocKey_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;

                         // autogenerated code
                         public enum LocKey {
                             ButtonPage__Save
                         }

                         public class TestClass 
                         {
                             private static string T(LocKey key, string fallback) =>
                                  Localizer.TrDefault(key, fallback);

                             public void Method() 
                             {
                                 var text = T(LocKey.ButtonPage__Save, "Save");
                             }
                         }
                         """;
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, []);
    }
    
    [Fact]
    public async Task Scenario1_InheritedProperties_ShouldMarkKeyAsUsed()
    {
        var sourceCode = """
                         using System.ComponentModel;
                         using PleasantUI.Core.Localization;

                         public interface IPage {}
                         public class Control {}

                         public abstract class LocalizedPage : IPage
                         {
                             public abstract string TitleKey { get; }
                             public string Title => Localizer.Instance.TryGetString(TitleKey, out string value) ? value : TitleKey;
                         }

                         public class PinCodePage : LocalizedPage
                         {
                             public override string TitleKey { get; } = "App/CardTitle/PinCode";
                         }
                         """;

        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, TestResxContent, []);
    }
    
    [Fact]
    public async Task Scenario2_ConstructorParametersAndMethodWrappers_ShouldMarkKeyAsUsed()
    {
        var sourceCode = """
                         using System;
                         using PleasantUI.Core.Localization;

                         public interface IPage {}

                         public class ControlPageCard
                         {
                             public string TitleKey { get; }
                             
                             public ControlPageCard(string titleKey)
                             {
                                 TitleKey = titleKey;
                                 var title = Resolve(titleKey);
                             }

                             private static string Resolve(string key) =>
                                 Localizer.Instance.TryGetString(key, out string value) ? value : key;
                         }

                         public class TestClass
                         {
                             public void CreateCard()
                             {
                                 // Прямой вызов конструктора с литералом
                                 var card = new ControlPageCard("App/CardTitle/PinCode");
                             }
                         }
                         """;

        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, TestResxContent, []);
    }
    
    [Fact]
    public async Task Scenario3_ImplicitObjectCreationInCollections_ShouldMarkKeysAsUsed()
    {
        var sourceCode = """
                         using System;
                         using System.Collections.Generic;
                         using PleasantUI.Core.Localization;

                         public interface IPage {}
                         public class PleasantSnackbarPage : IPage {}

                         public class ControlPageCard
                         {
                             public ControlPageCard(string titleKey, string descriptionKey, Func<IPage> page)
                             {
                                 var t = Resolve(titleKey);
                                 var d = Resolve(descriptionKey);
                             }

                             private static string Resolve(string key) =>
                                 Localizer.Instance.TryGetString(key, out string value) ? value : key;
                         }

                         public class CardContainer
                         {
                             public List<ControlPageCard> CreateCards()
                             {
                                 // Неявное создание объектов через new(...) внутри коллекции (ImplicitObjectCreationExpression)
                                 return
                                 [
                                     new("App/CardTitle/PinCode", "App/CardTitle/PinCode", () => new PleasantSnackbarPage())
                                 ];
                             }
                         }
                         """;

        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, TestResxContent, []);
    }
    
    [Fact]
    public async Task KeyUsedInLocalizeBinding_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;
                         using PleasantUI.Controls;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 PleasantMenuItem item = new();

                                 item.Bind(PleasantMenuItem.LabelProperty, LocalizeBinding.Create("ButtonPage/Save"));
                             }
                         }
                         """;

        List<MetadataReference> additionalReferences =
        [
            MetadataReference.CreateFromFile(typeof(LocalizeBinding).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(PleasantMenuItem).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AvaloniaObject).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(StyledProperty<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(BindingBase).Assembly.Location),
        ];
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, additionalReferences);
    }
    
    [Fact]
    public async Task KeyUsedInLocalizeBinding_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="App/ButtonPage/Save" xml:space="preserve">
                              <value>Save</value>
                            </data>
                          </root>
                          """;
        
        var sourceCode = """
                         using PleasantUI.Core.Localization;
                         using PleasantUI.Controls;

                         public class TestClass 
                         {
                             public void Method() 
                             {
                                 PleasantMenuItem item = new();

                                 item.Bind(PleasantMenuItem.LabelProperty, LocalizeBinding.Create("ButtonPage/Save", "App"));
                             }
                         }
                         """;
        
        List<MetadataReference> additionalReferences =
        [
            MetadataReference.CreateFromFile(typeof(LocalizeBinding).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(PleasantMenuItem).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AvaloniaObject).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(StyledProperty<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(BindingBase).Assembly.Location),
        ];
        
        await LocalizationAnalyzerVerifier<ResxUsageAnalyzer>.VerifyAnalyzerAsync(sourceCode, resxContent, additionalReferences);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                               <TextBlock Text="{Localize MiniWindow/PropEnableBlur}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithNamespace_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:loc="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{loc:Localize MiniWindow/PropEnableBlur}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                               <TextBlock Text="{Localize PropEnableBlur, Context='MiniWindow'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithNamespace_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:loc="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{loc:Localize PropEnableBlur, Context='MiniWindow'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithAnotherNamespace_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:new-core="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{new-core:Localize PropEnableBlur, Context='MiniWindow'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithMultiContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="App/MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:loc="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{Localize MiniWindow/PropEnableBlur, Context='App'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_InQuotationMarks_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:loc="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{Localize 'MiniWindow/PropEnableBlur'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_InQuotationMarks_WithContext_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                        xmlns:loc="clr-namespace:PleasantUI.Core.Localization">
                               <TextBlock Text="{Localize 'PropEnableBlur', Context='MiniWindow'}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task KeyUsedInAxaml_WithLocKey_ShouldNotReport()
    {
        var resxContent = """
                          <root>
                            <data name="MiniWindow/PropEnableBlur" xml:space="preserve">
                              <value>Blur</value>
                            </data>
                          </root>
                          """;

        var axamlContent = """
                           <UserControl xmlns="https://github.com/avaloniaui"
                                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                               <TextBlock Text="{Localize {x:Static LocKey.MiniWindow__PropEnableBlur}}" />
                           </UserControl>
                           """;

        var test = new CSharpAnalyzerTest<ResxUsageAnalyzer, DefaultVerifier>
        {
            TestCode = "// Empty C# code",
            TestState =
            {
                AdditionalFiles =
                {
                    ("Resources.resx", resxContent),
                    ("MainWindow.axaml", axamlContent)
                }
            }
        };

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}