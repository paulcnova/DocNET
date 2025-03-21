
namespace DocNET.Utilities;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

public static class CSProjUtility
{
	#region Public Methods
	
	public static string GetGeneratedBinDirectory(string projectName)
	{
		XmlDocument document = GetCSProject(projectName);
		string path = $"{Settings.CWD}/bin";
		string config = "Debug";
		string framework = "net8.0";
		
		// TODO: Find these factors in the document
		
		return Path.Combine(path, config, framework);
	}
	
	public static void CompileProject()
	{
		Process compiler = new Process()
		{
			StartInfo = new ProcessStartInfo()
			{
				FileName = "dotnet",
				Arguments = $"build {Settings.CSProjectFile}",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false,
			},
		};
		
		System.Console.WriteLine("Compiling...");
		compiler.Start();
		compiler.WaitForExit();
		System.Console.WriteLine(compiler.StandardOutput.ReadToEnd());
	}
	
	public static List<string> GetProjectsList()
	{
		string path = $"{Settings.CWD}/";
		List<string> projectNames = new List<string>();
		
		foreach(string file in Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories))
		{
			projectNames.Add(file.Replace(path, "").Replace(".csproj", ""));
		}
		
		
		return projectNames;
	}
	
	public static XmlDocument GetCSProject(string projectName)
	{
		XmlDocument document = new XmlDocument();
		
		document.Load($"{Settings.CWD}/{projectName}.csproj");
		
		return document;
	}
	
	public static XmlDocument Save(XmlDocument document, string projectName)
	{
		string path = $"{Settings.CWD}/{projectName}.csproj";
		
		document.Save(path);
		
		return document;
	}
	
	public static void ToggleCommentDocumentation(string projectName, bool enable)
	{
		XmlDocument document = GetCSProject(projectName);
		XmlElement propertyGroup = document["Project"]["PropertyGroup"];
		XmlElement gdNode = propertyGroup["GenerateDocumentationFile"];
		
		if(enable && gdNode == null)
		{
			XmlElement gd = document.CreateElement("GenerateDocumentationFile");
			gd.AppendChild(document.CreateTextNode("true"));
			
			propertyGroup.AppendChild(gd);
		}
		else if(!enable && gdNode != null)
		{
			propertyGroup.RemoveChild(gdNode);
		}
		
		Save(document, projectName);
	}
	
	#endregion // Public Methods
}
