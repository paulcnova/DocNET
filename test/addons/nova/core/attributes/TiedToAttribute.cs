
namespace Nova;

/// <summary>An attribute that lists another type that the class is tied to.</summary>
public sealed class TiedToAttribute : System.Attribute
{
	#region Properties
	
	/// <summary>Gets the other type being linked.</summary>
	public System.Type LinkedType { get; private set; }
	
	/// <summary>A constructor for the attribute to ties another type.</summary>
	/// <param name="linkedType">The other type to link.</param>
	public TiedToAttribute(System.Type linkedType)
	{
		this.LinkedType = linkedType;
	}
	
	#endregion // Properties
}
