using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Dump {
  static void Main() {
    var dll = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll";
    var asm = Assembly.LoadFrom(dll);
    var t = asm.GetType("LBoL.Core.GameRunController");
    foreach (var n in new[]{"get_FinalShopPriceMultiplier","get_ShopPriceMultiplier","get_UpgradeDeckCardPrice","get_RemoveDeckCardPrice"}) {
      var m = t.GetMethod(n, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
      Console.WriteLine("=== " + n + " ===");
      if (m==null) { Console.WriteLine("null"); continue; }
      Console.WriteLine(m);
      // dump IL briefly
      var body = m.GetMethodBody();
      if (body!=null) Console.WriteLine("IL len=" + body.GetILAsByteArray().Length + " locals=" + string.Join(",", body.LocalVariables.Select(v=>v.LocalType.Name)));
    }
    var st = asm.GetType("LBoL.Core.Stations.ShopStation");
    foreach (var n in new[]{"GetPrice","RefreshAfterBought","get_UpgradeDeckCardPrice","get_RemoveDeckCardPrice"}) {
      foreach (var m in st.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(x=>x.Name==n)) {
        Console.WriteLine("=== ShopStation." + m + " ===");
        var body = m.GetMethodBody();
        if (body!=null) Console.WriteLine("IL len=" + body.GetILAsByteArray().Length);
      }
    }
  }
}
