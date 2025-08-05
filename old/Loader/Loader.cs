
namespace DocNET;

using System.IO;
using System.Xml;

public static class Loader
{
	#region Public Methods
	
	/// <summary>Loads the current environment, typically found within the <c>*.csproj</c>.</summary>
	/// <param name="projectName">The name of the project to locate the <c>.csproj</c></param>
	/// <returns>The current environment of the project</returns>
	public static ProjectEnvironment LoadCurrentEnvironment(string projectName)
	{
		ProjectEnvironment environment = new ProjectEnvironment();
		string csprojFilePath = Path.Combine(Settings.CWD, $"{projectName}.csproj");
		XmlDocument document = new XmlDocument();
		
		
		document.Load(csprojFilePath);
		environment.ProjectName = projectName;
		
		return environment;
	}
	
	#endregion // Public Methods
}
