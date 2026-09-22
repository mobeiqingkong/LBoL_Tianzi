using System;
using System.Linq;
using System.Reflection;
using System.IO;

var game = @""D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed"";
var asm = Assembly.LoadFrom(Path.Combine(game, ""LBoL.EntityLib.dll""));
foreach (var t in asm.GetTypes().Where(t => t.Name.Contains("Tortoise") || t.Name.Contains("Shell") || t.Name.Contains("Reflect") || t.Name.Contains("Baota") || t.Name.Contains("Turtle") || (t.FullName!=null && t.FullName.Contains("StatusEffect") && t.Name.Contains("Thorn"))))
  Console.WriteLine(t.FullName);
Console.WriteLine("---search names---");
string[] keys = {""龟"", ""Tortoise"", ""Shell"", ""Reflect"", ""Retaliate"", ""Thorns"", ""Counter"", ""Mirror"", ""Query"", ""Aegis""};
foreach (var t in asm.GetTypes()) {
  var n = t.Name;
  if (keys.Any(k => n.IndexOf(k, StringComparison.OrdinalIgnoreCase)>=0))
    Console.WriteLine(t.FullName);
}
