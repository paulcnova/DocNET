
namespace DocNET.Information;

using DocNET.Inspections;
using DocNET.Utilities;

using System.Collections.Generic;
using System.Xml;

public class TypeInfo : BaseInfo<TypeInspection>
{
	#region Properties
	
	public List<MethodInfo> Methods { get; private set; } = new List<MethodInfo>();
	
	/// <summary>A constructor that gets the info for the type from it's given string name</summary>
	/// <param name="type">The full name of the type to get the information from</param>
	/// <param name="environment">The project's environment to get all the data for the type.</param>
	public TypeInfo(string type, ProjectEnvironment environment);
	
	public TypeInfo(string typePath, string[] assemblies, string xmlFile, bool ignorePrivate = true)
	{
		this.Inspection = TypeInspection.Search(typePath, assemblies, ignorePrivate);
		this.Xml = InformationDocument.Search($"T:{this.Inspection.Info.UnlocalizedName}", xmlFile);
		
		XmlDocument document = InformationDocument.Load(xmlFile);
		
		foreach(MethodInspection method in MethodInspection.CreateArray(TypeInspection.SearchDefinition(typePath, assemblies, ignorePrivate), true, false, ignorePrivate: ignorePrivate))
		{
			this.Methods.Add(new MethodInfo(method, document));
		}
		foreach(MethodInspection method in MethodInspection.CreateArray(TypeInspection.SearchDefinition(typePath, assemblies, ignorePrivate), true, true, ignorePrivate: ignorePrivate))
		{
			this.Methods.Add(new MethodInfo(method, document));
		}
	}
	
	#endregion // Properties
}
