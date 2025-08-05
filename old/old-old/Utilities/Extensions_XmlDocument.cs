
namespace DocNET.Utilities;

using System.Xml;

internal static class Extensions_XmlDocument
{
	#region Public Methods
	
	public static XmlElement QuickCreate(this XmlDocument document, string tagName, XmlElement[] children = null, (string, string)[] attributes = null, string content = null)
	{
		XmlElement elem = document.CreateElement(tagName);
		
		if(!string.IsNullOrEmpty(content))
		{
			elem.InnerText = content;
		}
		if(attributes != null)
		{
			foreach((string, string) attr in attributes)
			{
				elem.SetAttribute(attr.Item1, attr.Item2);
			}
		}
		if(children != null)
		{
			foreach(XmlElement child in children)
			{
				elem.AppendChild(child);
			}
		}
		
		return elem;
	}
	
	#endregion // Public Methods
}
