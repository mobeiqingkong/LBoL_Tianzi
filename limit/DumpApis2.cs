using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

class P {
  static string Dump(MethodBase m) {
    if (m == null) return "null";
    var body = m.GetMethodBody();
    if (body == null) return "no body";
    var ops = typeof(System.Reflection.Emit.OpCodes).GetFields().Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToArray();
    // use mono.cecil? skip - just list called methods via Resolve
    return "ok len="+body.GetILAsByteArray().Length;
  }
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
    var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));
    foreach (var t in core.GetTypes().Where(t => t.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase)>=0 || t.Name.IndexOf("Interaction", StringComparison.OrdinalIgnoreCase)>=0))
      if (t.FullName.Contains("Interaction") || t.Name.Contains("Exhibit"))
        Console.WriteLine(t.FullName);
    Console.WriteLine("--- GapOptionsPanel methods ---");
    var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
    foreach (var m in gap.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
      Console.WriteLine(m.ReturnType.Name+" "+m.Name+"("+string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name))+")");
    Console.WriteLine("--- BossExhibit Show ---");
    var boss = pres.GetType("LBoL.Presentation.UI.Panels.BossExhibitPanel");
    foreach (var m in boss.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Where(m=>m.Name.Contains("Show")||m.Name.Contains("Click")||m.Name.Contains("Gain")).OrderBy(m=>m.Name))
      Console.WriteLine(m.ReturnType.Name+" "+m.Name+"("+string.Join(",", m.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name))+")");
  }
}
