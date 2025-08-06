
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
	
	public string FlattenSummary(NodeFlattener flattener) => this.Flatten(XCD_Summary, flattener);
	public string Flatten(string type, NodeFlattener flattener)
	{
		switch(type)
		{
			default: return flattener.Flatten<XcdContentNode>(null);
			case XCD_Summary: return flattener.Flatten(this.Summary);
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
