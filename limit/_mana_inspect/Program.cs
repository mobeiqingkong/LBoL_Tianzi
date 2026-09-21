using System;
using System.Linq;
using System.Reflection;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;

static void Dump(Type t)
{
    Console.WriteLine("==== " + t.FullName + " ====");
    foreach (var c in t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
        Console.WriteLine(" ctor " + c);
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        Console.WriteLine(" m " + m);
}

Dump(typeof(ConvertManaAction));
Dump(typeof(ManaGroup));
Dump(typeof(GainManaAction));
foreach (var t in typeof(ConvertManaAction).Assembly.GetTypes().Where(x => x.Name.Contains("Mana") && x.Namespace != null && x.Namespace.Contains("BattleActions")))
    Console.WriteLine("BA " + t.FullName);

var mg = new ManaGroup { Red = 2, Blue = 1, Philosophy = 1 };
Console.WriteLine("Amount=" + mg.Amount + " Philosophy=" + mg.Philosophy);
// list ManaGroup static methods related
foreach (var m in typeof(ManaGroup).GetMethods(BindingFlags.Public|BindingFlags.Static))
    Console.WriteLine("static " + m);
