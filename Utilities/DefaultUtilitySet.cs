
namespace DocNET.Utilities;

using DocNET.Information;

using System.Xml;

/// <summary>A default utility set used by DocNET, this utility set generates nothing</summary>
public class DefaultUtilitySet : IUtilitySet
{
	#region Public Methods
	
	/// <inheritdoc/>
	public string CreateSystemLink(string typePath, string linkName)
		=> $@"<a href=""https://learn.microsoft.com/en-us/dotnet/api/{typePath}"">{linkName}</a>";
	
	/// <inheritdoc/>
	public string CreateInternalLink(string typePath, string linkName)
		=> $@"<a href=""/{typePath.Replace('.', '/')}"">{linkName}</a>";
	
	public void RenderAndSaveToFile(string output, string typePath, TypeInfo info)
	{
		XmlDocument document = new XmlDocument();
		XmlElement root = document.CreateElement("documentation");
		XmlElement head = document.CreateElement("head");
		
		head.AppendChild(Utility.CreateElementWithText(document, "declaration", info.Inspection.FullDeclaration));
		
		root.AppendChild(head);
		document.AppendChild(root);
		
		Utility.EnsurePath(output);
		document.Save($"{output}/{typePath.Replace('`', '-').Replace('/', '.')}.xml");
	}
	
	#endregion // Public Methods
}
