
namespace DocNET.Information;

using System.Collections.Generic;

public class XmlContentNode
{
	#region Properties
	
	public List<XmlContentNode> Children { get; private set; } = new List<XmlContentNode>();
	
	#endregion // Properties
}
