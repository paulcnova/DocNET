
namespace DocNET.Utilities;

using System.IO;

/// <summary>A utility class that helps with any IO file issues</summary>
/// <description>This is a more detailed description of the file utility, can be used with <see cref="CSProjUtility"/></description>
/// <tutorial>
/// <link title="How to Use File">https://www.youtube.com/</link>
/// </tutorial>
public static class FileUtility
{
	#region Public Methods
	
	public static string[] GetAllBinaries(string path)
	{
		return Directory.GetFiles(path, "*.dll");
	}
	
	#endregion // Public Methods
}
