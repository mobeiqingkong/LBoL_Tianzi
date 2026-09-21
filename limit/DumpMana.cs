using System;
using System.IO;
using System.Linq;
using System.Reflection;

class P {
  static void Main() {
    string managed=@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach(var d in new[]{"LBoL.Base.dll","LBoL.Core.dll"}) Assembly.LoadFrom(Path.Combine(managed,d));
    var baseAsm=Assembly.LoadFrom(Path.Combine(managed,"LBoL.Base.dll"));
    var mc=baseAsm.GetType("LBoL.Base.ManaColor");
    Console.WriteLine("ManaColor values:");
    foreach(var v in Enum.GetValues(mc)) Console.WriteLine("  "+(int)v+"="+v);
    var mg=baseAsm.GetType("LBoL.Base.ManaGroup");
    foreach(var m in mg.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
      if(m.Name.Contains("Color")||m.Name.Contains("White")||m.Name.Contains("Amount")||m.Name.Contains("Has")||m.Name.Contains("Get")||m.Name.Contains("Can"))
        Console.WriteLine("ManaGroup "+m.ReturnType.Name+" "+m.Name+"("+string.Join(",",m.GetParameters().Select(p=>p.ParameterType.Name))+")");
    foreach(var p in mg.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly))
      Console.WriteLine("PROP "+p.PropertyType.Name+" "+p.Name);
    // SakuyaShop colors
    var ent=Assembly.LoadFrom(Path.Combine(managed,"LBoL.EntityLib.dll"));
    Type shop=null;
    try{shop=ent.GetType("LBoL.EntityLib.Cards.Character.Sakuya.SakuyaShop");}catch{}
    if(shop==null){
      try{
        foreach(var t in ent.GetTypes()) if(t!=null && t.Name=="SakuyaShop"){shop=t;break;}
      }catch(ReflectionTypeLoadException e){ foreach(var t in e.Types) if(t!=null && t.Name=="SakuyaShop"){shop=t;break;} }
    }
    Console.WriteLine("SakuyaShop="+shop);
  }
}
