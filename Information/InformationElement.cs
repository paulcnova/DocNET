
namespace DocNET.Information;

using System.Collections.Generic;
using System.Xml;

public sealed class InformationElement
{
	#region Properties
	
	public Dictionary<string, XmlContentNode> Contents { get; private set; } = new Dictionary<string, XmlContentNode>();
	
	public XmlContentNode Summary => this.Contents.ContainsKey("summary") ? this.Contents["summary"] : null;
	
	public delegate string FlattenCallback(string type, XmlContentNode node);
	
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
				// System.Console.WriteLine(child.Name);
				this.Contents.Add(child.Name, this.ExtractContent(child));
			}
		}
		// TODO: Contents becomes zero
		// System.Console.WriteLine(this.Contents.Count);
		// System.Console.WriteLine(string.Join(", ", this.Contents.Keys));
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public string Flatten(string type, Inspections.TypeInspection insp, XmlContentNode node, FlattenCallback callback = null)
	{
		if(callback != null) { return callback(type, node); }
		
		switch(type)
		{
			case "summary":
				XmlContentNode summary = this.Summary;
				
				if(summary == null) { break; }
				
				return summary.Flatten();
		}
		
		return "No description.";
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private XmlContentNode ExtractContent(XmlNode node)
	{
		if(node is XmlElement elem)
		{
			XmlContentNode content = new XmlContentNode();
			
			// System.Console.WriteLine("ELEMENT:"+elem.Name);
			// foreach(XmlAttribute attr in elem.Attributes)
			// {
			// 	System.Console.WriteLine(attr.Name+"::"+attr.Value);
			// }
			
			foreach(XmlNode child in elem.ChildNodes)
			{
				content.Children.Add(this.ExtractContent(child));
			}
			return content;
		}
		if(node is XmlText txt)
		{
			XmlTextNode content = new XmlTextNode();
			
			// System.Console.WriteLine(txt.InnerText);
			content.Text = txt.InnerText;
			return content;
		}
		
		return null;
	}
	
	#endregion // Private Methods
}
