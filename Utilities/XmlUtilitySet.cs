
namespace DocNET.Utilities;

using DocNET.Information;

using System.Xml;

/// <summary>A default utility set used by DocNET, utility set is meant for static html</summary>
public class XmlUtilitySet : IUtilitySet
{
	#region Public Methods
	
	public void ProcessType(string fileName, TypeInfo info)
	{
		Utility.EnsurePath(fileName.Substring(0, fileName.LastIndexOf('/')));
		
		XmlDocument document = new XmlDocument();
		XmlElement root = document.CreateElement("documentation");
		
		root.SetAttribute("type", info.Inspection.Info.UnlocalizedName);
		
		document.AppendChild(root);
		document.Save(fileName);
	}
	
	// /// <inheritdoc/>
	// public string CreateSystemLink(string typePath, string linkName)
	// 	=> $@"<a href=""https://learn.microsoft.com/en-us/dotnet/api/{typePath}"">{linkName}</a>";
	
	// /// <inheritdoc/>
	// public string CreateInternalLink(string typePath, string linkName)
	// 	=> $@"<a href=""/{typePath.Replace('.', '/')}"">{linkName}</a>";
	
	#endregion // Public Methods
}
