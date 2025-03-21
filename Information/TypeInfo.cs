
namespace DocNET.Information;

using DocNET.Inspections;
using DocNET.Utilities;

public class TypeInfo : BaseInfo<TypeInspection>
{
	#region Properties
	
	public TypeInfo(string typePath, string[] assemblies, string xmlFile, bool ignorePrivate = true)
	{
		this.Inspection = TypeInspection.Search(typePath, assemblies, ignorePrivate);
		this.Xml = XmlFormat.Search($"T:{this.Inspection.Info.UnlocalizedName}", xmlFile);
	}
	
	#endregion // Properties
}
