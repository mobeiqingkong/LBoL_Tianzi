using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

class P
{
    static Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
    static Type servant;

    static P()
    {
        foreach (var f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            map[((OpCode)f.GetValue(null)).Value] = (OpCode)f.GetValue(null);
    }

    static void Scan(Assembly a)
    {
        Type[] types;
        try { types = a.GetTypes(); }
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
                    var body = m.GetMethodBody();
                    if (body == null) continue;
                    var il = body.GetILAsByteArray();
                    var mod = m.Module;
                    int pos = 0;
                    while (pos < il.Length)
                    {
                        OpCode op;
                        if (il[pos] == 0xFE) { pos++; op = map[(short)(0xFE00 | il[pos++])]; }
                        else op = map[il[pos++]];
                        if (op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok
                            || op.OperandType == OperandType.InlineMethod || op.OperandType == OperandType.InlineField)
                        {
                            int token = BitConverter.ToInt32(il, pos); pos += 4;
                            try
                            {
                                if (op.OperandType == OperandType.InlineType || op.OperandType == OperandType.InlineTok)
                                {
                                    var ty = mod.ResolveType(token);
                                    if (ty == servant)
                                    {
                                        Console.WriteLine(t.FullName + "." + m.Name);
                                        break;
                                    }
                                }
                                else
                                {
                                    var mem = mod.ResolveMember(token);
                                    if (mem != null && mem.DeclaringType == servant)
                                    {
                                        Console.WriteLine(t.FullName + "." + m.Name + " -> " + mem);
                                        break;
                                    }
                                }
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
                        else if (op.OperandType == OperandType.InlineString)
                            pos += 4;
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
    }

    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        foreach (var d in new[] { "UnityEngine.CoreModule.dll", "LBoL.Base.dll", "YamlDotNet.dll" })
            try { Assembly.LoadFrom(Path.Combine(managed, d)); } catch { }
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        servant = core.GetType("LBoL.Core.StatusEffects.Servant");
        Scan(core);
        Console.WriteLine("done");

        var dc = core.GetType("LBoL.Core.DieCause");
        if (dc != null)
            foreach (var v in Enum.GetValues(dc))
                Console.WriteLine("DieCause " + (int)v + "=" + v);

        // Read full Servant localization
        var loc = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\StreamingAssets\Localization\zh-Hans\StatusEffect.yaml";
        bool print = false;
        foreach (var line in File.ReadLines(loc))
        {
            if (line.StartsWith("Servant:")) { print = true; Console.WriteLine(line); continue; }
            if (print)
            {
                if (line.Length > 0 && line[0] != ' ' && line.Contains(":")) break;
                Console.WriteLine(line);
            }
        }
    }
}
