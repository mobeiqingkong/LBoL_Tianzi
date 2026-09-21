using System.IO;
using System.Linq;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
var dll = args[0];
using var pe = new PEFile(dll);
var resolver = new UniversalAssemblyResolver(dll, false, pe.DetectTargetFrameworkId());
resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
resolver.AddSearchDirectory(@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed");
var ts = new DecompilerTypeSystem(pe, resolver);
if (args.Length >= 2 && args[1] == "methods")
{
  var type = ts.MainModule.TypeDefinitions.First(t => t.FullName == args[2]);
  foreach (var m in type.Methods.OrderBy(m => m.Name))
    Console.WriteLine(m.ToString());
}
else
{
  foreach (var t in ts.MainModule.TypeDefinitions
      .Where(t => t.Namespace == "LBoL.Core.Battle.BattleActions")
      .OrderBy(t => t.Name))
    Console.WriteLine(t.Name);
}
