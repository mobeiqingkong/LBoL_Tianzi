using System;
using System.IO;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","LBoL.Base.dll","YamlDotNet.dll"})
      try { Assembly.LoadFrom(Path.Combine(managed,d)); } catch {}
    var cfg = Assembly.LoadFrom(Path.Combine(managed,"LBoL.ConfigData.dll"));
    var seCfg = cfg.GetType("LBoL.ConfigData.StatusEffectConfig");
    var fromId = seCfg.GetMethod("FromId", BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
    object c = fromId.Invoke(null, new object[]{"Servant"});
    Console.WriteLine("Servant config="+c);
    if (c != null) {
      foreach (var p in seCfg.GetProperties(BindingFlags.Instance|BindingFlags.Public)) {
        try { Console.WriteLine(p.Name+"="+p.GetValue(c)); } catch {}
      }
    }
    // Search string Servant in core methods via metadata is hard; check BattleController methods with Servant in name
    var core = Assembly.LoadFrom(Path.Combine(managed,"LBoL.Core.dll"));
    var bc = core.GetType("LBoL.Core.Battle.BattleController");
    foreach (var m in bc.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
      if (m.Name.IndexOf("Servant", StringComparison.OrdinalIgnoreCase)>=0 || m.Name.IndexOf("Die", StringComparison.OrdinalIgnoreCase)>=0 || m.Name.IndexOf("Alive", StringComparison.OrdinalIgnoreCase)>=0 || m.Name.IndexOf("Enemy", StringComparison.OrdinalIgnoreCase)>=0)
        Console.WriteLine("BC "+m.ReturnType.Name+" "+m.Name+"("+string.Join(",",m.GetParameters().Select(p=>p.ParameterType.Name))+")");
  }
}
