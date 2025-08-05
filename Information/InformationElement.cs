
namespace DocNET.Information;

using System.Collections.Generic;
using System.Xml;

public sealed class InformationElement
{
	#region Properties
	
	public Dictionary<string, XmlContentNode> Contents { get; private set; } = new Dictionary<string, XmlContentNode>();
	
	public XmlContentNode Summary => this.Contents["summary"];
	
	public InformationElement(XmlElement member)
	{
		foreach(XmlNode child in member.ChildNodes)
		{
			if(this.Contents.ContainsKey(child.Name))
			{
				this.Contents[child.Name] = this.ExtractContent(child);
			}
			else
			{
				this.Contents.Add(child.Name, this.ExtractContent(child));
			}
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static InformationElement Search(string typePath, XmlDocument document)
	{
		foreach(XmlElement elem in document["doc"]["members"])
		{
			if(elem.HasAttribute("name") && elem.GetAttribute("name") == typePath)
			{
				return new InformationElement(elem);
			}
		}
		
		return null;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private XmlContentNode ExtractContent(XmlNode node)
	{
		if(node is XmlElement elem)
		{
			XmlContentNode content = new XmlContentNode();
			
			System.Console.WriteLine("ELEMENT:"+elem.Name);
			foreach(XmlAttribute attr in elem.Attributes)
			{
				System.Console.WriteLine(attr.Name+"::"+attr.Value);
			}
			
			foreach(XmlNode child in elem.ChildNodes)
			{
				content.Children.Add(this.ExtractContent(child));
			}
		}
		if(node is XmlText txt)
		{
			XmlTextNode content = new XmlTextNode();
			
			System.Console.WriteLine(txt.InnerText);
			content.Text = txt.InnerText;
			return content;
		}
		
		return null;
	}
	
	#endregion // Private Methods
}
