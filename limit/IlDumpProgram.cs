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
        var il = method.GetMethodBody().GetILAsByteArray();
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
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "UnityEngine.UI.dll", "LBoL.Base.dll", "YamlDotNet.dll", "DOTween.dll", "Unity.TextMeshPro.dll" })
        {
            try { Assembly.LoadFrom(Path.Combine(managed, d)); } catch { }
        }
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));
        var gap = pres.GetType("LBoL.Presentation.UI.Panels.GapOptionsPanel");
        var boss = pres.GetType("LBoL.Presentation.UI.Panels.BossExhibitPanel");
        var outDir = @"D:\riderProject\hextech\limit";
        foreach (var n in new[] { "SelectedAndHide", "OptionClicked", "WaitUntilOptionSelected" })
        {
            var m = gap.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            File.WriteAllText(Path.Combine(outDir, "il_" + n + ".txt"), Dump(m));
        }
        File.WriteAllText(Path.Combine(outDir, "il_OnClickExhibit.txt"),
            Dump(boss.GetMethod("OnClickExhibit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
        File.WriteAllText(Path.Combine(outDir, "il_OnShowingBoss.txt"),
            Dump(boss.GetMethod("OnShowing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
        Console.WriteLine("done");
    }
}
