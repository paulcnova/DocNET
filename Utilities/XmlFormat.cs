
namespace DocNET.Utilities;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

using DocNET.Inspections;

/// <summary>A class that holds the format of the XML content</summary>
public partial class XmlFormat
{
	#region Properties
	
	/// <summary>Gets and sets the name of the type</summary>
	public string Type { get; set; } = "";
	
	/// <summary>Gets and sets the content of the summary documentation</summary>
	public string Summary { get; set; } = "No description.";
	
	/// <summary>Gets and sets the content of the returns documentation</summary>
	public string Returns { get; set; } = "";
	
	/// <summary>Gets and sets the content of the remarks documentation</summary>
	public string Remarks { get; set; } = "";
	
	/// <summary>Gets and sets the content of the example documentation</summary>
	public string Example { get; set; } = "";
	
	/// <summary>Gets the list of names and descriptions of the parameters documentation</summary>
	public List<NameDescription> Parameters { get; private set; } = new List<NameDescription>();
	
	/// <summary>Gets the list of names and descriptions of the exceptions documentation</summary>
	public List<NameDescription> Exceptions { get; private set; } = new List<NameDescription>();
	
	/// <summary>Gets the list of names and descriptions of the type parameters documentation</summary>
	public List<NameDescription> TypeParameters { get; private set; } = new List<NameDescription>();
	
	/// <summary>Generates an xml format class from a given xml element found in the generated docs.</summary>
	/// <param name="member">The xml node for the member to generate the xml format for.</param>
	public XmlFormat(XmlElement member)
	{
		List<NameDescription> parameters = this.GatherNameDescriptionList(member.GetElementsByTagName("param"), "name");
		List<NameDescription> exceptions = this.GatherNameDescriptionList(member.GetElementsByTagName("exception"), "cref");
		List<NameDescription> typeParameters = this.GatherNameDescriptionList(member.GetElementsByTagName("typeparam"), "name");
		
		this.Summary = this.GetMarkdownTextContent(member, "summary", "No description");
		this.Returns = this.GetMarkdownTextContent(member, "returns");
		this.Remarks = this.GetMarkdownTextContent(member, "remarks");
		this.Example = this.GetMarkdownTextContent(member, "example");
		this.Parameters.AddRange(parameters);
		this.Exceptions.AddRange(exceptions);
		this.TypeParameters.AddRange(typeParameters);
		
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static XmlFormat Search(string typePath, string xmlFile)
	{
		System.Console.WriteLine(typePath);
		XmlDocument document = new XmlDocument();
		
		document.Load(xmlFile);
		
		foreach(XmlElement elem in document["doc"]["members"])
		{
			if(elem.HasAttribute("name") && elem.GetAttribute("name") == typePath)
			{
				return new XmlFormat(elem);
			}
		}
		
		return null;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private List<NameDescription> GatherNameDescriptionList(XmlNodeList members, string attrName)
	{
		List<NameDescription> list = new List<NameDescription>();
		
		foreach(XmlElement member in members)
		{
			string name = member.Attributes[attrName]?.Value;
			
			if(string.IsNullOrEmpty(name)) { continue; }
			
			string desc = TrimTextContent(GetTextContent(member, "No description"));
			
			list.Add(new NameDescription(name, Utility.RenderMarkdown(desc)));
		}
		
		return list;
	}
	
	private string GetMarkdownTextContent(XmlElement member, string id, string defaultText = "")
	{
		XmlNodeList elements = member.GetElementsByTagName(id);
		
		if(elements.Count == 0) { return defaultText; }
		
		string desc = this.TrimTextContent(this.GetTextContent(elements[0] as XmlElement, defaultText));
		
		return Utility.RenderMarkdown(desc);
	}
	
	private string TrimTextContent(string content)
	{
		string results = InspectionRegex.Unindent().Replace(content, "").Trim();
		
		if(results != "" && !(
			results.EndsWith('.')
			|| results.EndsWith('!')
			|| results.EndsWith('?')
			|| results.EndsWith("```")
		))
		{
			results += '.';
		}
		
		return results;
	}
	
	private string GetTextContent(XmlElement member, string defaultText)
	{
		if(string.IsNullOrEmpty(member.InnerText)) { return defaultText; }
		
		string content = "";
		
		if(member.HasChildNodes)
		{
			foreach(XmlNode element in member.ChildNodes)
			{
				switch(element.Name)
				{
					default:
						System.Console.WriteLine(element.Name);
						content += element.InnerText;
						break;
					case "#text":
						content += element.InnerText;
						break;
					case "paramref":
						content += @"<span class=""paramref"">";
						content += element.Attributes["name"];
						content += "</span>";
						break;
					case "see":
						if(element.Attributes != null && element.Attributes["langword"] != null)
						{
							content += $@"<span class=""langword"">{element.Attributes["langword"]}</span>";
						}
						else if(element.Attributes != null && element.Attributes["cref"] != null)
						{
							Match match = InspectionRegex.Cref().Match(element.Attributes["cref"].Value);
							
							if(!match.Success) { break; }
							
							Match typeMatch = InspectionRegex.Type().Match(match.Groups[2].Value);
							
							if(!typeMatch.Success) { break; }
							
							bool isSystem = typeMatch.Groups[1].Value.StartsWith("System");
							string link = isSystem
								? match.Groups[2].Value.Replace('`', '-').Replace('/', '.')
								: typeMatch.Groups[match.Groups[1].Value == "T" ? 0 : 1].Value;
							string name = !typeMatch.Groups[1].Success
								? typeMatch.Groups[0].Value
								: InspectionRegex.GenericNotation().Replace(typeMatch.Groups[2].Value, "");
							
							content += isSystem
								? Utility.CreateSystemLink(link, name)
								: Utility.CreateInternalLink(link, name);
						}
						break;
				}
			}
		}
		else
		{
			content = member.InnerText;
		}
		
		return content;
	}
	
	#endregion // Private Methods
}
