using System;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

var dll = args[0];
var needle = args.Length > 1 ? args[1] : "";
using var pe = new PEFile(dll);
foreach (var t in pe.Metadata.TypeDefinitions)
{
    var td = pe.Metadata.GetTypeDefinition(t);
    var ns = pe.Metadata.GetString(td.Namespace);
    var name = pe.Metadata.GetString(td.Name);
    var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    if (needle.Length == 0 || full.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
        Console.WriteLine(full);
}
