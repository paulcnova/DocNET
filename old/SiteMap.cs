
namespace DocNET;

using System.Collections.Generic;

/// <summary>A class that holds a dictionary of assemblies to types as well as types to links for later reference</summary>
public sealed class SiteMap
{
	#region Properties
	
	/// <summary>A constructor that creates a site map from the given environment</summary>
	/// <param name="environment">The environment to create a site map for</param>
	public SiteMap(ProjectEnvironment environment);
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Finds all the types from the <c>environment</c> that is relevant to the project.</summary>
	/// <returns>A list of all the types relevant to the project</returns>
	public List<string> FindTypes();
	
	#endregion // Public Methods
}
