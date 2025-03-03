
namespace DocNET;

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
		string output = "default";
		string template = "html";
		
		for(int i = 0; i < args.Length; ++i)
		{
			switch(args[i].ToLower())
			{
				case "-h": case "--help": isHelp = true; break;
				case "-o": case "--out": output = args[++i]; break;
				case "-t": case "--template": template = args[++i]; break;
				case "--list-templates": listTemplates = true; break;
			}
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
		
		System.Console.WriteLine($"Output: {output}; Template: {template}");
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
		System.Console.WriteLine("--help (-h)                        Displays the help menu.");
		System.Console.WriteLine("--out <output_file> (-o)           The file to be outputted.");
		System.Console.WriteLine("--template <template_name> (-t)    The template to convert the documents into.");
		System.Console.WriteLine("--list-templates                   Lists all the templates available.");
	}
	#endregion // Public Methods
}
