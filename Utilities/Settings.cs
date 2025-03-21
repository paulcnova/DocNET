
namespace DocNET.Utilities;

public static class Settings
{
	#region Properties
	
	public static bool IgnorePrivate { get; set; } = false;
	public static string CWD { get; set; } = System.Environment.CurrentDirectory;
	public static string Output { get; set; } = $"{System.Environment.CurrentDirectory}/bin/docs";
	public static string ProjectName { get; set; }
	public static IUtilitySet UtilitySet { get; set; } = new XmlUtilitySet();
	
	public static string CSProjectFile => $"{Settings.CWD}/{ProjectName}.csproj";
	
	#endregion // Properties
}
