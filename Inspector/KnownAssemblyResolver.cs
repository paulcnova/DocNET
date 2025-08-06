
namespace DocNET.Inspections;

using Mono.Cecil;

using System.Collections.Generic;

public sealed class KnownAssemblyResolver : BaseAssemblyResolver
{
	#region Properties
	
	private DefaultAssemblyResolver defaultResolver;
	private List<AssemblyDefinition> definitions;
	
	public KnownAssemblyResolver(List<AssemblyDefinition> definitions)
	{
		this.defaultResolver = new DefaultAssemblyResolver();
		this.definitions = definitions;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public override AssemblyDefinition Resolve(AssemblyNameReference name)
	{
		AssemblyDefinition asm;
		
		try
		{
			asm = this.defaultResolver.Resolve(name);
		}
		catch
		{
			foreach(AssemblyDefinition definition in this.definitions)
			{
				if(name.FullName == definition.FullName)
				{
					asm = definition;
					break;
				}
			}
			asm = null;
		}
		
		return asm;
	}
	
	#endregion // Public Methods
}
