
namespace DocNET.Templates.HTML;

using DocNET.Information;
using DocNET.Utilities;

public class HtmlUtilitySet : IUtilitySet
{
	#region Public Methods
	
	public void ProcessType(string fileName, TypeInfo info)
	{
		
	}
	
	// /// <inheritdoc/>
	// public string CreateSystemLink(string typePath, string linkName)
	// 	=> $@"<a href=""https://learn.microsoft.com/en-us/dotnet/api/{typePath}"">{linkName}</a>";
	
	// /// <inheritdoc/>
	// public string CreateInternalLink(string typePath, string linkName)
	// 	=> $@"<a href=""/{typePath.Replace('.', '/')}"">{linkName}</a>";
	
	#endregion // Public Methods
}
