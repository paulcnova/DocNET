
namespace DocNET;

using Mono.Cecil;

using System.Collections.Generic;

/// <summary>A class that holds a dictionary of assemblies to types as well as types to links for later reference</summary>
public sealed class SiteMap
{
	#region Properties
	
	public ProjectEnvironment Environment { get; private set; }
	public Dictionary<string, List<string>> Types { get; set; } = new Dictionary<string, List<string>>();
	
	/// <summary>A constructor that creates a site map from the given environment</summary>
	/// <param name="environment">The environment to create a site map for</param>
	public SiteMap(ProjectEnvironment environment)
	{
		this.Environment = environment;
		foreach(string assembly in this.Environment.Assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			int index = System.Math.Max(assembly.LastIndexOf('/'), assembly.LastIndexOf('\\'));
			string asmName = (index == -1 ? assembly : assembly.Substring(index + 1));
			
			if(!this.Types.ContainsKey(asmName))
			{
				this.Types.Add(asmName, new List<string>());
			}
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				foreach(TypeDefinition type in module.GetTypes())
				{
					if(type.FullName.Contains('<') && type.FullName.Contains('>'))
					{
						continue;
					}
					if(!this.Environment.IncludePrivate)
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
					this.Types[asmName].Add(type.FullName);
					this.Types[asmName].Sort();
				}
			}
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Finds all the types from the <c>environment</c> that is relevant to the project.</summary>
	/// <returns>A list of all the types relevant to the project</returns>
	public List<string> FindTypes()
	{
		if(this.Types.ContainsKey(this.Environment.OriginalAssembly))
		{
			return this.Types[this.Environment.OriginalAssembly];
		}
		return new List<string>();
	}
	
	#endregion // Public Methods
}
