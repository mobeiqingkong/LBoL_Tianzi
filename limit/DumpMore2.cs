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
        {
            var op = (OpCode)f.GetValue(null);
            map[op.Value] = op;
        }
    }

    static string Dump(MethodInfo method)
    {
        if (method == null) return "null method";
        var body = method.GetMethodBody();
        if (body == null) return "no body " + method.Name;
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
                        catch { sb.Append(" str ").Append(token); }
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
                        catch { sb.Append(" tok ").Append(token.ToString("X8")); }
                        break;
                    }
                case OperandType.InlineSwitch:
                    {
                        int n = BitConverter.ToInt32(il, pos); pos += 4 + 4 * n;
                        sb.Append(" switch(").Append(n).Append(")"); break;
                    }
                default: sb.Append(" ?"); break;
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    static void ListMethods(Type t)
    {
        if (t == null) { Console.WriteLine("null type"); return; }
        Console.WriteLine("==== " + t.FullName);
        foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).OrderBy(m => m.Name))
        {
            var ps = string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name));
            Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
        }
    }

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "UnityEngine.UI.dll", "UnityEngine.IMGUIModule.dll", "UnityEngine.TextRenderingModule.dll", "UnityEngine.AnimationModule.dll", "Unity.TextMeshPro.dll", "LBoL.Base.dll", "YamlDotNet.dll", "DOTween.dll", "0Harmony.dll", "MonoMod.Utils.dll", "Mono.Cecil.dll" })
        {
            var p = Path.Combine(managed, d);
            if (File.Exists(p)) try { Assembly.LoadFrom(p); } catch { }
        }
        // also BepInEx
        var bepin = @"D:\software\steam\steamapps\common\LBoL\BepInEx\core";
        if (Directory.Exists(bepin))
            foreach (var f in Directory.GetFiles(bepin, "*.dll"))
                try { Assembly.LoadFrom(f); } catch { }

        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));
        var outDir = @"D:\riderProject\hextech\limit";

        string[] names = new[] {
            "LBoL.Core.Stations.GapStation",
            "LBoL.Core.Exhibit",
            "LBoL.Core.GameRunController",
            "LBoL.Core.Battle.Interactions.SelectCardInteraction",
            "LBoL.Core.Interactions.SelectExhibitInteraction",
            "LBoL.Presentation.UI.Panels.GapOptionsPanel",
            "LBoL.Presentation.UI.Panels.BossExhibitPanel"
        };
        foreach (var n in names)
        {
            Type t = null;
            try { t = core.GetType(n) ?? pres.GetType(n); } catch (Exception err) { Console.WriteLine("err "+n+": "+err.Message); }
            ListMethods(t);
        }

        // try find Select*Exhibit*
        foreach (var asm in new[] { core, pres })
        {
            try
            {
                foreach (var t in asm.GetExportedTypes())
                {
                    if (t.Name.IndexOf("Exhibit", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        (t.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0 || t.Name.IndexOf("Interaction", StringComparison.OrdinalIgnoreCase) >= 0))
                        Console.WriteLine("FOUND " + t.FullName);
                }
            }
            catch (ReflectionTypeLoadException rtl)
            {
                foreach (var t in rtl.Types)
                {
                    if (t == null) continue;
                    if (t.Name.IndexOf("Exhibit", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        (t.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0 || t.Name.IndexOf("Interaction", StringComparison.OrdinalIgnoreCase) >= 0 || t.Name.IndexOf("Reward", StringComparison.OrdinalIgnoreCase) >= 0))
                        Console.WriteLine("FOUND " + t.FullName);
                }
            }
        }

        var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
        foreach (var n in new[] { "DrinkTea", "UpgradeCard", "FindExhibit", "Hide", "OnShowing" })
        {
            var m = gap.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            File.WriteAllText(Path.Combine(outDir, "il_gap_" + n + "2.txt"), Dump(m));
        }

        var gs = core.GetType("LBoL.Core.Stations.GapStation");
        if (gs != null)
        {
            foreach (var m in gs.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (m.Name.Contains("Enter") || m.Name.Contains("Process") || m.Name.Contains("Leave") || m.Name.Contains("Option") || m.Name.Contains("Run") || m.Name.Contains("Visit"))
                    File.WriteAllText(Path.Combine(outDir, "il_gs_" + m.Name + ".txt"), Dump(m));
            }
        }

        var boss = pres.GetType("LBoL.Presentation.UI.Panels.BossExhibitPanel");
        foreach (var m in boss.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            if (m.Name == "Show" || m.Name.StartsWith("Co") || m.Name.Contains("Gain"))
                File.WriteAllText(Path.Combine(outDir, "il_boss_" + m.Name + ".txt"), Dump(m));
        }

        var exhibitType = core.GetType("LBoL.Core.Exhibit");
        foreach (var m in exhibitType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            if (m.Name.Contains("Special") || m.Name.Contains("Gain") || m.Name == "Added")
                File.WriteAllText(Path.Combine(outDir, "il_ex_" + m.Name + ".txt"), Dump(m));

        Console.WriteLine("done");
    }
}

