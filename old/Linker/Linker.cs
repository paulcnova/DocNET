
namespace DocNET.Linking;

using DocNET.Information;
using DocNET.Inspections;

using System.Collections.Generic;

public sealed class Linker : IEnumerable<LinkedType>
{
	#region Public Methods
	
	/// <summary>A constructor that links together the type's inspection as well as it's information</summary>
	/// <param name="inspection">The actual type's inspection (source code).</param>
	/// <param name="information">The documentation tied to the type</param>
	/// <param name="environment">The environment meant to keep data types and inspections consistent</param>
	public Linker(TypeInspection inspection, TypeInfo information, ProjectEnvironment environment);
	
	#endregion // Public Methods
}
