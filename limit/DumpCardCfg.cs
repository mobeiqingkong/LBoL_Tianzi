using System;
using System.IO;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    string managed=@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach(var d in Directory.GetFiles(managed,"*.dll")) try{Assembly.LoadFrom(d);}catch{}
    var cfg=Assembly.LoadFrom(Path.Combine(managed,"LBoL.ConfigData.dll"));
    var cardCfg=cfg.GetType("LBoL.ConfigData.CardConfig");
    // find FromId or All
    foreach(var m in cardCfg.GetMethods(BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).Where(m=>m.Name.Contains("Id")||m.Name.Contains("All")||m.Name.Contains("Get")).Take(20))
      Console.WriteLine(m.ReturnType.Name+" "+m.Name+"("+string.Join(",",m.GetParameters().Select(p=>p.ParameterType.Name))+")");
  }
}
