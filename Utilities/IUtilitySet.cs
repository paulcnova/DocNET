
namespace DocNET.Utilities;

/// <summary>An interface for a utility set to compile from XML to whatever the utility is built for.</summary>
public interface IUtilitySet
{
	#region Public Methods
	
	/// <summary>Creates a hyperlink to any <see cref="System"/> types.</summary>
	/// <param name="typePath">The full path of the type.</param>
	/// <param name="linkName">The name the user will see on the link.</param>
	/// <returns>Returns a hyperlink to the type.</returns>
	string CreateSystemLink(string typePath, string linkName);
	
	/// <summary>Creates a hyperlink to any types within the same library.</summary>
	/// <param name="typePath">The full path of the type.</param>
	/// <param name="linkName">The name the user will see on the link.</param>
	/// <returns>Returns a hyperlink to the type.</returns>
	string CreateInternalLink(string typePath, string linkName);
	
	// TODO: Figure out a way to map external libraries to a way for creating external links. Maybe through the csproj?
	/// <summary>Creates a hyperlink to any types from external libraries.</summary>
	/// <param name="typePath">The full path of the type.</param>
	/// <param name="linkName">The name the user will see on the link.</param>
	/// <returns>Returns a hyperlink to the type.</returns>
	string CreateExternalLink(string typePath, string linkName) => "";
	
	#endregion // Public Methods
}
