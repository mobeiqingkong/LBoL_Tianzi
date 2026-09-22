using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
var path = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.EntityLib.dll";
var d = new CSharpDecompiler(path, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
foreach (var name in new[] {
  "LBoL.EntityLib.StatusEffects.Basic.Amulet",
  "LBoL.EntityLib.StatusEffects.Basic.AmuletForCard",
}) {
  try { Console.WriteLine(d.DecompileTypeAsString(new FullTypeName(name))); Console.WriteLine("---"); }
  catch (Exception ex) { Console.WriteLine(name + ": " + ex.Message); }
}
