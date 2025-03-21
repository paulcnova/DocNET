
namespace DocNET.Templates.Godot;

using DocNET.Information;
using DocNET.Inspections;
using DocNET.Utilities;
using DocNET.Xml;

using System.Xml;

public class GodotUtilitySet : IUtilitySet
{
	#region Public Methods
	
	public void ProcessType(string fileName, TypeInfo info)
	{
		int index = System.Math.Max(fileName.LastIndexOf('/'), fileName.LastIndexOf('\\'));
		
		Utility.EnsurePath(fileName.Substring(0, index));
		
		
		XmlDocument document = new XmlDocument();
		XmlElement @class = document.CreateElement("class");
		
		@class.SetAttribute("name", info.Inspection.Info.Name);
		if(!string.IsNullOrEmpty(info.Inspection.BaseType.Name))
		{
			@class.SetAttribute("inherits", info.Inspection.BaseType.Name);
		}
		@class.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
		@class.SetAttribute("xsi:noNamespaceSchemaLocation", "../class.xsd");
		
		@class.AppendChild(this.ProcessMethods(document, info));
		
		document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));
		document.AppendChild(@class);
		document.Save(fileName);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private XmlElement ProcessMethods(XmlDocument document, TypeInfo info)
	{
		XmlElement methods = document.CreateElement("methods");
		
		foreach(MethodInfo method in info.Methods)
		{
			if(method.Inspection.ImplementedType.FullName == "System.Object") { continue; }
			
			XmlElement elem = document.CreateElement("method");
			
			elem.SetAttribute("name", method.Inspection.Name);
			if(!string.IsNullOrEmpty(method.Inspection.Modifier))
			{
				elem.SetAttribute("qualifiers", method.Inspection.Modifier);
			}
			
			XmlElement @return = document.CreateElement("return");
			
			@return.SetAttribute("type", method.Inspection.ReturnType.Name);
			elem.AppendChild(@return);
			
			int index = 0;
			
			foreach(ParameterInspection parameter in method.Inspection.Parameters)
			{
				XmlElement paramElem = document.CreateElement("param");
				
				paramElem.SetAttribute("index", index.ToString());
				paramElem.SetAttribute("name", parameter.Name);
				paramElem.SetAttribute("type", this.MakeGodotFriendly(parameter.TypeInfo.Name));
				++index;
				elem.AppendChild(paramElem);
			}
			
			methods.AppendChild(elem);
		}
		
		return methods;
	}
	
	private string MakeGodotFriendly(string name)
	{
		switch(name.ToLower())
		{
			default: return name;
			case "string": return "String";
		}
	}
	
	#endregion // Private Methods
}
