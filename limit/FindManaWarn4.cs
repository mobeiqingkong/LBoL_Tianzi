using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Text;

class P {
  static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
  static P() {
    foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static))
      map[((OpCode)f.GetValue(null)).Value] = (OpCode)f.GetValue(null);
  }
  static void ScanType(Type t) {
    MethodInfo[] ms;
    try { ms = t.GetMethods(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly); }
    catch { return; }
    foreach (var m in ms) {
      try {
        var body = m.GetMethodBody();
        if (body == null) continue;
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
              var s = mod.ResolveString(token);
              if (s.IndexOf("BattleMana", StringComparison.OrdinalIgnoreCase) >= 0
                  || s.IndexOf("widget", StringComparison.OrdinalIgnoreCase) >= 0
                  || s.IndexOf("not equal", StringComparison.OrdinalIgnoreCase) >= 0
                  || s.IndexOf("Base widget", StringComparison.OrdinalIgnoreCase) >= 0)
                Console.WriteLine(t.Name + "." + m.Name + " => " + s);
            } catch {}
          }
          else if (op.OperandType == OperandType.ShortInlineBrTarget || op.OperandType == OperandType.ShortInlineI || op.OperandType == OperandType.ShortInlineVar) pos++;
          else if (op.OperandType == OperandType.InlineBrTarget || op.OperandType == OperandType.InlineI || op.OperandType == OperandType.ShortInlineR || op.OperandType == OperandType.InlineMethod || op.OperandType == OperandType.InlineField || op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok || op.OperandType == OperandType.InlineSig) pos += 4;
          else if (op.OperandType == OperandType.InlineI8 || op.OperandType == OperandType.InlineR) pos += 8;
          else if (op.OperandType == OperandType.InlineVar) pos += 2;
          else if (op.OperandType == OperandType.InlineSwitch) { int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n; }
        }
      } catch {}
    }
    foreach (var nt in t.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public))
      ScanType(nt);
  }
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","UnityEngine.dll","UnityEngine.UI.dll","LBoL.Base.dll","YamlDotNet.dll","DOTween.dll","Unity.TextMeshPro.dll"})
      try { Assembly.LoadFrom(Path.Combine(managed,d)); } catch {}
    var pres = Assembly.LoadFrom(Path.Combine(managed,"LBoL.Presentation.dll"));
    Type[] types;
    try { types = pres.GetTypes(); }
    catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
    foreach (var t in types) {
      if (t == null) continue;
      if (t.FullName != null && (t.FullName.Contains("Mana") || t.FullName.Contains("BattleMana")))
        ScanType(t);
    }
    // also all LogWarning strings with BattleManaPanel prefix via full assembly scan of strings containing "color"
    foreach (var t in types) {
      if (t == null) continue;
      ScanTypeFor(t, "color ");
    }
    Console.WriteLine("done");
  }
  static void ScanTypeFor(Type t, string needle) {
    MethodInfo[] ms;
    try { ms = t.GetMethods(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly); }
    catch { return; }
    foreach (var m in ms) {
      try {
        var body = m.GetMethodBody();
        if (body == null) continue;
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
              var s = mod.ResolveString(token);
              if (s.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 && s.IndexOf("Mana", StringComparison.OrdinalIgnoreCase) >= 0)
                Console.WriteLine("X "+t.FullName+"."+m.Name+" => "+s);
            } catch {}
          }
          else if (op.OperandType == OperandType.ShortInlineBrTarget || op.OperandType == OperandType.ShortInlineI || op.OperandType == OperandType.ShortInlineVar) pos++;
          else if (op.OperandType == OperandType.InlineBrTarget || op.OperandType == OperandType.InlineI || op.OperandType == OperandType.ShortInlineR || op.OperandType == OperandType.InlineMethod || op.OperandType == OperandType.InlineField || op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok || op.OperandType == OperandType.InlineSig) pos += 4;
          else if (op.OperandType == OperandType.InlineI8 || op.OperandType == OperandType.InlineR) pos += 8;
          else if (op.OperandType == OperandType.InlineVar) pos += 2;
          else if (op.OperandType == OperandType.InlineSwitch) { int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n; }
        }
      } catch {}
    }
  }
}
