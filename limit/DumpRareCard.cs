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
  static string Safe(string s) {
    foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
    return s.Replace('<','_').Replace('>','_');
  }
  static void Main() {
    string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","UnityEngine.dll","UnityEngine.UI.dll","LBoL.Base.dll","YamlDotNet.dll","DOTween.dll","Unity.TextMeshPro.dll"})
      try { Assembly.LoadFrom(Path.Combine(managed,d)); } catch {}
    var pres = Assembly.LoadFrom(Path.Combine(managed,"LBoL.Presentation.dll"));
    var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
    foreach (var n in new[]{"GetRareCardRunner","UpgradeCardRunner","RemoveCardRunner","CoFindExhibit","Hide","OnHided"}) {
      var m = gap.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).FirstOrDefault(x=>x.Name==n);
      if (m != null) {
        // iterator stub
        File.WriteAllText(@"D:\riderProject\hextech\limit\il_gapr_"+n+".txt", Dump(m));
        Console.WriteLine(n+" stub done");
      }
    }
    foreach (var nt in gap.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public)) {
      if (!(nt.Name.Contains("GetRareCard") || nt.Name.Contains("UpgradeCard") || nt.Name.Contains("RemoveCard") || nt.Name.Contains("CoFindExhibit") || nt.Name.Contains("FindExhibit")))
        continue;
      var mn = nt.GetMethod("MoveNext", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
      if (mn == null) continue;
      var path = @"D:\riderProject\hextech\limit\il_gapr_"+Safe(nt.Name)+".txt";
      File.WriteAllText(path, Dump(mn));
      Console.WriteLine("Wrote "+path);
      Console.WriteLine(File.ReadAllText(path));
      Console.WriteLine("====");
    }
  }
}
