
namespace DocNET.Utilities;

using DocNET.Information;

using System.Collections.Generic;
using System.Xml;

/// <summary>A default utility set used by DocNET, utility set is meant for static html</summary>
public class XmlUtilitySet : IUtilitySet
{
	#region Public Methods
	
	/// <inheritdoc/>
	public void ProcessType(string fileName, TypeInfo info)
	{
		int index = System.Math.Max(fileName.LastIndexOf('/'), fileName.LastIndexOf('\\'));
		
		Utility.EnsurePath(fileName.Substring(0, index));
		
		XmlDocument document = new XmlDocument();
		XmlElement root = document.QuickCreate("documentation", new XmlElement[] {
			document.QuickCreate("header", new XmlElement[] {
				document.QuickCreate("assembly", content: $"{info.Inspection.AssemblyName}.dll"),
				document.QuickCreate("object-type", content: info.Inspection.ObjectType),
				document.QuickCreate("name", content: info.Inspection.Info.Name),
				document.QuickCreate("namespace", content: info.Inspection.Info.NamespaceName),
				document.QuickCreate("full-declaration", content: info.Inspection.FullDeclaration),
				document.QuickCreate("declaration", content: info.Inspection.Declaration),
			}),
			document.QuickCreate("methods", this.ProcessMethods(document, info))
		},
		new (string, string)[] {
			("type", info.Inspection.Info.UnlocalizedName)
		});
		
		if(info.Inspection.BaseType != null && !string.IsNullOrEmpty(info.Inspection.BaseType.FullName))
		{
			root["header"].AppendChild(document.QuickCreate("base-type", content: info.Inspection.BaseType.FullName));
		}
		if(info.Inspection.HasDeclaringType)
		{
			root["header"].AppendChild(document.QuickCreate("declaring-type", content: info.Inspection.DeclaringType.FullName));
		}
		
		document.AppendChild(root);
		document.Save(fileName);
	}
	
	public XmlElement[] ProcessMethods(XmlDocument document, TypeInfo info)
	{
		List<XmlElement> elements = new List<XmlElement>();
		
		foreach(MethodInfo method in info.Methods)
		{
			elements.Add(document.QuickCreate("method", new XmlElement[] {
				
			}, new (string, string)[] {
				("name", method.Inspection.Name)
			}));
		}
		
		return elements.ToArray();
	}
	
	// /// <inheritdoc/>
	// public string CreateSystemLink(string typePath, string linkName)
	// 	=> $@"<a href=""https://learn.microsoft.com/en-us/dotnet/api/{typePath}"">{linkName}</a>";
	
	// /// <inheritdoc/>
	// public string CreateInternalLink(string typePath, string linkName)
	// 	=> $@"<a href=""/{typePath.Replace('.', '/')}"">{linkName}</a>";
	
	#endregion // Public Methods
}
