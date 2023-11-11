
namespace Taco.DocNET.Utilities;

/// <summary>A class that hosts a pairing between an XML file and DLL file</summary>
public class DllXmlPair
{
	#region Properties
	
	/// <summary>Gets and sets the absolute path to the generated XML file</summary>
	public string XmlAbsolutePath { get; set; }
	
	/// <summary>Gets and sets the absolute path to the generated DLL file</summary>
	public string DllAbsolutePath { get; set; }
	
	/// <summary>A base constructor for dll-xml pairing</summary>
	/// <param name="xmlPath">The path to the generated XML file</param>
	public DllXmlPair(string xmlPath) => this.XmlAbsolutePath = xmlPath;
	
	#endregion // Properties
}
