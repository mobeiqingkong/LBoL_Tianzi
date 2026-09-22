using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var lib = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.EntityLib.dll";
var game = Path.GetDirectoryName(lib)!;
var resolver = new UniversalAssemblyResolver(lib, false, null);
resolver.AddSearchDirectory(game);
var d = new CSharpDecompiler(lib, resolver, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
var outDir = @"D:\riderProject\Hinanawi-Tenshi\limit";
foreach (var name in new[] {
    "LBoL.EntityLib.StatusEffects.Basic.Reflect",
    "LBoL.EntityLib.StatusEffects.Neutral.TwoColor.YachieDefendSe",
    "LBoL.EntityLib.Cards.Neutral.TwoColor.YachieDefend",
    "LBoL.EntityLib.StatusEffects.Basic.TempElectric",
    "LBoL.EntityLib.StatusEffects.Neutral.TwoColor.ReflectDamage",
})
{
    Console.WriteLine("======== " + name + " ========");
    try
    {
        var code = d.DecompileTypeAsString(new FullTypeName(name));
        Console.WriteLine(code);
        File.WriteAllText(Path.Combine(outDir, "_de_" + name.Split('.').Last() + ".txt"), code);
    }
    catch (Exception ex) { Console.WriteLine(ex); }
}
