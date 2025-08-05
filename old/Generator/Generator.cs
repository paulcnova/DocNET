
namespace DocNET.Generation;

using DocNET.Linking;

public class Generator
{
	#region Public Methods
	
	/// <summary>Generates documentation from the given linked type</summary>
	/// <param name="type">The type linked with it's actual documentation</param>
	/// <returns>The generated documentation</return>
	public virtual GeneratedDocumentation Generate(LinkedType type);
	
	#endregion // Public Methods
}
