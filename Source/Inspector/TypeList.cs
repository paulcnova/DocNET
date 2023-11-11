
namespace Taco.DocNET.Inspector;

using Mono.Cecil;

using System.Collections.Generic;

/// <summary>All the information of types with it's associated library or executable</summary>
public class TypeList
{
	
	#region Properties
	
	/// <summary>A hashmap of a library or executable mapping to a list of types it contains</summary>
	public Dictionary<string, List<string>> Types { get; set; } = new Dictionary<string, List<string>>();
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates a type list</summary>
	/// <param name="assemblies">The assemblies to look into</param>
	/// <returns>Returns the type list generated</returns>
	public static TypeList GenerateList(params string[] assemblies)
	{
		TypeList list = new TypeList();
		
		foreach(string assembly in assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			int index = System.Math.Max(assembly.LastIndexOf('/'), assembly.LastIndexOf('\\'));
			string asmName = (index == -1 ? assembly : assembly.Substring(index + 1));
			
			if(!list.Types.ContainsKey(asmName))
			{
				list.Types.Add(asmName, new List<string>());
			}
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				foreach(TypeDefinition type in module.GetTypes())
				{
					if(type.FullName.Contains('<') && type.FullName.Contains('>'))
					{
						continue;
					}
					if(TypeInfo.ignorePrivate)
					{
						if(type.IsNotPublic) { continue; }
						if(type.IsNestedAssembly || type.IsNestedPrivate) { continue; }
						
						TypeDefinition nestedType = type;
						
						while(nestedType.IsNested)
						{
							nestedType = nestedType.DeclaringType;
						}
						
						if(nestedType.IsNotPublic) { continue; }
					}
					list.Types[asmName].Add(type.FullName);
					list.Types[asmName].Sort();
				}
			}
		}
		
		return list;
	}
	
	#endregion // Public Methods
}
