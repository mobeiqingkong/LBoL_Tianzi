using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Text;

class P {
  static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
  static P() { foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static)) map[((OpCode)f.GetValue(null)).Value]=(OpCode)f.GetValue(null); }
  static bool Mentions(MethodInfo m, string s) {
    try {
      var il = m.GetMethodBody()?.GetILAsByteArray(); if (il==null) return false;
      var mod = m.Module; int pos=0;
      while (pos < il.Length) {
        OpCode op; if (il[pos]==0xFE){pos++;op=map[(short)(0xFE00|il[pos++])];} else op=map[il[pos++]];
        switch(op.OperandType){
          case OperandType.InlineString: { int t=BitConverter.ToInt32(il,pos); pos+=4; try{ if(mod.ResolveString(t).IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0) return true;}catch{} break;}
          case OperandType.InlineMethod: case OperandType.InlineField: case OperandType.InlineType: case OperandType.InlineTok: case OperandType.InlineSig: {
            int t=BitConverter.ToInt32(il,pos); pos+=4;
            try { var mem=mod.ResolveMember(t); if (mem!=null && mem.Name.IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0) return true;
              if (mem!=null && mem.DeclaringType!=null && mem.DeclaringType.Name.IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0) return true;
            } catch {} break; }
          case OperandType.ShortInlineBrTarget: case OperandType.ShortInlineI: case OperandType.ShortInlineVar: pos++; break;
          case OperandType.InlineBrTarget: case OperandType.InlineI: case OperandType.ShortInlineR: pos+=4; break;
          case OperandType.InlineI8: case OperandType.InlineR: pos+=8; break;
          case OperandType.InlineVar: pos+=2; break;
          case OperandType.InlineSwitch: { int n=BitConverter.ToInt32(il,pos); pos+=4+4*n; break; }
          default: break;
        }
      }
    } catch {}
    return false;
  }
  static void Main(){
    string managed=@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach (var d in new[]{"UnityEngine.CoreModule.dll","UnityEngine.dll","UnityEngine.UI.dll","LBoL.Base.dll","YamlDotNet.dll","DOTween.dll","Unity.TextMeshPro.dll"})
      try{Assembly.LoadFrom(Path.Combine(managed,d));}catch{}
    var ent = Assembly.LoadFrom(Path.Combine(managed,"LBoL.EntityLib.dll"));
    int n=0;
    Type[] types;
    try { types = ent.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types.Where(t=>t!=null).ToArray(); }
    foreach (var t in types) {
      if (t==null || !t.IsSubclassOf(typeof(object))) continue;
      var m = t.GetMethod("SpecialGain", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
      if (m==null) continue;
      if (Mentions(m, "BossExhibit") || Mentions(m, "ExhibitPanel") || Mentions(m, "GainExhibit") || Mentions(m, "SampleMany") || Mentions(m, "InteractionViewer")) {
        Console.WriteLine(t.FullName + " SpecialGain mentions pick-ish");
        n++;
      }
    }
    Console.WriteLine("count="+n);

    // dump a few known jingnang-like if any
    foreach (var name in new[]{"JinNang","BaoXiang","Wanbaochui","SelectExhibit","ExhibitSelect"}) {
      foreach (var t in types.Where(x=>x!=null && x.Name.IndexOf(name,StringComparison.OrdinalIgnoreCase)>=0).Take(20))
        Console.WriteLine("namehit "+t.FullName);
    }
  }
}
