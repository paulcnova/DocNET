
namespace DocNET.Information;

using System.Collections.Generic;
using System.Xml;

public sealed class InformationElement
{
	#region Properties
	
	public const string XCD_Summary = "summary";
	
	public Dictionary<string, XcdContentNode> Contents { get; private set; } = new Dictionary<string, XcdContentNode>();
	
	public XcdContentNode Summary => this.Contents.ContainsKey(XCD_Summary) ? this.Contents[XCD_Summary] : null;
	
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
	
	public string StringifySummary(NodeFlattener flattener) => this.Stringify(XCD_Summary, flattener);
	public string Stringify(string type, NodeFlattener flattener)
	{
		switch(type)
		{
			default: return flattener.Stringify<XcdContentNode>(null);
			case XCD_Summary: return flattener.Stringify(this.Summary);
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private XcdContentNode ExtractContent(XmlNode node)
	{
		if(node is XmlElement elem)
		{
			XcdContentNode content = new XcdContentNode();
			
			foreach(XmlNode child in elem.ChildNodes)
			{
				content.Children.Add(this.ExtractContent(child));
			}
			return content;
		}
		else if(node is XmlText txt)
		{
			XcdTextNode content = new XcdTextNode();
			
			content.Text = txt.InnerText;
			return content;
		}
		else
		{
			System.Console.WriteLine(node.GetType());
		}
		
		return null;
	}
	
	#endregion // Private Methods
}
