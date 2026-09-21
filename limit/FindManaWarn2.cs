using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

class P {
  static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
  static P() {
    foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static))
      map[((OpCode)f.GetValue(null)).Value] = (OpCode)f.GetValue(null);
  }
  static bool Mentions(MethodInfo m, string s) {
    try {
      var body = m.GetMethodBody();
      if (body == null) return false;
      var il = body.GetILAsByteArray();
      var mod = m.Module;
      int pos = 0;
      while (pos < il.Length) {
        OpCode op;
        if (il[pos] == 0xFE) { pos++; op = map[(short)(0xFE00 | il[pos++])]; }
        else op = map[il[pos++]];
        if (op.OperandType == OperandType.InlineString) {
          int token = BitConverter.ToInt32(il, pos); pos += 4;
          try {
            var str = mod.ResolveString(token);
            if (str.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) return true;
          } catch {}
        }
        else if (op.OperandType == OperandType.ShortInlineBrTarget || op.OperandType == OperandType.ShortInlineI || op.OperandType == OperandType.ShortInlineVar) pos++;
        else if (op.OperandType == OperandType.InlineBrTarget || op.OperandType == OperandType.InlineI || op.OperandType == OperandType.ShortInlineR || op.OperandType == OperandType.InlineMethod || op.OperandType == OperandType.InlineField || op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok || op.OperandType == OperandType.InlineSig) pos += 4;
        else if (op.OperandType == OperandType.InlineI8 || op.OperandType == OperandType.InlineR) pos += 8;
        else if (op.OperandType == OperandType.InlineVar) pos += 2;
        else if (op.OperandType == OperandType.InlineSwitch) { int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n; }
      }
    } catch {}
    return false;
  }
  static void Scan(Assembly a, string tag) {
    Type[] types;
    try { types = a.GetTypes(); }
    catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
    foreach (var t in types) {
      if (t == null) continue;
      MethodInfo[] ms;
      try { ms = t.GetMethods(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly); }
      catch { continue; }
      foreach (var m in ms) {
        if (Mentions(m, "widget count") || Mentions(m, "BattleManaPanel") || Mentions(m, "not equal to Battle"))
          Console.WriteLine(tag + " " + t.FullName + "." + m.Name);
      }
      foreach (var nt in t.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public)) {
        var mn = nt.GetMethod("MoveNext", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if (mn != null && (Mentions(mn, "widget count") || Mentions(mn, "BattleManaPanel")))
          Console.WriteLine(tag + " nested " + nt.FullName + ".MoveNext");
      }
    }
  }
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in Directory.GetFiles(managed, "*.dll")) {
      try {
        var name = Path.GetFileName(d);
        if (name.StartsWith("Unity") || name.StartsWith("System") || name.StartsWith("Mono") || name.StartsWith("mscorlib") || name.StartsWith("netstandard"))
          continue;
        var a = Assembly.LoadFrom(d);
        Scan(a, name);
      } catch {}
    }
    Console.WriteLine("done");
  }
}
