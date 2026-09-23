using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var game = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
var path = Path.Combine(game, "LBoL.Presentation.dll");
var resolver = new UniversalAssemblyResolver(path, false, null);
resolver.AddSearchDirectory(game);
var decompiler = new CSharpDecompiler(path, resolver, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
File.WriteAllText(@"D:\riderProject\Hinanawi-Tenshi\limit\_de_healthbar.txt",
  decompiler.DecompileTypeAsString(new FullTypeName("LBoL.Presentation.UI.Widgets.HealthBar")));
Console.WriteLine("ok");
