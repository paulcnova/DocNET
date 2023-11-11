
namespace Taco.DocNET.Generators;

using HandlebarsDotNet;

using Taco.DocNET.Inspector;
using Taco.DocNET.Utilities;

using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>A generator class that generates HTML content for individual types</summary>
public class HtmlGenerator
{
	#region Properties
	
	/// <summary>Gets the list of namespaces found from types generated</summary>
	public List<string> Namespaces { get; private set; } = new List<string>();
	
	/// <summary>Gets the dictionary of member identifiers that maps to the documentation content</summary>
	public Dictionary<string, XmlFormat> XmlContent { get; private set; } = new Dictionary<string, XmlFormat>();
	
	/// <summary>A base constructor to generate HTML</summary>
	/// <param name="xmlLocation">The location to the XML documentation</param>
	public HtmlGenerator(string xmlLocation)
	{
		FillXmlContent(xmlLocation);
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public void Generate(string theme, string view, string output, TypeInfo info, string typePath)
	{
		
	}
	
	// public void Generate(string theme, string view, string output, TypeInfo info, string typePath)
	// {
	// 	string path = Path.Combine(Utility.AppDataPath, "themes", theme);
		
	// 	Utility.EnsurePath(path);
		
	// 	string file = File.ReadAllText(Path.Combine(path, $"{view}.hbs"));
	// 	IHandlebars handlebars = Handlebars.Create();
	// 	Helper helper = new Helper(handlebars, info, theme)
	// 	{
	// 		Project = new Project()
	// 		{
	// 			Name = "Test",
	// 			FavIcon = "https://archive.smashing.media/assets/344dbf88-fdf9-42bb-adb4-46f01eedd629/d0a4481f-e801-4cb7-9daa-17cdae32cc89/icon-design-21-opt.png",
	// 		},
	// 		Generator = this,
	// 		ViewType = "type",
	// 		TypePath = typePath,
	// 	};
	// 	HandlebarsTemplate<object, object> template = handlebars.Compile(file);
	// 	string compiled = template.Invoke(helper);
		
	// 	Utility.Write(output, compiled);
	// }
	
	// public string GenerateView(string theme, string view, Helper helper, string viewType)
	// {
	// 	string path = Path.Combine(Utility.AppDataPath, "themes", theme);
		
	// 	Utility.EnsurePath(path);
		
	// 	string file = File.ReadAllText(Path.Combine(path, $"{view}.hbs"));
	// 	HandlebarsTemplate<object, object> template = (new Helper(helper, viewType)).Handlebars.Compile(file);
		
	// 	return template.Invoke(helper, viewType);
	// }
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void FillXmlContent(string xmlLocation)
	{
		XmlDocument document = new XmlDocument();
		
		document.Load(xmlLocation);
		
		XmlNodeList members = document.GetElementsByTagName("member");
		
		foreach(XmlElement member in members)
		{
			string typePath = member.Attributes["name"].Value;
			XmlFormat format = XmlFormat.Generate(member);
			
			format.Type = typePath.Split(':')[0];
			
			this.XmlContent.Add(typePath, format);
		}
	}
	
	#endregion // Private Methods
}
