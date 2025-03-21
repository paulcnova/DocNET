
namespace DocNET.Information;

using DocNET.Inspections;
using DocNET.Utilities;

using System.Collections.Generic;
using System.Xml;

public class TypeInfo : BaseInfo<TypeInspection>
{
	#region Properties
	
	public List<MethodInfo> Methods { get; private set; } = new List<MethodInfo>();
	
	public TypeInfo(string typePath, string[] assemblies, string xmlFile, bool ignorePrivate = true)
	{
		this.Inspection = TypeInspection.Search(typePath, assemblies, ignorePrivate);
		this.Xml = XmlFormat.Search($"T:{this.Inspection.Info.UnlocalizedName}", xmlFile);
		
		XmlDocument document = XmlFormat.Find(xmlFile);
		
		foreach(MethodInspection method in MethodInspection.CreateArray(TypeInspection.SearchDefinition(typePath, assemblies, ignorePrivate), true, true, ignorePrivate: ignorePrivate))
		{
			this.Methods.Add(new MethodInfo(method, document));
		}
	}
	
	#endregion // Properties
}
