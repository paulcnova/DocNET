
namespace DocNET;

using DocNET.Generation;

public class ProjectEnvironment
{
	#region Properties
	
	public string ProjectName { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Creates a documentation generator given the project's environments.</summary>
	/// <returns>The documentation generator used to generate the documentation.</returns>
	public Generator CreateGenerator();
	
	#endregion // Public Methods
}
