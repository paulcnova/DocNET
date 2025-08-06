
namespace DocNET.Information;

public class XmlTextNode : XmlContentNode
{
	#region Properties
	
	public string Text { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override string Flatten()
	{
		return this.Text;
	}
	
	#endregion // Public Methods
}
