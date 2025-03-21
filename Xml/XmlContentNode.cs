
namespace DocNET.Xml;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

using DocNET.Inspections;

public class XmlContentNode
{
	#region Properties
	
	public List<XmlContentNode> Children { get; private set; } = new List<XmlContentNode>();
	
	#endregion // Properties
}
