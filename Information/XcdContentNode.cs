
namespace DocNET.Information;

using System.Collections.Generic;
using System.Linq;

public class XcdContentNode
{
	#region Properties
	
	public List<XcdContentNode> Children { get; private set; } = new List<XcdContentNode>();
	
	#endregion // Properties
}
