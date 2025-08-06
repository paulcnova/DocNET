
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
			
			System.Console.WriteLine(member.GetAttribute("name"));
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
	
	public InformationElement Find(IXmlMember member)
	{
		string xcdID = member.GetXmlNameID();
		
		// System.Console.WriteLine(xcdID + "----"+this.Contents.ContainsKey(xcdID));
		if(this.Contents.ContainsKey(xcdID))
		{
			return this.Contents[xcdID];
		}
		
		return null;
		// return new InformationElement((new XmlDocument()).CreateElement("NA"));
	}
	
	#endregion // Public Methods
}
