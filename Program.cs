
namespace DocNET;

using DocNET.Inspections;

using System.Collections.Generic;
using System.IO;

/// <summary>A static class where the program starts from</summary>
public static class Program
{
	#region Public Methods
	
	/// <summary>The starting point for the program</summary>
	/// <param name="args">The list of arguments put in by the user</param>
	public static void Main(string[] args)
	{
		bool isHelp = false;
		bool listTemplates = false;
		bool listTypes = false;
		bool inspectSpecific = false;
		string assemblies = "";
		string inspect = "";
		string output = "default";
		string template = "html";
		
		for(int i = 0; i < args.Length; ++i)
		{
			System.Console.WriteLine(args[i]);
			switch(args[i].ToLower())
			{
				case "-h": case "--help": isHelp = true; break;
				case "-o": case "--out": output = args[++i]; break;
				case "-t": case "--template": template = args[++i]; break;
				case "--list-types": listTypes = true; break;
				case "--list-templates": listTemplates = true; break;
				case "-a": case "--assemblies": assemblies = args[++i]; break;
				case "-i": case "--inspect": inspectSpecific = true; inspect = args[++i]; break;
			}
		}
		
		// TODO: Tailor this to the .csproj
		if(string.IsNullOrEmpty(assemblies))
		{
			assemblies = string.Join(",", System.IO.Directory.GetFiles($"{System.Environment.CurrentDirectory}/bin/Debug/net8.0", "*.dll"));
		}
		
		if(isHelp)
		{
			DisplayHelp();
			return;
		}
		if(listTemplates)
		{
			ListTemplates();
			return;
		}
		if(listTypes)
		{
			ListTypes(assemblies.Split(","));
			return;
		}
		if(inspectSpecific)
		{
			System.Console.WriteLine(assemblies);
			InspectSpecific(inspect, assemblies.Split(","));
			return;
		}
		
		System.Console.WriteLine($"Output: {output}; Template: {template}");
	}
	
	public static void InspectSpecific(string typePath, string[] assemblies)
	{
		TypeInspection data = TypeInspection.Search(typePath, assemblies);
		
		if(data == null) { System.Console.WriteLine($"Type [{typePath}] does not exist within any of the listed assemblies:\n\t{string.Join("\n\t", assemblies)}"); }
		
		System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(data));
	}
	
	public static void ListTypes(string[] assemblies)
	{
		TypeList list = DocNET.Inspections.TypeList.Create(true, assemblies);
		
		foreach(var kv in list.Types)
		{
			System.Console.WriteLine($"{kv.Key}:\n\t{string.Join("\n\t", kv.Value)}");
		}
	}
	
	/// <summary>Gets the list of templates available for DocNET</summary>
	/// <returns>Returns a list of strings that name each template available</returns>
	public static List<string> GetTemplates()
	{
		// TODO: Get user created templates from %APPDATA%
		List<string> templates = new List<string>()
		{
			"html",
			"react",
			"angular",
			"vue",
			"godot"
		};
		
		return templates;
	}
	
	/// <summary>Lists the templates onto the logger</summary>
	public static void ListTemplates()
	{
		List<string> templates = GetTemplates();
		
		System.Console.WriteLine("Templates:");
		foreach(string template in templates)
		{
			System.Console.WriteLine($"    {template}");
		}
	}
	
	/// <summary>Displays the help menu onto the logger</summary>
	public static void DisplayHelp()
	{
		System.Console.WriteLine("Use: DocNET [options] [arguments]");
		System.Console.WriteLine("Options:");
		System.Console.WriteLine("--help (-h)                               Displays the help menu.");
		System.Console.WriteLine("--out <output_file> (-o)                  The file to be outputted.");
		System.Console.WriteLine("--template <template_name> (-t)           The template to convert the documents into.");
		System.Console.WriteLine("--inspect <class_name> (-i)               Inspects the given class name.");
		System.Console.WriteLine("--list-types                              Lists all the types found within the project.");
		System.Console.WriteLine("--list-templates                          Lists all the templates available.");
		System.Console.WriteLine("--assemblies <assembly1,assembly2> (-a)   Provides all the assemblies to be used to inspect. If left empty, it will search the .csproj file.");
	}
	#endregion // Public Methods
}
