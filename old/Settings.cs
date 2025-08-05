
namespace DocNET;

public static class Settings
{
	#region Properties
	
	public static string CWD { get; set; }
	
	static Settings()
	{
		CWD = System.Environment.CurrentDirectory;
	}
	
	#endregion // Properties
}
