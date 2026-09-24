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
foreach (var t in ts.GetAllTypeDefinitions().Where(x => x.Name.Contains("Hybrid") || x.Name == "ManaGroup"))
  Console.WriteLine(t.FullName);
File.WriteAllText(@"D:\riderProject\Hinanawi-Tenshi\limit\_de_managroup.txt",
  decompiler.DecompileTypeAsString(new FullTypeName("LBoL.Base.ManaGroup")));
