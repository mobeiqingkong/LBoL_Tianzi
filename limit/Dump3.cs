using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

class Program
{
    static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
    static Program()
    {
        foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            map[((OpCode)f.GetValue(null)).Value] = (OpCode)f.GetValue(null);
    }

    static MethodInfo One(Type t, string n)
    {
        var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.Name == n).ToArray();
        if (ms.Length == 0) return null;
        if (ms.Length == 1) return ms[0];
        // prefer declared
        var d = ms.FirstOrDefault(m => m.DeclaringType == t);
        return d ?? ms[0];
    }

    static string Dump(MethodInfo method)
    {
        if (method == null) return "null";
        var body = method.GetMethodBody();
        if (body == null) return "nobody " + method.DeclaringType.Name + "." + method.Name;
        var il = body.GetILAsByteArray();
        var mod = method.Module;
        var sb = new StringBuilder();
        int pos = 0;
        while (pos < il.Length)
        {
            int start = pos;
            OpCode op;
            if (il[pos] == 0xFE) { pos++; op = map[(short)(0xFE00 | il[pos++])]; }
            else op = map[il[pos++]];
            sb.Append("IL_").Append(start.ToString("X4")).Append(": ").Append(op.Name);
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineBrTarget:
                    { int t = pos + 1 + (sbyte)il[pos]; pos++; sb.Append(" IL_").Append(t.ToString("X4")); break; }
                case OperandType.InlineBrTarget:
                    { int t = pos + 4 + BitConverter.ToInt32(il, pos); pos += 4; sb.Append(" IL_").Append(t.ToString("X4")); break; }
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar: sb.Append(" ").Append(il[pos++]); break;
                case OperandType.InlineVar: sb.Append(" ").Append(BitConverter.ToUInt16(il, pos)); pos += 2; break;
                case OperandType.InlineI: sb.Append(" ").Append(BitConverter.ToInt32(il, pos)); pos += 4; break;
                case OperandType.InlineI8: sb.Append(" ").Append(BitConverter.ToInt64(il, pos)); pos += 8; break;
                case OperandType.ShortInlineR: sb.Append(" ").Append(BitConverter.ToSingle(il, pos)); pos += 4; break;
                case OperandType.InlineR: sb.Append(" ").Append(BitConverter.ToDouble(il, pos)); pos += 8; break;
                case OperandType.InlineString:
                    { int token = BitConverter.ToInt32(il, pos); pos += 4;
                      try { sb.Append(" \"").Append(mod.ResolveString(token)).Append("\""); } catch { sb.Append(" str"); }
                      break; }
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                case OperandType.InlineSig:
                    { int token = BitConverter.ToInt32(il, pos); pos += 4;
                      try { sb.Append(" ").Append(mod.ResolveMember(token)); } catch { sb.Append(" tok"); }
                      break; }
                case OperandType.InlineSwitch:
                    { int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n; sb.Append(" switch(").Append(n).Append(")"); break; }
                default: sb.Append(" ?"); break;
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "UnityEngine.UI.dll", "LBoL.Base.dll", "YamlDotNet.dll", "DOTween.dll", "Unity.TextMeshPro.dll" })
        {
            var p = Path.Combine(managed, d);
            if (File.Exists(p)) try { Assembly.LoadFrom(p); } catch { }
        }
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));
        var outDir = @"D:\riderProject\hextech\limit";

        var ex = core.GetType("LBoL.Core.Exhibit");
        foreach (var n in new[] { "TriggerGain", "SpecialGain", "OnGain", "TriggerGainInstantly" })
            File.WriteAllText(Path.Combine(outDir, "il_ex_" + n + ".txt"), Dump(One(ex, n)));

        var gr = core.GetType("LBoL.Core.GameRunController");
        File.WriteAllText(Path.Combine(outDir, "il_gr_GainExhibitRunner.txt"), Dump(One(gr, "GainExhibitRunner")));
        File.WriteAllText(Path.Combine(outDir, "il_gr_GainExhibitInstantly.txt"), Dump(One(gr, "GainExhibitInstantly")));

        var gs = core.GetType("LBoL.Core.Stations.GapStation");
        File.WriteAllText(Path.Combine(outDir, "il_gs_OnEnter.txt"), Dump(One(gs, "OnEnter")));
        File.WriteAllText(Path.Combine(outDir, "il_gs_FindExhibitRunner.txt"), Dump(One(gs, "FindExhibitRunner")));

        var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
        foreach (var n in new[] { "DrinkTea", "Hide", "OnShowing", "UpgradeCard", "FindExhibit", "OnHided" })
            File.WriteAllText(Path.Combine(outDir, "il_gapm_" + n + ".txt"), Dump(One(gap, n)));
        var pred = gap.GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).FirstOrDefault(m => m.Name.Contains("WaitUntilOptionSelected") && m.Name.Contains("b__"));
        File.WriteAllText(Path.Combine(outDir, "il_gapm_waitpred.txt"), Dump(pred));

        try
        {
            foreach (var t in core.GetTypes())
            {
                if (t != null && t.Namespace != null && t.Namespace.Contains("Interaction"))
                    Console.WriteLine("INT " + t.FullName);
            }
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var t in e.Types)
                if (t != null && t.Namespace != null && t.Namespace.Contains("Interaction"))
                    Console.WriteLine("INT " + t.FullName);
        }

        Console.WriteLine("TriggerGain:\n" + File.ReadAllText(Path.Combine(outDir, "il_ex_TriggerGain.txt")));
        Console.WriteLine("done");
    }
}
