using System;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
var core = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll";
var d = new CSharpDecompiler(core, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
int count = 0;
foreach (var t in d.TypeSystem.GetAllTypeDefinitions()) {
  string s;
  try { s = d.DecompileTypeAsString(new FullTypeName(t.FullName)); } catch { continue; }
  if (s.Contains("new UseCardAction") || s.Contains("new PlayCardAction")) {
    Console.WriteLine(t.FullName);
    if (++count > 20) break;
  }
}
