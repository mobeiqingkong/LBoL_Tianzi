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

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "UnityEngine.UI.dll", "LBoL.Base.dll", "YamlDotNet.dll", "DOTween.dll", "Unity.TextMeshPro.dll" })
            try { Assembly.LoadFrom(Path.Combine(managed, d)); } catch { }
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));

        Type[] types;
        try { types = pres.GetTypes(); }
        catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

        foreach (var t in types)
        {
            if (t == null) continue;
            MethodInfo[] ms;
            try { ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly); }
            catch { continue; }
            foreach (var m in ms)
            {
                try
                {
                    var il = m.GetMethodBody()?.GetILAsByteArray();
                    if (il == null) continue;
                    var mod = m.Module;
                    int pos = 0;
                    while (pos < il.Length)
                    {
                        OpCode op;
                        if (il[pos] == 0xFE) { pos++; op = map[(short)(0xFE00 | il[pos++])]; }
                        else op = map[il[pos++]];
                        if (op.OperandType == OperandType.InlineMethod || op.OperandType == OperandType.InlineField
                            || op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok)
                        {
                            int token = BitConverter.ToInt32(il, pos); pos += 4;
                            try
                            {
                                var mem = mod.ResolveMember(token);
                                string n = mem != null ? mem.Name : "";
                                if (n.IndexOf("WaitUntilOptionSelected", StringComparison.Ordinal) >= 0
                                    || (n == "Show" && mem.DeclaringType != null && mem.DeclaringType.Name.IndexOf("Gap", StringComparison.Ordinal) >= 0))
                                {
                                    Console.WriteLine(t.FullName + "." + m.Name + " -> " + mem);
                                    break;
                                }
                            }
                            catch { }
                        }
                        else if (op.OperandType == OperandType.InlineString)
                        {
                            int token = BitConverter.ToInt32(il, pos); pos += 4;
                            try
                            {
                                var s = mod.ResolveString(token);
                                if (s != null && (s.IndexOf("前进") >= 0 || s.IndexOf("Advance") >= 0))
                                    Console.WriteLine("STR " + t.FullName + "." + m.Name + " \"" + s + "\"");
                            }
                            catch { }
                        }
                        else if (op.OperandType == OperandType.ShortInlineBrTarget || op.OperandType == OperandType.ShortInlineI || op.OperandType == OperandType.ShortInlineVar)
                            pos++;
                        else if (op.OperandType == OperandType.InlineBrTarget || op.OperandType == OperandType.InlineI || op.OperandType == OperandType.ShortInlineR)
                            pos += 4;
                        else if (op.OperandType == OperandType.InlineI8 || op.OperandType == OperandType.InlineR)
                            pos += 8;
                        else if (op.OperandType == OperandType.InlineVar)
                            pos += 2;
                        else if (op.OperandType == OperandType.InlineSwitch)
                        {
                            int n = BitConverter.ToInt32(il, pos);
                            pos += 4 + 4 * n;
                        }
                    }
                }
                catch { }
            }
        }
        Console.WriteLine("done");
    }
}
