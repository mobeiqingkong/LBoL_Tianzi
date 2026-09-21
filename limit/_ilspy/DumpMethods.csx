using System;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var dll = args[0];
var typeName = args[1];
var methodNames = args.Skip(2).ToHashSet();
var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
using var pe = new PEFile(dll);
var resolver = new UniversalAssemblyResolver(dll, false, pe.DetectTargetFrameworkId());
resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
resolver.AddSearchDirectory(@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed");
var dec = new CSharpDecompiler(pe, resolver, settings);
var type = dec.TypeSystem.MainModule.TypeDefinitions.First(t => t.FullName == typeName);
foreach (var m in type.Methods.Where(m => methodNames.Count==0 || methodNames.Contains(m.Name)))
{
    if (methodNames.Count==0 && !(m.Name.Contains("Battle") || m.Name.Contains("Move") || m.Name.Contains("Intent") || m.Name.Contains("Start") || m.Name.Contains("Turn"))) continue;
    try {
      Console.WriteLine("===== " + m.Name + " =====");
      Console.WriteLine(dec.DecompileAsString(m.MetadataToken));
    } catch (Exception ex) { Console.WriteLine(ex.Message); }
}
