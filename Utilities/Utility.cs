
namespace DocNET.Utilities;

using System.Collections.Generic;
using System.IO;
using System.Xml;

public static class Utility
{
	#region Public Methods
	
	/// <summary>Gets the absolute path this program's place in the user's `APPDATA` folder</summary>
	public static string AppDataPath => Path.Combine(GetAppDataPath(), "DocNET");
	
	/// <summary>Gets the absolute path to the temp folder used by this program</summary>
	public static string TempPath => Path.Combine(AppDataPath, "temp");
	
	/// <summary>Ensures the path by making any directories if they don't exist yet</summary>
	/// <param name="path">The folder path to ensure it's path</param>
	public static void EnsurePath(string path)
	{
		if(!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
	
	/// <summary>Writes to the content, making sure it can override it without error</summary>
	/// <param name="path">The file path to the file</param>
	/// <param name="content">The content to write</param>
	public static void Write(string path, string content)
	{
		EnsurePath(path);
		if(File.Exists(path))
		{
			File.Delete(path);
		}
		
		File.WriteAllText(path, content);
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
	
	public static void DisplayHelp()
	{
		System.Console.WriteLine("Use: DocNET [options] [arguments]");
		System.Console.WriteLine("Information:");
		System.Console.WriteLine("-h, --help                     Displays the help menu.");
		System.Console.WriteLine("--list-projects                Lists all the projects are available.");
		System.Console.WriteLine("--list-templates               Lists all the templates available.");
		System.Console.WriteLine(".csproj Settings:");
		System.Console.WriteLine("-d, --directory <directory>    Sets the directory to look for the .csproj.");
	}
	
	public static void DisplayTemplates()
	{
		System.Console.WriteLine("Templates:");
		foreach(string template in GetTemplates())
		{
			System.Console.WriteLine($"    {template}");
		}
	}
	
	public static void DisplayProjects()
	{
		System.Console.WriteLine("Projects:");
		foreach(string project in CSProjUtility.GetProjectsList())
		{
			System.Console.WriteLine($"    {project}");
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the path to the user's `APPDATA` folder</summary>
	/// <returns>Returns the path to the user's `APPDATA` folder</returns>
	private static string GetAppDataPath()
	{
		return System.Environment.GetEnvironmentVariable("APPDATA");
	}
	
	#endregion // Private Methods
}
