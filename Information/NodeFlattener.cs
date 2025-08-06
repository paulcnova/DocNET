
namespace DocNET.Information;

public class NodeFlattener
{
	#region Properties
	
	public SiteMap SiteMap { get; set; }
	
	public NodeFlattener(SiteMap siteMap)
	{
		this.SiteMap = siteMap;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual string GetNoDescription() => "No description.";
	
	public virtual string Flatten<T>(T node) where T : XcdContentNode
	{
		if(node == null) { return this.GetNoDescription(); }
		if(node is XcdTextNode textNode) { return this.FlattenText(textNode); }
		return string.Join("", node.Children.ConvertAll(child => this.Flatten(child)));
	}
	
	public virtual string FlattenText(XcdTextNode node)
	{
		return node.Text;
	}
	
	#endregion // Public Methods
}
