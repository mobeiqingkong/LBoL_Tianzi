using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var game = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
var dll = Path.Combine(game, "LBoL.Presentation.dll");
var resolver = new UniversalAssemblyResolver(dll, false, null);
resolver.AddSearchDirectory(game);
var d = new CSharpDecompiler(dll, resolver, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var code = d.DecompileTypeAsString(new FullTypeName("LBoL.Presentation.ResourcesHelper"));
File.WriteAllText(@"D:\riderProject\Hinanawi-Tenshi\limit\_de_resourceshelper.txt", code);
foreach (var line in code.Split('\n'))
  if (line.Contains("Boss") || line.Contains("TryGet") || line.Contains("public static"))
    Console.WriteLine(line.TrimEnd());
