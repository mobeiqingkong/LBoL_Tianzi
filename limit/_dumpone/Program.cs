using System;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var game = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
var path = Path.Combine(game, "LBoL.Base.dll");
var resolver = new UniversalAssemblyResolver(path, false, null);
resolver.AddSearchDirectory(game);
var decompiler = new CSharpDecompiler(path, resolver, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var pe = new PEFile(path);
var ts = new DecompilerTypeSystem(pe, resolver);
foreach (var t in ts.GetAllTypeDefinitions().Where(x => x.Name == "ManaGroup"))
{
  Console.WriteLine(t.FullName);
  foreach (var m in t.Members.Where(mm => mm.Name.Contains("Amount") || mm.Name == "Total" || mm.Name == "IsEmpty" || mm.Name == "Empty"))
    Console.WriteLine("  "+m.Name);
}
