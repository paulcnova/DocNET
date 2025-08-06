
namespace Nova;

using Godot;

/// <summary>An extension for <see cref="Godot.Camera3D"/> to add helpful methods.</summary>
public static class Extension_Camera3D
{
	#region Public Methods
	
	/// <summary>Picks at objects on screen using the camera.</summary>
	/// <param name="camera">The camera to pick with.</param>
	/// <param name="position">The position of the mouse on screen.</param>
	/// <param name="layer">The collision layer to pick.</param>
	/// <returns>Returns the ray cast info created from picking.</returns>
	public static RaycastInfo Pick(this Camera3D camera, Vector2 position, uint layer)
	{
		Vector3 origin = camera.ProjectRayOrigin(position);
		Vector3 direction = camera.ProjectRayNormal(position);
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
			origin,
			direction * camera.Far,
			layer
		);
		
		return (RaycastInfo)(camera.GetWorld3D().DirectSpaceState.IntersectRay(query));
	}
	
	#endregion // Public Methods
}
