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
    static string outDir;

    static Program()
    {
        foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            map[((OpCode)f.GetValue(null)).Value] = (OpCode)f.GetValue(null);
    }

    static string Dump(MethodInfo method)
    {
        if (method == null) return "null";
        var body = method.GetMethodBody();
        if (body == null) return "nobody";
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
                    {
                        int token = BitConverter.ToInt32(il, pos); pos += 4;
                        try { sb.Append(" \"").Append(mod.ResolveString(token)).Append("\""); }
                        catch { sb.Append(" str"); }
                        break;
                    }
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                case OperandType.InlineSig:
                    {
                        int token = BitConverter.ToInt32(il, pos); pos += 4;
                        try { sb.Append(" ").Append(mod.ResolveMember(token)); }
                        catch { sb.Append(" tok"); }
                        break;
                    }
                case OperandType.InlineSwitch:
                    {
                        int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n;
                        sb.Append(" switch(").Append(n).Append(")");
                        break;
                    }
                default: sb.Append(" ?"); break;
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    static string Safe(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s.Replace('<', '_').Replace('>', '_');
    }

    static void DumpNested(Type t, string tag)
    {
        if (t == null) return;
        foreach (var nt in t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
        {
            Console.WriteLine(tag + " nest " + nt.Name);
            var mn = nt.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mn == null) continue;
            if (!(nt.Name.Contains("TriggerGain") || nt.Name.Contains("GainExhibit") || nt.Name.Contains("FindExhibit") || nt.Name.Contains("SpecialGain")))
                continue;
            var path = Path.Combine(outDir, "il_" + tag + "_" + Safe(nt.Name) + ".txt");
            File.WriteAllText(path, Dump(mn));
            Console.WriteLine("Wrote " + path);
            Console.WriteLine(File.ReadAllText(path));
        }
    }

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        outDir = @"D:\riderProject\hextech\limit";
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "UnityEngine.UI.dll", "LBoL.Base.dll", "YamlDotNet.dll", "DOTween.dll", "Unity.TextMeshPro.dll" })
        {
            var p = Path.Combine(managed, d);
            if (File.Exists(p)) try { Assembly.LoadFrom(p); } catch { }
        }
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));

        DumpNested(core.GetType("LBoL.Core.Exhibit"), "ex");
        DumpNested(core.GetType("LBoL.Core.GameRunController"), "gr");
        DumpNested(core.GetType("LBoL.Core.Stations.GapStation"), "gs");

        var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
        File.WriteAllText(Path.Combine(outDir, "il_gapm_DrinkTea.txt"),
            Dump(gap.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First(m => m.Name == "DrinkTea")));
        File.WriteAllText(Path.Combine(outDir, "il_gapm_OnShowing.txt"),
            Dump(gap.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First(m => m.Name == "OnShowing")));

        var ri = core.GetType("LBoL.Core.Battle.Interactions.RewardInteraction");
        if (ri != null)
        {
            Console.WriteLine("RewardInteraction:");
            foreach (var m in ri.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
            foreach (var p in ri.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                Console.WriteLine("  PROP " + p.PropertyType.Name + " " + p.Name);
            foreach (var f in ri.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                Console.WriteLine("  FIELD " + f.FieldType.Name + " " + f.Name);
            foreach (var c in ri.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                Console.WriteLine("  CTOR(" + string.Join(",", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
        }

        Console.WriteLine("DrinkTea:\n" + File.ReadAllText(Path.Combine(outDir, "il_gapm_DrinkTea.txt")));
        Console.WriteLine("done");
    }
}
