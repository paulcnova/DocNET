
namespace Nova;

using Godot;

/// <summary>A structure for basis coming from a plane flattening.</summary>
public struct PlaneBasis
{
	#region Properties
	
	/// <summary>The forward vector of the basis.</summary>
	public Vector3 Forward;
	
	/// <summary>The right vector of the basis.</summary>
	public Vector3 Right;
	
	/// <summary>The up vector of the basis.</summary>
	public Vector3 Up;
	
	/// <summary>A constructor for creating a plane basis.</summary>
	/// <param name="forward">The forward vector to use for the basis.</param>
	/// <param name="right">The right vector to use for the basis.</param>
	/// <param name="up">The up vector to use for the basis.</param>
	public PlaneBasis(Vector3 forward, Vector3 right, Vector3 up)
	{
		this.Forward = forward;
		this.Right = right;
		this.Up = up;
	}
	
	#endregion // Properties
}
