
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
		string output = "bin/docs";
		string input = "bin/Debug/net8.0/DocNET.dll";
		string inputXml = "bin/Debug/net8.0/DocNET.xml";
		string template = "xml";
		bool ignorePrivate = true;
		
		for(int i = 0; i < args.Length; ++i)
		{
			switch(args[i].ToLower())
			{
				case "-h": case "--help": isHelp = true; break;
				case "--list-types": listTypes = true; break;
				case "--list-templates": listTemplates = true; break;
				
				// case "-o": case "--out": output = args[++i]; break;
				// case "-t": case "--template": template = args[++i]; break;
				// case "-a": case "--assemblies": assemblies = args[++i]; break;
				// case "-i": case "--inspect": inspectSpecific = true; inspect = args[++i]; break;
			}
		}
		
		// TODO: Tailor this to the .csproj
		if(string.IsNullOrEmpty(assemblies))
		{
			assemblies = string.Join(",", System.IO.Directory.GetFiles($"{System.Environment.CurrentDirectory}/bin/Debug/net8.0", "*.dll"));
		}
		// inspect = inspect.Replace('-', '`');
		// System.Console.WriteLine(inspect);
		
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
			ListTypes(input);
			// ListTypes(assemblies.Split(","));
			return;
		}
		// if(inspectSpecific)
		// {
		// 	System.Console.WriteLine(assemblies);
		// 	InspectSpecific(inspect, assemblies.Split(","));
		// 	return;
		// }
		
		// System.Console.WriteLine($"Output: {output}; Template: {template}");
		
		GenerateDocumentation(ignorePrivate, inputXml, output, template, input.Split(','), assemblies.Split(','));
	}
	
	public static void GenerateDocumentation(bool ignorePrivate, string inputXml, string output, string template, string[] inputs, string[] assemblies)
	{
		Utility.SetTemplate(template);
		
		inputXml = Utility.MakeAbsolute(inputXml);
		output = Utility.MakeAbsolute(output);
		for(int i = 0; i < inputs.Length; ++i)
		{
			inputs[i] = Utility.MakeAbsolute(inputs[i]);
		}
		for(int i = 0; i < assemblies.Length; ++i)
		{
			assemblies[i] = Utility.MakeAbsolute(assemblies[i]);
		}
		
		TypeList list = TypeList.Create(ignorePrivate, inputs);
		
		foreach(KeyValuePair<string, List<string>> kv in list.Types)
		{
			foreach(string typePath in kv.Value)
			{
				TypeInfo info = new TypeInfo(typePath, assemblies, inputXml, ignorePrivate);
				
				if(info.ShouldIgnore) { continue; }
				
				Utility.RenderAndSaveToFile(output, typePath, info);
			}
		}
	}
	
	public static void InspectSpecific(string typePath, string[] assemblies)
	{
		TypeInspection data = TypeInspection.Search(typePath, assemblies);
		
		if(data == null) { System.Console.WriteLine($"Type [{typePath}] does not exist within any of the listed assemblies:\n\t{string.Join("\n\t", assemblies)}"); }
		
		System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(data));
		System.Console.WriteLine();
		TypeInfo info = new TypeInfo(typePath, assemblies, $"{System.Environment.CurrentDirectory}/bin/Debug/net8.0/DocNET.xml", false);
	}
	
	public static void ListTypes(params string[] assemblies)
	{
		TypeList list = TypeList.Create(true, assemblies);
		
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
