
namespace DocNET;

using DocNET.Information;
using DocNET.Inspections;
using DocNET.Utilities;

using System.Collections.Generic;
using System.IO;

/// <summary>A static class where the program starts from</summary>
public static class Program
{
	#region Public Methods
	
	public static void Main(string[] args)
	{
		for(int i = 0; i < args.Length; ++i)
		{
			switch(args[i].ToLower())
			{
				default: Settings.ProjectName = args[i]; break;
				case "-h": case "--help": Utility.DisplayHelp(); return;
				case "--list-templates": Utility.DisplayTemplates(); return;
				case "--list-projects": Utility.DisplayProjects(); return;
				case "-d": case "--directory": Settings.CWD = args[++i]; break;
			}
		}
		
		if(string.IsNullOrEmpty(Settings.ProjectName))
		{
			List<string> projectList = CSProjUtility.GetProjectsList();
			
			if(projectList.Count == 0)
			{
				System.Console.WriteLine("NO PROJECT FOUND!");
				return;
			}
			
			Settings.ProjectName = projectList[0];
		}
		
		CSProjUtility.ToggleCommentDocumentation(Settings.ProjectName, true);
		CSProjUtility.CompileProject();
		CSProjUtility.ToggleCommentDocumentation(Settings.ProjectName, false);
		
		string binPath = CSProjUtility.GetGeneratedBinDirectory(Settings.ProjectName);
		string[] assemblies = FileUtility.GetAllBinaries(binPath);
		TypeList list = TypeList.Create(Settings.IgnorePrivate, assemblies);
		string xmlFile = $"{binPath}/{Settings.ProjectName}.xml";
		List<string> types = list.Types[$"{Settings.ProjectName}.dll"];
		
		
		foreach(string type in types)
		{
			TypeInfo info = new TypeInfo(type, assemblies, xmlFile, Settings.IgnorePrivate);
			string savePath = Path.Combine(Settings.Output, $"{type}.xml");
			
			Settings.UtilitySet.ProcessType(savePath, info);
			System.Console.WriteLine(type);
		}
	}
	
	// /// <summary>The starting point for the program</summary>
	// /// <param name="args">The list of arguments put in by the user</param>
	// public static void Main(string[] args)
	// {
	// 	bool isHelp = false;
	// 	bool listTemplates = false;
	// 	bool listTypes = false;
	// 	bool inspectSpecific = false;
	// 	string assemblies = "";
	// 	string inspect = "";
	// 	string output = "default";
	// 	string template = "html";
		
	// 	for(int i = 0; i < args.Length; ++i)
	// 	{
	// 		System.Console.WriteLine(args[i]);
	// 		switch(args[i].ToLower())
	// 		{
	// 			case "-h": case "--help": isHelp = true; break;
	// 			case "-o": case "--out": output = args[++i]; break;
	// 			case "-t": case "--template": template = args[++i]; break;
	// 			case "--list-types": listTypes = true; break;
	// 			case "--list-templates": listTemplates = true; break;
	// 			case "-a": case "--assemblies": assemblies = args[++i]; break;
	// 			case "-i": case "--inspect": inspectSpecific = true; inspect = args[++i]; break;
	// 		}
	// 	}
		
	// 	CSProjUtility.ToggleCommentDocumentation(CSProjUtility.GetProjectsList()[0], true);
		
	// 	// TODO: Tailor this to the .csproj
	// 	if(string.IsNullOrEmpty(assemblies))
	// 	{
	// 		assemblies = string.Join(",", System.IO.Directory.GetFiles($"{System.Environment.CurrentDirectory}/bin/Debug/net8.0", "*.dll"));
	// 	}
	// 	inspect = inspect.Replace('-', '`');
	// 	System.Console.WriteLine(inspect);
		
	// 	if(isHelp)
	// 	{
	// 		DisplayHelp();
	// 		return;
	// 	}
	// 	if(listTemplates)
	// 	{
	// 		ListTemplates();
	// 		return;
	// 	}
	// 	if(listTypes)
	// 	{
	// 		ListTypes(assemblies.Split(","));
	// 		return;
	// 	}
	// 	if(inspectSpecific)
	// 	{
	// 		System.Console.WriteLine(assemblies);
	// 		InspectSpecific(inspect, assemblies.Split(","));
	// 		return;
	// 	}
		
	// 	System.Console.WriteLine($"Output: {output}; Template: {template}");
	// }
	
	// public static void InspectSpecific(string typePath, string[] assemblies)
	// {
	// 	TypeInspection data = TypeInspection.Search(typePath, assemblies);
		
	// 	if(data == null) { System.Console.WriteLine($"Type [{typePath}] does not exist within any of the listed assemblies:\n\t{string.Join("\n\t", assemblies)}"); }
		
	// 	System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(data));
	// 	System.Console.WriteLine();
	// 	TypeInfo info = new TypeInfo(typePath, assemblies, $"{System.Environment.CurrentDirectory}/bin/Debug/net8.0/DocNET.xml", false);
	// }
	
	// public static void ListTypes(string[] assemblies)
	// {
	// 	TypeList list = TypeList.Create(true, assemblies);
		
	// 	foreach(var kv in list.Types)
	// 	{
	// 		System.Console.WriteLine($"{kv.Key}:\n\t{string.Join("\n\t", kv.Value)}");
	// 	}
	// }
	
	// /// <summary>Gets the list of templates available for DocNET</summary>
	// /// <returns>Returns a list of strings that name each template available</returns>
	// public static List<string> GetTemplates()
	// {
	// 	// TODO: Get user created templates from %APPDATA%
	// 	List<string> templates = new List<string>()
	// 	{
	// 		"html",
	// 		"react",
	// 		"angular",
	// 		"vue",
	// 		"godot"
	// 	};
		
	// 	return templates;
	// }
	
	// /// <summary>Lists the templates onto the logger</summary>
	// public static void ListTemplates()
	// {
	// 	List<string> templates = GetTemplates();
		
	// 	System.Console.WriteLine("Templates:");
	// 	foreach(string template in templates)
	// 	{
	// 		System.Console.WriteLine($"    {template}");
	// 	}
	// }
	
	// /// <summary>Displays the help menu onto the logger</summary>
	// public static void DisplayHelp()
	// {
	// 	System.Console.WriteLine("Use: DocNET [options] [arguments]");
	// 	System.Console.WriteLine("Options:");
	// 	System.Console.WriteLine("--help (-h)                               Displays the help menu.");
	// 	System.Console.WriteLine("--out <output_file> (-o)                  The file to be outputted.");
	// 	System.Console.WriteLine("--template <template_name> (-t)           The template to convert the documents into.");
	// 	System.Console.WriteLine("--inspect <class_name> (-i)               Inspects the given class name.");
	// 	System.Console.WriteLine("--list-types                              Lists all the types found within the project.");
	// 	System.Console.WriteLine("--list-templates                          Lists all the templates available.");
	// 	System.Console.WriteLine("--assemblies <assembly1,assembly2> (-a)   Provides all the assemblies to be used to inspect. If left empty, it will search the .csproj file.");
	// }
	
	#endregion // Public Methods
}
