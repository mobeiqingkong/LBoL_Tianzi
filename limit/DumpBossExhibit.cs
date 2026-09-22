using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

class Program {
    static void Main() {
        var root = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\";
        var core = Assembly.LoadFrom(root + "LBoL.Core.dll");
        var stage = core.GetType("LBoL.Core.Stage");
        var m = stage.GetMethod("GetBossExhibits");
        var body = m.GetMethodBody();
        Console.WriteLine("Locals: " + body.LocalVariables.Count);
        var il = body.GetILAsByteArray();
        var module = m.Module;
        for (int i = 0; i < il.Length;) {
            var op = (OpCode)typeof(OpCodes).GetFields().Select(f => (OpCode)f.GetValue(null))
                .FirstOrDefault(o => o.Value == il[i] || (o.Size==2 && i+1<il.Length && (((ushort)(il[i]<<8)|il[i+1])== (ushort)o.Value)));
            // simpler: just dump member refs via MethodBody tokens
            i++;
        }
        // Use Mono.Cecil if available? Fall back: dump called methods via RuntimeMethodHandle is hard.
        // Instead list string literals via metadata
        foreach (var mi in stage.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
            if (mi.Name.Contains("Boss") || mi.Name.Contains("Exhibit"))
                Console.WriteLine(mi.ReturnType.Name + " " + mi.Name + "(" + string.Join(",", mi.GetParameters().Select(p=>p.ParameterType.Name)) + ")");
        }

        // AppearanceType
        var bas = Assembly.LoadFrom(root + "LBoL.Base.dll");
        var app = bas.GetType("LBoL.Base.AppearanceType");
        Console.WriteLine("App type: " + app);
        if (app != null) Console.WriteLine(string.Join(", ", Enum.GetNames(app)));

        // Search dialog related in presentation
        var pres = Assembly.LoadFrom(root + "LBoL.Presentation.dll");
        foreach (var t in SafeTypes(pres)) {
            if (t.Name.IndexOf("Dialog", StringComparison.OrdinalIgnoreCase)>=0 || t.Name.IndexOf("Chat", StringComparison.OrdinalIgnoreCase)>=0 || t.Name.IndexOf("Speech", StringComparison.OrdinalIgnoreCase)>=0)
                Console.WriteLine("PRES " + t.FullName);
        }
        foreach (var t in SafeTypes(core)) {
            if (t.Name.IndexOf("Dialog", StringComparison.OrdinalIgnoreCase)>=0 || t.Name.IndexOf("Chat", StringComparison.OrdinalIgnoreCase)>=0)
                Console.WriteLine("CORE " + t.FullName);
        }
    }
    static System.Collections.Generic.IEnumerable<Type> SafeTypes(Assembly a) {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
    }
}
