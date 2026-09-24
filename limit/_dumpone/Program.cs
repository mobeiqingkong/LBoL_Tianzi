using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var game = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
// Load configs via reflection from game assemblies if possible - or dump Gun class
var path = Path.Combine(game, "LBoL.Presentation.dll");
var resolver = new UniversalAssemblyResolver(path, false, null);
resolver.AddSearchDirectory(game);
var decompiler = new CSharpDecompiler(path, resolver, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
File.WriteAllText(@"D:\riderProject\Hinanawi-Tenshi\limit\_de_gun.txt",
  decompiler.DecompileTypeAsString(new FullTypeName("LBoL.Presentation.Bullet.Gun")));

var path2 = Path.Combine(game, "LBoL.ConfigData.dll");
var resolver2 = new UniversalAssemblyResolver(path2, false, null);
resolver2.AddSearchDirectory(game);
var decompiler2 = new CSharpDecompiler(path2, resolver2, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
File.WriteAllText(@"D:\riderProject\Hinanawi-Tenshi\limit\_de_gunconfig.txt",
  decompiler2.DecompileTypeAsString(new FullTypeName("LBoL.ConfigData.GunConfig")));
Console.WriteLine("ok");
