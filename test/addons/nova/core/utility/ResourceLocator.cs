
namespace Nova;

using Godot;
using Godot.Collections;

using System.Text.RegularExpressions;

/// <summary>A utility class that help find resources in bulk.</summary>
public static class ResourceLocator
{
	#region Public Methods
	
	/// <summary>Checks to see if the path exists and has any files.</summary>
	/// <param name="path">The path to check.</param>
	/// <returns>Returns true if the path exists and has any files.</returns>
	public static bool HasFiles(string path)
	{
		if(!DirAccess.DirExistsAbsolute(path)) { return false; }
		
		Array<string> files = GetFiles(path, false);
		
		return files.Count > 0;
	}
	
	/// <summary>Gets all the files from the given path.</summary>
	/// <param name="path">The path to get the files from.</param>
	/// <param name="recursive">Set to true to search sub-folders recursively. (Defaults to true).</param>
	/// <returns>Returns an array of file locations.</returns>
	public static Array<string> GetFiles(string path, bool recursive = true)
	{
		if(path.EndsWith('/') || path.EndsWith('\\'))
		{
			path = path.Substring(0, path.Length - 1);
		}
		
		Array<string> files = new Array<string>();
		string[] foundFiles = DirAccess.GetFilesAt(path);
		
		foreach(string file in foundFiles)
		{
			files.Add($"{path}/{file}");
		}
		
		if(recursive)
		{
			string[] foundDirectories = DirAccess.GetDirectoriesAt(path);
			
			foreach(string subDir in foundDirectories)
			{
				Array<string> nestedFiles = GetFiles($"{path}/{subDir}");
				
				foreach(string file in nestedFiles)
				{
					files.Add($"{file}");
				}
			}
		}
		
		return files;
	}
	
	/// <summary>Gets all the files from the given path and has a specific suffix.</summary>
	/// <param name="path">The path to get the files from.</param>
	/// <param name="suffix">The suffix (excluding the file extension) to search with. (Regex is allowed).</param>
	/// <param name="recursive">Set to true to search sub-folders recursively. (Defaults to true).</param>
	/// <returns>Returns an array of file locations.</returns>
	public static Array<string> GetFiles(string path, string suffix, bool recursive = true)
	{
		Array<string> files = GetFiles(path, recursive);
		Array<string> results = new Array<string>();
		
		foreach(string file in files)
		{
			if(Regex.IsMatch(file, $@"\.{suffix}\.t?res(\.remap)?$"))
			{
				results.Add(file);
			}
		}
		
		return results;
	}
	
	/// <summary>Loads in all the resources from the given path.</summary>
	/// <param name="path">The path to get the resources from.</param>
	/// <param name="recursive">Set to true to search sub-folders recursively. (Defaults to true).</param>
	/// <typeparam name="T">The type of resource to load in. If a resource is found but doesn't inherit from this type it won't be included into the resulting array.</typeparam>
	/// <returns>Returns an array of loaded resources.</returns>
	public static Array<T> LoadAll<[MustBeVariant] T>(string path, bool recursive = true) where T : Resource
	{
		Array<T> resources = new Array<T>();
		Array<string> files = GetFiles(path, recursive);
		
		foreach(string file in files)
		{
			string correctedFilename = CorrectFileName(file);
		
			if(ResourceLoader.Exists(correctedFilename))
			{
				Resource resource = ResourceLoader.Load(correctedFilename);
				
				if(resource != null && resource.GetType().IsAssignableTo(typeof(T)))
				{
					resources.Add(resource as T);
				}
			}
		}
		
		return resources;
	}
	
	/// <summary>Loads in all the resources from the given path and has a specific suffix.</summary>
	/// <param name="path">The path to get the files from.</param>
	/// <param name="suffix">The suffix (excluding the file extension) to search with. (Regex is allowed).</param>
	/// <param name="recursive">Set to true to search sub-folders recursively. (Defaults to true).</param>
	/// <typeparam name="T">The type of resource to load in. If a resource is found but doesn't inherit from this type it won't be included into the resulting array.</typeparam>
	/// <returns>Returns an array of loaded resources.</returns>
	public static Array<T> LoadAll<[MustBeVariant] T>(string path, string suffix, bool recursive = true) where T : Resource
	{
		Array<T> resources = new Array<T>();
		Array<string> files = GetFiles(path, suffix, recursive);
		
		foreach(string file in files)
		{
			string correctedFilename = CorrectFileName(file);
			
			if(ResourceLoader.Exists(correctedFilename))
			{
				Resource resource = ResourceLoader.Load(correctedFilename);
				
				if(resource != null && resource.GetType().IsAssignableTo(typeof(T)))
				{
					resources.Add(resource as T);
				}
			}
		}
		
		return resources;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Corrects the filename to properly load the resource.</summary>
	/// <param name="filename">The filename to correct.</param>
	/// <returns>Returns the corrected filename.</returns>
	/// <remarks>When the project is exported, all resources seem to take on a .remap file extension. This method is here to take care of that case.</remarks>
	private static string CorrectFileName(string filename) => filename.Replace(".remap", "");
	
	#endregion // Private Methods
}
