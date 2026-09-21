using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var dll = args[0];
var mode = args[1];
var needle = args.Length > 2 ? args[2] : "";
var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
using var pe = new PEFile(dll);
var resolver = new UniversalAssemblyResolver(dll, false, pe.DetectTargetFrameworkId());
resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
resolver.AddSearchDirectory(@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed");
if (mode == "list")
{
    foreach (var t in pe.Metadata.TypeDefinitions)
    {
        var td = pe.Metadata.GetTypeDefinition(t);
        var ns = pe.Metadata.GetString(td.Namespace);
        var name = pe.Metadata.GetString(td.Name);
        var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        if (full.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
            Console.WriteLine(full);
    }
    return;
}
var dec = new CSharpDecompiler(pe, resolver, settings);
Console.WriteLine(dec.DecompileTypeAsString(new FullTypeName(needle)));
