The question addresses how to dynamically compile C# code in .NET 9.0. A small demo program is created that compiles an arbitrary C# code file and creates a DLL.
The basic framework is relatively simple. To integrate a C# compiler into an application, we only need to:

Install the Microsoft.CodeAnalysis package
Parse the source code into a syntax tree
Create a compilation with proper options
Emit the assembly

However, the basic implementation fails because important references are missing. These references need to be added to CSharpCompilation through the references property. The code demonstrates adding essential assemblies including:

Core .NET assemblies (mscorlib.dll, System.Runtime.dll)
Basic functionality assemblies (System.Collections.dll, System.Linq.dll)
Composition-related assemblies
System.Private.CoreLib.dll

Even with these references, there's still one more requirement: a *.runtimeconfig.json file needs to be generated. This file is necessary for the runtime configuration, and once it's created, the compilation process works successfully.
The code shows how to:

Parse source code into a syntax tree
Set up compilation options
Add necessary references
Emit the compiled assembly
Either start the process if successful or display compilation errors

This approach allows for dynamic compilation of C# code within a .NET 9.0 application, which can be useful for scenarios requiring runtime code generation and execution.

Link: https://www.piwonka.cc/code-compilation-wie-c-code-magisch-zur-exe-wird/
