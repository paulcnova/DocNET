
namespace DocNET.Tasks;

using DocNET.Generators;
using DocNET.Inspections;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using System.Collections.Generic;
using System.IO;

public class GenerateDocumentation : Task
{
	#region Properties
	
	[Required] public string ProjectName { get; set; }
	[Required] public string AssembliesPath { get; set; }
	[Required] public string OriginalAssembly { get; set; }
	[Required] public string GeneratorType { get; set; }
	public bool IncludePrivate { get; set; } = false;
	
	#endregion // Properties
	
	#region Public Methods
	
	public override bool Execute()
	{
		string[] files = Directory.GetFiles(this.AssembliesPath, "*.dll", SearchOption.AllDirectories);
		ProjectEnvironment environment = new ProjectEnvironment()
		{
			ProjectName = this.ProjectName,
			OriginalAssembly = this.OriginalAssembly,
			Assemblies = new List<string>(files),
			GeneratorType = this.GeneratorType,
			IncludePrivate = this.IncludePrivate,
		};
		IGenerator generator = environment.CreateGenerator();
		
		if(generator == null) { return false; }
		
		SiteMap siteMap = new SiteMap(environment);
		List<string> typesToDocument = siteMap.FindTypes();
		
		foreach(string type in typesToDocument)
		{
		// 	TypeInspection inspectedType = new TypeInspection(type, environment);
		// 	TypeInfo information = new TypeInfo(type, environment);
		// 	Linker linker = new Linker(inspectedType, information, environment);
			
		// 	foreach(LinkedType linkedType in linker)
		// 	{
		// 		GeneratedDocumentation documentation = generator.Generate(linkedType);
				
		// 		documentation.Save(environment);
		// 	}
		}
		
		return true;
	}
	
	#endregion // Public Methods
}
