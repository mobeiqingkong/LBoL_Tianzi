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

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var name = new AssemblyName(e.Name).Name + ".dll";
            var p = Path.Combine(managed, name);
            if (File.Exists(p)) return Assembly.LoadFrom(p);
            return null;
        };
        foreach (var d in Directory.GetFiles(managed, "*.dll"))
        {
            try { Assembly.LoadFrom(d); } catch { }
        }
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));
        var outDir = @"D:\riderProject\hextech\limit";

        // interactions with Exhibit in name
        Console.WriteLine("=== Interactions ===");
        foreach (var t in core.GetTypes())
        {
            try
            {
                if (t.FullName != null && (t.FullName.Contains("Interaction") || t.Name.Contains("Select")))
                    if (t.Name.IndexOf("Exhibit", StringComparison.OrdinalIgnoreCase) >= 0
                        || t.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0)
                        Console.WriteLine(t.FullName);
            }
            catch { }
        }

        var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
        foreach (var n in new[] { "DrinkTea", "UpgradeCard", "FindExhibit", "GetMoney", "RemoveCard", "InternalGetRareCard", "UpgradeBaota", "Hide", "Show", "OnShowing" })
        {
            var m = gap.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m != null)
                File.WriteAllText(Path.Combine(outDir, "il_gap_" + n + ".txt"), Dump(m));
        }

        // GapStation
        var gs = core.GetType("LBoL.Core.Stations.GapStation");
        Console.WriteLine("=== GapStation ===");
        if (gs != null)
        {
            foreach (var m in gs.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).OrderBy(m => m.Name))
            {
                var ps = string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name));
                Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
            }
            foreach (var n in new[] { "OnEnter", "InternalProcess", "Process", "Leave", "SelectOption" })
            {
                var m = gs.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null)
                    File.WriteAllText(Path.Combine(outDir, "il_gapstation_" + n + ".txt"), Dump(m));
            }
        }

        // Boss Show methods
        var boss = pres.GetType("LBoL.Presentation.UI.Panels.BossExhibitPanel");
        Console.WriteLine("=== BossExhibitPanel Show-ish ===");
        foreach (var m in boss.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            if (m.Name.IndexOf("Show", StringComparison.OrdinalIgnoreCase) >= 0
                || m.Name.IndexOf("Gain", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
                File.WriteAllText(Path.Combine(outDir, "il_boss_" + m.Name + ".txt"), Dump(m));
            }
        }

        // Exhibit SpecialGain caller
        var ex = core.GetType("LBoL.Core.Exhibit");
        Console.WriteLine("=== Exhibit gain methods ===");
        foreach (var m in ex.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            if (m.Name.IndexOf("Gain", StringComparison.OrdinalIgnoreCase) >= 0
                || m.Name.IndexOf("Special", StringComparison.OrdinalIgnoreCase) >= 0
                || m.Name.IndexOf("Added", StringComparison.OrdinalIgnoreCase) >= 0)
                Console.WriteLine(m.ReturnType.Name + " " + m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");

        Console.WriteLine("done");
    }
}
