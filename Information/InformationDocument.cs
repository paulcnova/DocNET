
namespace DocNET.Information;

using DocNET.Inspections;

using System.Collections.Generic;
using System.Xml;

public sealed class InformationDocument
{
	#region Properties
	
	public Dictionary<string, InformationElement> Contents { get; private set; } = new Dictionary<string, InformationElement>();
	
	public InformationDocument(string xmlFile)
	{
		XmlDocument document = Load(xmlFile);
		
		foreach(XmlElement member in document.GetElementsByTagName("member"))
		{
			InformationElement element = new InformationElement(member);
			
			this.Contents.Add(member.GetAttribute("name"), element);
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static XmlDocument Load(string xmlFile)
	{
		XmlDocument document = new XmlDocument();
		
		document.Load(xmlFile);
		
		return document;
	}
	
	public InformationElement Find(IXmlMember member) => this.Find(member.GetXmlNameID());
	public InformationElement Find(string id)
		=> this.Contents.TryGetValue(id, out InformationElement elem)
			? elem
			: null;
	
	#endregion // Public Methods
}
