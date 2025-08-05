
namespace DocNET;

using DocNET.Generation;
using DocNET.Inspections;
using DocNET.Information;
using DocNET.Linking;

using System.Collections.Generic;

/// <summary>A static class where the program starts from</summary>
public static class Program
{
	#region Public Methods
	
	public static void Main(string[] args)
	{
		CreateDocumentation();
	}
	
	public static void CreateDocumentation()
	{
		ProjectEnvironment environment = Loader.LoadCurrentEnvironment();
		Generator generator = environment.CreateGenerator();
		SiteMap siteMap = new SiteMap(environment);
		List<string> typesToDocument = siteMap.FindTypes();
		
		foreach(string type in typesToDocument)
		{
			TypeInspection inspectedType = new TypeInspection(type, environment);
			TypeInfo information = new TypeInfo(type, environment);
			Linker linker = new Linker(inspectedType, information, environment);
			
			foreach(LinkedType linkedType in linker)
			{
				GeneratedDocumentation documentation = generator.Generate(linkedType);
				
				documentation.Save(environment);
			}
		}
	}
	
	#endregion // Public Methods
}
