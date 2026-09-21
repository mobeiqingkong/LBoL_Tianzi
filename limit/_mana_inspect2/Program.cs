using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
foreach (var path in args) {
  using var fs = File.OpenRead(path);
  using var pe = new PEReader(fs);
  var mr = pe.GetMetadataReader();
  foreach (var th in mr.TypeDefinitions) {
    var t = mr.GetTypeDefinition(th);
    var ns = mr.GetString(t.Namespace);
    var name = mr.GetString(t.Name);
    if (ns.EndsWith("StatusEffects.Basic") || (ns.Contains("StatusEffects") && name=="Grace"))
      if (name=="Grace" || name=="Firepower" || name=="Spirit" || name=="Amulet" || name=="AmuletForCard")
        Console.WriteLine(path.Split('\\').Last()+": "+ns+"."+name);
  }
}
