
namespace DocNET.Information;

using DocNET.Inspections;
using DocNET.Utilities;

using System.Xml;

public class MethodInfo : BaseInfo<MethodInspection>
{
	#region Properties
	
	public MethodInfo(MethodInspection inspection, XmlDocument document)
	{
		this.Inspection = inspection;
		this.Xml = XmlFormat.Search($"M:{inspection.Name}", document);
	}
	
	#endregion // Properties
}
