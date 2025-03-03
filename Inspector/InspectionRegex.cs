
namespace DocNET.Inspections;

using System.Text.RegularExpressions;

/// <summary>A central class for all the regex used by all the inspection classes</summary>
public static partial class InspectionRegex
{
	#region Properties
	
	[GeneratedRegex(@"(.*)\..*$")]
	public static partial Regex NamespaceName();
	
	[GeneratedRegex(@"[a-zA-Z0-9]+((?:\[,*\])+)")]
	public static partial Regex Array();
	
	[GeneratedRegex(@"([a-zA-Z0-9]+\.)+")]
	public static partial Regex Namespace();
	
	[GeneratedRegex(@",(\w)")]
	public static partial Regex TypeFromCulture();
	
	[GeneratedRegex(@"\u0060+\d+")]
	public static partial Regex GenericNotation();
	
	[GeneratedRegex(@"((?:[a-zA-Z0-9`]+[\.\/]?)*)[\.\/](.*)|([a-zA-Z0-9`]+)")]
	public static partial Regex Type();
	
	[GeneratedRegex(@"(.):((?:[a-zA-Z0-9`]+[\.\/]?)*).*")]
	public static partial Regex Cref();
	
	[GeneratedRegex(@"^[ ]{12}", RegexOptions.Multiline)]
	public static partial Regex Unindent();
	
	#endregion // Properties
}
