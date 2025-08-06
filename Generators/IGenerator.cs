
namespace DocNET.Generators;

using DocNET.Linking;

public interface IGenerator
{
	#region Public Methods
	
	GeneratedDocumentation Generate(LinkedMember member);
	
	#endregion // Public Methods
}
