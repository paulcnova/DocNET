
namespace DocNET.Information;

using DocNET.Inspections;
using DocNET.Utilities;

/// <summary>A base class for informational classes that hold an inspection of the codebase along with the XML content tied to that code.</summary>
/// <typeparam name="T">The type of inspection class to hold as data for the class.</typeparam>
public abstract class BaseInfo<T>
{
	#region Properties
	
	/// <summary>Gets the code inspection.</summary>
	public T Inspection { get; protected set; }
	
	/// <summary>Gets the XML content for this piece of code.</summary>
	public XmlFormat Xml { get; protected set; }
	
	#endregion // Properties
}
