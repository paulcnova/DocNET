
namespace DocNET;

using DocNET.Generators;

using System.Collections.Generic;

public class ProjectEnvironment
{
	#region Properties
	
	public const string GeneratorType_StaticHTML = "generator/static_html";
	public const string GeneratorType_Godot = "generator/godot";
	
	public required string ProjectName { get; set; }
	public required string OriginalAssembly { get; set; }
	public required List<string> Assemblies { get; set; }
	public required string GeneratorType { get; set; }
	public required bool IncludePrivate { get; set; }
	
	public bool IgnorePrivate => !this.IncludePrivate;
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Creates a documentation generator given the project's environments.</summary>
	/// <returns>The documentation generator used to generate the documentation.</returns>
	public IGenerator CreateGenerator()
	{
		// switch(this.GeneratorType)
		// {
		// 	case GeneratorType_StaticHTML: return new StaticHTMLGenerator();
		// 	case GeneratorType_Godot: return new GodotGenerator();
		// }
		
		// System.Console.WriteLine($"Could not find generator of: {this.GeneratorType}");
		
		return null;
	}
	
	#endregion // Public Methods
}
