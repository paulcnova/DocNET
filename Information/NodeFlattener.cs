
namespace DocNET.Information;

public class NodeFlattener
{
	#region Properties
	
	public InformationDocument Document { get; set; }
	public SiteMap SiteMap { get; set; }
	
	public NodeFlattener(InformationDocument document, SiteMap siteMap)
	{
		this.Document = document;
		this.SiteMap = siteMap;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual string GetNoDescription() => "No description.";
	
	public virtual string Stringify<T>(T node) where T : XcdContentNode
	{
		if(node == null) { return this.GetNoDescription(); }
		if(node is XcdTextNode textNode) { return this.StringifyText(textNode); }
		return string.Join("", node.Children.ConvertAll(child => this.Stringify(child)));
	}
	
	public virtual string StringifyText(XcdTextNode node)
	{
		return node.Text;
	}
	
	#endregion // Public Methods
}
