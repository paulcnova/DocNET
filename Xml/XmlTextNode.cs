
namespace DocNET.Xml;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

using DocNET.Inspections;

public class XmlTextNode : XmlContentNode
{
	#region Properties
	
	public string Text { get; set; }
	
	#endregion // Properties
}
