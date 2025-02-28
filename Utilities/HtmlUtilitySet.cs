
namespace DocNET.Utilities;

public class HtmlUtilitySet : IUtilitySet
{
	#region Public Methods
	
	/// <inheritdoc/>
	public string CreateSystemLink(string typePath, string linkName)
		=> $@"<a href=""https://learn.microsoft.com/en-us/dotnet/api/{typePath}"">{linkName}</a>";
	
	/// <inheritdoc/>
	public string CreateInternalLink(string typePath, string linkName)
		=> $@"<a href=""/{typePath.Replace('.', '/')}"">{linkName}</a>";
	
	#endregion // Public Methods
}
