using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

class Program {
  static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
  static Program() {
    foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static))
      map[((OpCode)f.GetValue(null)).Value]=(OpCode)f.GetValue(null);
  }
  static string Dump(MethodInfo method) {
    if (method==null) return "null";
    var body=method.GetMethodBody(); if(body==null) return "nobody";
    var il=body.GetILAsByteArray(); var mod=method.Module; var sb=new StringBuilder(); int pos=0;
    while(pos<il.Length){
      int start=pos; OpCode op;
      if(il[pos]==0xFE){pos++;op=map[(short)(0xFE00|il[pos++])];} else op=map[il[pos++]];
      sb.Append("IL_").Append(start.ToString("X4")).Append(": ").Append(op.Name);
      switch(op.OperandType){
        case OperandType.InlineNone: break;
        case OperandType.ShortInlineBrTarget:{int t=pos+1+(sbyte)il[pos];pos++;sb.Append(" IL_").Append(t.ToString("X4"));break;}
        case OperandType.InlineBrTarget:{int t=pos+4+BitConverter.ToInt32(il,pos);pos+=4;sb.Append(" IL_").Append(t.ToString("X4"));break;}
        case OperandType.ShortInlineI: case OperandType.ShortInlineVar: sb.Append(" ").Append(il[pos++]); break;
        case OperandType.InlineVar: sb.Append(" ").Append(BitConverter.ToUInt16(il,pos)); pos+=2; break;
        case OperandType.InlineI: sb.Append(" ").Append(BitConverter.ToInt32(il,pos)); pos+=4; break;
        case OperandType.InlineI8: sb.Append(" ").Append(BitConverter.ToInt64(il,pos)); pos+=8; break;
        case OperandType.ShortInlineR: sb.Append(" ").Append(BitConverter.ToSingle(il,pos)); pos+=4; break;
        case OperandType.InlineR: sb.Append(" ").Append(BitConverter.ToDouble(il,pos)); pos+=8; break;
        case OperandType.InlineString:{int token=BitConverter.ToInt32(il,pos);pos+=4; try{sb.Append(" \"").Append(mod.ResolveString(token)).Append("\"");}catch{sb.Append(" str");} break;}
        case OperandType.InlineMethod: case OperandType.InlineField: case OperandType.InlineType: case OperandType.InlineTok: case OperandType.InlineSig:
          {int token=BitConverter.ToInt32(il,pos);pos+=4; try{sb.Append(" ").Append(mod.ResolveMember(token));}catch{sb.Append(" tok");} break;}
        case OperandType.InlineSwitch:{int n=BitConverter.ToInt32(il,pos);pos+=4+4*n;sb.Append(" switch("+n+")");break;}
        default: sb.Append(" ?"); break;
      }
      sb.AppendLine();
    }
    return sb.ToString();
  }
  static void Main(){
    string managed=@"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
    foreach(var d in new[]{"UnityEngine.CoreModule.dll","UnityEngine.dll","UnityEngine.UI.dll","LBoL.Base.dll","YamlDotNet.dll","DOTween.dll","Unity.TextMeshPro.dll"})
      try{Assembly.LoadFrom(Path.Combine(managed,d));}catch{}
    var core=Assembly.LoadFrom(Path.Combine(managed,"LBoL.Core.dll"));
    var pres=Assembly.LoadFrom(Path.Combine(managed,"LBoL.Presentation.dll"));
    var gap=pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
    Console.WriteLine("=== GapOptionsPanel fields/props/methods ===");
    foreach(var f in gap.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
      Console.WriteLine("FIELD "+f.FieldType.Name+" "+f.Name);
    foreach(var p in gap.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
      Console.WriteLine("PROP "+p.PropertyType.Name+" "+p.Name);
    foreach(var m in gap.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).OrderBy(m=>m.Name))
      Console.WriteLine(m.ReturnType.Name+" "+m.Name+"("+string.Join(",",m.GetParameters().Select(x=>x.ParameterType.Name))+")");

    // GapStation OnEnter + nested
    var gs=core.GetType("LBoL.Core.Stations.GapStation");
    foreach(var nt in gs.GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public)){
      Console.WriteLine("GS nest "+nt.Name);
      var mn=nt.GetMethod("MoveNext",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
      if(mn!=null) File.WriteAllText(@"D:\riderProject\hextech\limit\il_gs_"+nt.Name.Replace('<','_').Replace('>','_')+".txt", Dump(mn));
    }
    File.WriteAllText(@"D:\riderProject\hextech\limit\il_gs_OnEnter.txt", Dump(gs.GetMethod("OnEnter",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)));

    // Find how presentation shows gap - search for WaitUntilOptionSelected callers
    var types = new[]{pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel")};
    // MiniXijian equivalent - baota gap leave button?
    // Look at UiManager gap flow in presentation
    foreach(var name in new[]{
      "LBoL.Presentation.Stations.GapInteraction","LBoL.Presentation.UI.GapInteraction",
      "LBoL.Presentation.IGameRunVisualTrigger","LBoL.Core.IGameRunVisualTrigger"})
    {
      var t=pres.GetType(name)??core.GetType(name);
      Console.WriteLine(name+" => "+(t!=null));
    }
  }
}
