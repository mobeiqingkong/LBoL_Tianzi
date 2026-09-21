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
  static string Dump(MethodInfo method) {
    if (method == null) return "null";
    var body = method.GetMethodBody();
    if (body == null) return "nobody";
    var il = body.GetILAsByteArray();
    var mod = method.Module;
    var sb = new StringBuilder();
    int pos = 0;
    while (pos < il.Length) {
      int start = pos;
      OpCode op;
      if (il[pos] == 0xFE) { pos++; op = map[(short)(0xFE00 | il[pos++])]; }
      else op = map[il[pos++]];
      sb.Append("IL_").Append(start.ToString("X4")).Append(": ").Append(op.Name);
      switch (op.OperandType) {
        case OperandType.InlineNone: break;
        case OperandType.ShortInlineBrTarget: { int t = pos+1+(sbyte)il[pos]; pos++; sb.Append(" IL_").Append(t.ToString("X4")); break; }
        case OperandType.InlineBrTarget: { int t = pos+4+BitConverter.ToInt32(il,pos); pos+=4; sb.Append(" IL_").Append(t.ToString("X4")); break; }
        case OperandType.ShortInlineI: case OperandType.ShortInlineVar: sb.Append(" ").Append(il[pos++]); break;
        case OperandType.InlineVar: sb.Append(" ").Append(BitConverter.ToUInt16(il,pos)); pos+=2; break;
        case OperandType.InlineI: sb.Append(" ").Append(BitConverter.ToInt32(il,pos)); pos+=4; break;
        case OperandType.InlineI8: sb.Append(" ").Append(BitConverter.ToInt64(il,pos)); pos+=8; break;
        case OperandType.ShortInlineR: sb.Append(" ").Append(BitConverter.ToSingle(il,pos)); pos+=4; break;
        case OperandType.InlineR: sb.Append(" ").Append(BitConverter.ToDouble(il,pos)); pos+=8; break;
        case OperandType.InlineString: {
          int token=BitConverter.ToInt32(il,pos); pos+=4;
          try { sb.Append(" \"").Append(mod.ResolveString(token)).Append("\""); } catch { sb.Append(" str"); }
          break;
        }
        case OperandType.InlineMethod: case OperandType.InlineField: case OperandType.InlineType: case OperandType.InlineTok: case OperandType.InlineSig: {
          int token=BitConverter.ToInt32(il,pos); pos+=4;
          try { sb.Append(" ").Append(mod.ResolveMember(token)); } catch { sb.Append(" tok"); }
          break;
        }
        case OperandType.InlineSwitch: {
          int n=BitConverter.ToInt32(il,pos); pos+=4+4*n; sb.Append(" switch("+n+")"); break;
        }
        default: sb.Append(" ?"); break;
      }
      sb.AppendLine();
    }
    return sb.ToString();
  }
  static void ListTypes(Assembly a, string pred) {
    Type[] types;
    try { types = a.GetTypes(); }
    catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
    foreach (var t in types) {
      if (t == null) continue;
      if (t.Name.IndexOf(pred, StringComparison.OrdinalIgnoreCase) >= 0)
        Console.WriteLine(a.GetName().Name + " " + t.FullName + " : " + (t.BaseType != null ? t.BaseType.Name : ""));
    }
  }
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","LBoL.Base.dll","YamlDotNet.dll"})
      try { Assembly.LoadFrom(Path.Combine(managed,d)); } catch {}
    var core = Assembly.LoadFrom(Path.Combine(managed,"LBoL.Core.dll"));
    var ent = Assembly.LoadFrom(Path.Combine(managed,"LBoL.EntityLib.dll"));
    ListTypes(core, "Servant");
    ListTypes(ent, "Servant");
    ListTypes(core, "Minion");
    ListTypes(ent, "Minion");
    ListTypes(ent, "Summon");

    // Direct get type used by UndyingSe
    var s1 = core.GetType("LBoL.Core.StatusEffects.Servant");
    var s2 = ent.GetType("LBoL.EntityLib.StatusEffects.Basic.Servant");
    var s3 = ent.GetType("LBoL.EntityLib.StatusEffects.Enemy.Servant");
    var s4 = ent.GetType("LBoL.EntityLib.StatusEffects.Others.Servant");
    Console.WriteLine("core Servant="+s1);
    Console.WriteLine("basic Servant="+s2);
    Console.WriteLine("enemy Servant="+s3);
    Console.WriteLine("others Servant="+s4);

    // Search localization id for 使魔
    var loc = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\StreamingAssets\Localization\zh-Hans\StatusEffect.yaml";
    string cur = null;
    foreach (var line in File.ReadLines(loc)) {
      if (line.Length > 0 && line[0] != ' ' && line.EndsWith(":"))
        cur = line.TrimEnd(':');
      if (line.Contains("使魔"))
        Console.WriteLine("ID="+cur+" "+line.Trim());
    }
  }
}
