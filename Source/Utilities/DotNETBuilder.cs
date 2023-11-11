
namespace Taco.DocNET.Utilities;

using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>A static class that builds the dotnet project</summary>
public static class DotNETBuilder
{
	#region Public Methods
	
	/// <summary>Injects, builds, and restores the projects</summary>
	/// <param name="writeToConsole">Set to true to write the build command to console</param>
	/// <returns>Returns the list of dll-xml pairings generated from the build</returns>
	public static List<DllXmlPair> InjectBuildRestore(bool writeToConsole = true)
	{
		string[] projects = GetCSProjects();
		List<string> correctProjectTexts = new List<string>();
		List<DllXmlPair> docDllPairs = new List<DllXmlPair>();
		
		foreach(string project in projects)
		{
			correctProjectTexts.Add(Inject(project, docDllPairs));
		}
		
		bool isComplete = BuildProject(docDllPairs, writeToConsole);
		
		for(int i = 0; i < projects.Length; ++i)
		{
			Restore(projects[i], correctProjectTexts[i]);
		}
		
		return isComplete ? docDllPairs : new List<DllXmlPair>();
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Injects the xml code with the generate documentation code needed for building documentation</summary>
	/// <param name="filePath">The path to the xml file</param>
	/// <param name="pairs">The list of dll-xml pairs to add xml paths to</param>
	/// <returns>Returns the content found on the xml file before it was injected</returns>
	private static string Inject(string filePath, List<DllXmlPair> pairs)
	{
		string name = filePath.Substring(filePath.Replace('\\', '/').LastIndexOf('/') + 1).Replace(".csproj", "");
		string oldContent = File.ReadAllText(filePath);
		XmlDocument document = new XmlDocument();
		
		document.Load(filePath);
		
		foreach(XmlNode node in document.GetElementsByTagName("GenerateDocumentationFile"))
		{
			node.InnerText = "true";
		}
		
		XmlNodeList nodesToDelete = document.GetElementsByTagName("DocumentationFile");
		
		for(int i = 0; i < nodesToDelete.Count; ++i)
		{
			nodesToDelete[i].ParentNode.RemoveChild(nodesToDelete[i]);
		}
		
		XmlNode propertyGroup = document.CreateNode(XmlNodeType.Element, "PropertyGroup", null);
		XmlNode generate = document.CreateNode(XmlNodeType.Element, "GenerateDocumentationFile", null);
		XmlNode documentFile = document.CreateNode(XmlNodeType.Element, "DocumentationFile", null);
		string docFilePath = Path.Combine(Utility.TempPath, $"{name}.xml");
		
		pairs.Add(new DllXmlPair(docFilePath));
		generate.InnerText = "true";
		documentFile.InnerText = docFilePath;
		
		propertyGroup.AppendChild(generate);
		propertyGroup.AppendChild(documentFile);
		
		document.FirstChild.AppendChild(propertyGroup);
		
		// Append target message node
		XmlNode target = document.CreateNode(XmlNodeType.Element, "Target", null);
		XmlAttribute targetName = document.CreateAttribute("Name");
		XmlAttribute afterBuild = document.CreateAttribute("AfterTargets");
		
		targetName.Value = "TEMP_GetDLLPath";
		afterBuild.Value = "Build";
		target.Attributes.Append(targetName);
		target.Attributes.Append(afterBuild);
		
		XmlNode message = document.CreateNode(XmlNodeType.Element, "Message", null);
		XmlAttribute importance = document.CreateAttribute("Importance");
		XmlAttribute text = document.CreateAttribute("Text");
		
		importance.Value = "High";
		text.Value = @"==> $(MSBuildProjectDirectory)\$(OutputPath)$(AssemblyName).dll|$(DocumentationFile)";
		message.Attributes.Append(importance);
		message.Attributes.Append(text);
		
		target.AppendChild(message);
		document.FirstChild.AppendChild(target);
		
		document.Save(filePath);
		
		return oldContent;
	}
	
	/// <summary>Restores the injected xml code to what it was before</summary>
	/// <param name="filePath">The path to the xml file</param>
	/// <param name="xml">The xml code to restore with</param>
	private static void Restore(string filePath, string xml) => Utility.Write(filePath, xml);
	
	/// <summary>Builds the project, filling in the pairs list with the location of the compiled .dlls</summary>
	/// <param name="pairs">The list of dll-xml pairs used to pair the documentation with the actual code</param>
	/// <param name="writeToConsole">Set to true to write the output of `dotnet build` onto the console</param>
	/// <returns>Returns true if the build was successful</returns>
	private static bool BuildProject(List<DllXmlPair> pairs, bool writeToConsole = true)
	{
		Process process = new Process();
		ProcessStartInfo processInfo = new ProcessStartInfo("dotnet")
		{
			CreateNoWindow = false,
			RedirectStandardOutput = writeToConsole,
			RedirectStandardError = writeToConsole,
			RedirectStandardInput = writeToConsole,
		};
		
		// TODO: Remove this, this is for testing purposes
		processInfo.WorkingDirectory = @"E:\_Projects\Personal\C#\sharp-checker";
		processInfo.UseShellExecute = false;
		processInfo.ArgumentList.Add("build");
		process.StartInfo = processInfo;
		process.OutputDataReceived += (_, content) => {
			if(writeToConsole) { System.Console.WriteLine(content.Data); }
			
			if(string.IsNullOrEmpty(content.Data)) { return; }
			
			if(content.Data.Trim().StartsWith("==> "))
			{
				string text = content.Data.Trim().Substring(4);
				string[] dirName = text.Split('|');
				
				pairs.Find(pair => pair.XmlAbsolutePath == dirName[1].Trim()).DllAbsolutePath = dirName[0].Trim();
			}
		};
		if(writeToConsole)
		{
			process.ErrorDataReceived += (_, content) => System.Console.WriteLine(content.Data);
		}
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();
		
		return process.ExitCode == 0;
	}
	
	/// <summary>Gets the list of C# project files</summary>
	/// <returns>Returns the list of the absolute paths to the C# project files</returns>
	private static string[] GetCSProjects()
	{
		return Directory.GetFiles(
			// TODO: Remove this once done testing and uncomment the CurrentDirectory line
			@"E:\_Projects\Personal\C#\sharp-checker",
			// System.Environment.CurrentDirectory,
			"*.csproj",
			SearchOption.AllDirectories
		);
	}
	
	#endregion // Private Methods
}
