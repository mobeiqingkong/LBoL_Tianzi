using System;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
var dll = args[0];
var typeName = args[1];
var methodName = args[2];
var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
using var pe = new PEFile(dll);
var resolver = new UniversalAssemblyResolver(dll, false, pe.DetectTargetFrameworkId());
resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
resolver.AddSearchDirectory(@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed");
var dec = new CSharpDecompiler(pe, resolver, settings);
var type = dec.TypeSystem.MainModule.TypeDefinitions.First(t => t.FullName == typeName);
foreach (var m in type.Methods.Where(m => m.Name == methodName))
{
    Console.WriteLine("===== " + m + " =====");
    Console.WriteLine(dec.DecompileAsString(m.MetadataToken));
}
