
namespace Taco.DocNET.Generators;

using Taco.DocNET.Inspector;
using Taco.DocNET.Utilities;

using System.IO;
using System.Collections.Generic;

/// <summary>A generator class that generates whole documentations from dll-xml pairings</summary>
public class DocumentationGenerator
{
	#region Properties
	
	
	
	#endregion // Properties
	
	#region Public Methods
	
	public void Generate(string theme, string outputDir, DllXmlPair dllXml)
	{
		HtmlGenerator generator = new HtmlGenerator(dllXml.XmlAbsolutePath);
		TypeList list = TypeList.GenerateList(dllXml.DllAbsolutePath);
		
		foreach(KeyValuePair<string, List<string>> entry in list.Types)
		{
			foreach(string type in entry.Value)
			{
				string typePath = type.Replace('/', '.');
				
				TypeInfo.GenerateTypeInfo(type, out TypeInfo info, dllXml.DllAbsolutePath);
				
				if(typePath.Contains('<')) { continue; }
				
				string filePath = $@"{outputDir}/{info.AssemblyName}/{typePath.Replace('`', '-').Replace('.', '/')}.html";
				int lastSlash = filePath.LastIndexOf('/');
				
				Utility.EnsurePath(filePath.Substring(0, lastSlash));
				
			}
		}
	}
	
	// public void Generate(string theme, string output)
	// {
	// 	string dir = System.AppDomain.CurrentDomain.BaseDirectory;
	// 	string[] assemblies = Directory.GetFiles(dir, "*.dll");
	// 	TypeList list = TypeList.GenerateList(assemblies);
	// 	HtmlGenerator generator = new HtmlGenerator();
		
	// 	foreach(KeyValuePair<string, List<string>> keyVal in list.Types)
	// 	{
	// 		foreach(string value in keyVal.Value)
	// 		{
	// 			string typePath = value.Replace("/", ".");
				
	// 			TypeInfo.GenerateTypeInfo(assemblies, value, out TypeInfo info);
				
	// 			if(typePath.Contains('<')) { continue; }
				
	// 			string filePath = $"{output}/{info.AssemblyName}/{typePath.Replace("`", "-").Replace(".", "/")}.html";
	// 			int lastDash = filePath.LastIndexOf('/');
				
	// 			Utility.EnsurePath(filePath.Substring(0, lastDash));
	// 			generator.Generate(theme, "base", filePath, info, typePath.Replace("`", "-"));
	// 		}
	// 	}
	// }
	
	#endregion // Public Methods
}
