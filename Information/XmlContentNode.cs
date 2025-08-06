
namespace DocNET.Information;

using System.Collections.Generic;
using System.Linq;

public class XmlContentNode
{
	#region Properties
	
	public List<XmlContentNode> Children { get; private set; } = new List<XmlContentNode>();
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual string Flatten()
	{
		return string.Join("", this.Children.ConvertAll(child => child.Flatten()));
	}
	
	#endregion // Public Methods
}
