using System;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
var path = @"D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll";
var d = new CSharpDecompiler(path, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
Console.WriteLine(d.DecompileTypeAsString(new FullTypeName("LBoL.Core.Battle.BattleActions.DrawManyCardAction")));
Console.WriteLine("---");
Console.WriteLine(d.DecompileTypeAsString(new FullTypeName("LBoL.Core.Battle.BattleActions.DrawCardAction")));
