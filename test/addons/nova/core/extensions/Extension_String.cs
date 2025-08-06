
namespace Nova;

using System.Linq;
using System.Reflection;

/// <summary>An extension for <see cref="System.String"/> for some helpful methods.</summary>
public static class Extension_String
{
	#region Public Methods
	
	/// <summary>Converts the string (assuming it's naming a type) into a type.</summary>
	/// <param name="str">The string to convert.</param>
	/// <returns>Returns the converted type.</returns>
	public static System.Type AsType(this string str) => Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == str);
	
	#endregion // Public Methods
}
