using System;
using System.Linq;
using System.Reflection;
using System.IO;

class Dump
{
    static void Main()
    {
        string managed = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed";
        var core = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Core.dll"));
        var pres = Assembly.LoadFrom(Path.Combine(managed, "LBoL.Presentation.dll"));

        void DumpType(Assembly a, string name)
        {
            var t = a.GetType(name);
            if (t == null) { Console.WriteLine("MISSING " + name); return; }
            Console.WriteLine("==== " + t.FullName + " ====");
            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .OrderBy(m => m.Name))
            {
                var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                Console.WriteLine("  " + (m.IsStatic ? "static " : "") + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
            }
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                Console.WriteLine("  FIELD " + f.FieldType.Name + " " + f.Name);
            foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                Console.WriteLine("  PROP " + p.PropertyType.Name + " " + p.Name);
        }

        DumpType(pres, "LBoL.Presentation.UI.Panels.BossExhibitPanel");
        DumpType(core, "LBoL.Core.Stations.GapStation");
        DumpType(core, "LBoL.Core.GapOptions.GapOption");
        DumpType(core, "LBoL.Core.GameRunController");

        // Find methods related to select exhibits / reward
        foreach (var t in core.GetTypes().Where(t => t.Name.Contains("Exhibit") || t.Name.Contains("Reward") || t.Name.Contains("Gap")))
        {
            if (t.Namespace != null && (t.Namespace.Contains("Stations") || t.Namespace.Contains("Gap") || t.Name.Contains("Reward")))
            {
                var interesting = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Show", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Choose", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Hide", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Done", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Pick", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();
                if (interesting.Length > 0)
                {
                    Console.WriteLine("---- " + t.FullName);
                    foreach (var m in interesting)
                    {
                        var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                        Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
                    }
                }
            }
        }
    }
}
