using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;

namespace CodeCompiler;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please enter the path to the source code file.");
            return;
        }

        string sourceFile = args[0];
        string outputFile = Path.ChangeExtension(sourceFile, ".dll");

        try
        {


            // Quellcode aus der Textdatei lesen
            string sourceCode = await File.ReadAllTextAsync(sourceFile);

            // Syntax Tree erstellen
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // Assembly Name aus dem Dateinamen erstellen
            string assemblyName = Path.GetFileNameWithoutExtension(sourceFile);
            string runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            // Referenzen hinzufügen
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Decimal).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Core.dll")),
                   // Explizit CoreLib referenzieren
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Private.CoreLib.dll")),
                // Weitere wichtige Basis-Assemblies
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Collections.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Threading.dll")),

                MetadataReference.CreateFromFile(typeof(System.Composition.Convention.AttributedModelProvider).Assembly.Location), //System.Composition.AttributeModel
                MetadataReference.CreateFromFile(typeof(System.Composition.Convention.ConventionBuilder).Assembly.Location),   //System.Composition.Convention
                MetadataReference.CreateFromFile(typeof(System.Composition.Hosting.CompositionHost).Assembly.Location),        //System.Composition.Hosting
                MetadataReference.CreateFromFile(typeof(System.Composition.CompositionContext).Assembly.Location),             //System.Composition.Runtime
                MetadataReference.CreateFromFile(typeof(System.Composition.CompositionContextExtensions).Assembly.Location),   //System.Composition.TypedParts

            };

            // Compilation Optionen erstellen
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,

                platform: Platform.AnyCpu
            );

            // Compilation erstellen
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,

                options: compilationOptions);

            // Assembly erstellen
            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(outputFile);

            // Ergebnis überprüfen
            if (!result.Success)
            {
                Console.WriteLine("Compilation error:");
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    var location = diagnostic.Location;
                    if (location != Location.None)
                    {
                        var lineSpan = location.GetLineSpan();
                        Console.WriteLine($"Line {lineSpan.StartLinePosition.Line + 1}: {lineSpan.StartLinePosition.Character + 1}");
                    }
                }
            }
            else
            {

                var runtimeConfig = new
                {
                    runtimeOptions = new
                    {
                        tfm = "net9.0",
                        framework = new
                        {
                            name = "Microsoft.NETCore.App",
                            version = "9.0.0"
                        },
                        configProperties = new
                        {
                            EnableUnsafeBinaryFormatterSerialization = false,
                        }
                    }
                };

                string jsonString = JsonSerializer.Serialize(runtimeConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(assemblyName + ".runtimeconfig.json", jsonString);
                Console.WriteLine($"Compilation successful! Output file: {outputFile}");
                Console.WriteLine($"\nDu kannst die Anwendung mit 'dotnet {outputFile}' ausführen.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}