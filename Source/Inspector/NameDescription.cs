
namespace Taco.DocNET.Inspector;

/// <summary>A class that holds a name and description</summary>
public class NameDescription
{
	#region Properties
	
	/// <summary>Gets and sets the name of the object</summary>
	public string Name { get; set; }
	
	/// <summary>Gets and sets the description of the object</summary>
	public string Description { get; set; }
	
	/// <summary>A base constructor that create a name-description object</summary>
	/// <param name="name">The name of the object</param>
	/// <param name="description">The description of the object</param>
	public NameDescription(string name, string description)
	{
		this.Name = name;
		this.Description = description;
	}
	
	#endregion // Properties
}
