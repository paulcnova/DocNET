
namespace DocNET.Utilities;

using System.IO;

public static class FileUtility
{
	#region Public Methods
	
	public static string[] GetAllBinaries(string path)
	{
		return Directory.GetFiles(path, "*.dll");
	}
	
	#endregion // Public Methods
}
