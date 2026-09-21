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
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","LBoL.Base.dll","YamlDotNet.dll"})
      try { Assembly.LoadFrom(Path.Combine(managed,d)); } catch {}
    var core = Assembly.LoadFrom(Path.Combine(managed,"LBoL.Core.dll"));
    var bc = core.GetType("LBoL.Core.Battle.BattleController");
    var c = bc.GetNestedType("<>c", BindingFlags.NonPublic);
    var m = c.GetMethod("<get_BattleShouldEnd>b__176_0", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
    Console.WriteLine("b176:\n"+Dump(m));

    var fk = core.GetType("LBoL.Core.Battle.BattleActions.ForceKillAction");
    foreach (var mm in fk.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
      Console.WriteLine("FK "+mm.Name);
    foreach (var nt in fk.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public)) {
      var mn = nt.GetMethod("MoveNext", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
      if (mn != null) { Console.WriteLine("FK nested "+nt.Name+"\n"+Dump(mn)); File.WriteAllText(@"D:\riderProject\hextech\limit\il_ForceKill.txt", Dump(mn)); }
    }

    var die = core.GetType("LBoL.Core.Battle.BattleActions.DieAction");
    foreach (var ctor in die.GetConstructors())
      Console.WriteLine("DieAction CTOR("+string.Join(",", ctor.GetParameters().Select(p=>p.ParameterType.Name+" "+p.Name))+")");
    foreach (var f in die.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
      Console.WriteLine("Die FIELD "+f.FieldType.Name+" "+f.Name);

    // UnitStatus
    var us = core.GetType("LBoL.Core.Units.UnitStatus");
    if (us != null) foreach (var v in Enum.GetValues(us)) Console.WriteLine("UnitStatus "+(int)v+"="+v);

    // DieCause
    var dc = core.GetType("LBoL.Core.DieCause");
    if (dc != null) foreach (var v in Enum.GetValues(dc)) Console.WriteLine("DieCause "+(int)v+"="+v);
  }
}
