
namespace Nova;

/// <summary>An attribute that gives a asset location to a scene to a class.</summary>
public sealed class SceneLocationAttribute : System.Attribute
{
	#region Properties
	
	/// <summary>Gets the path to the scene.</summary>
	public string ScenePath { get; private set; }
	
	/// <summary>A constructor for the scene location attribute.</summary>
	/// <param name="scenePath">The path to the scene.</param>
	public SceneLocationAttribute(string scenePath)
	{
		this.ScenePath = scenePath;
	}
	
	#endregion // Properties
}
