
namespace Tactics.Controls.Features;

using Godot;
using Godot.Collections;

using Nova;

using Tactics.Actions;

[GlobalClass] public sealed partial class MovementACF : ActionControllerFeature
{
	#region Properties
	
	private Vector3 center;
	
	[Export] public float Speed { get; set; } = 1.0f;
	[Export] public float MinRange { get; set; } = 0.0f;
	[Export] public float MaxRange { get; set; } = 25.0f;
	[Export] public Path3D Path { get; set; }
	[Export] public CsgPolygon3D Mesh { get; set; }
	[Export] public Material GoodMaterial { get; set; }
	[Export] public Material BadMaterial { get; set; }
	[ExportGroup("Debug")]
	[Export] public bool IsWithinRange { get; private set; }
	
	private float Distance
		=> (this.Controller.Body.GlobalPosition - this.center).Length()
			+ this.Controller.Body.Width * 0.5f;
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Process(double delta)
	{
		Vector3 playerDirection = this.Controller.AlignWithCamera(new Vector3(
			this.Controller.HorizontalMovement,
			0.0f,
			this.Controller.VerticalMovement
		)) * this.Speed;
		
		this.Controller.Body.Velocity = playerDirection;
		this.Controller.Body.MoveAndSlide();
		if(this.Distance >= this.MaxRange * 0.5f)
		{
			// TODO: Make this look smoother.
			this.Controller.Body.Velocity = this.Speed * (this.center - this.Controller.Body.GlobalPosition).Normalized();
			this.Controller.Body.MoveAndSlide();
		}
		this.Controller.Body.OnMove();
		this.SetPath();
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public override void Setup(ActionController controller, ActionState state, int stageIndex, Vector3 position)
	{
		base.Setup(controller, state, stageIndex, position);
		this.Mesh.Material = this.GoodMaterial;
		this.MinRange = this.GetData(state, stageIndex, MID_MinRange, 0.0f);
		this.MaxRange = this.GetData(state, stageIndex, MID_MaxRange, 25.0f);
		this.Speed = this.GetData(state, stageIndex, MID_Speed, 8.0f);
		this.Controller.SetLockCamera(true);
		this.Controller.CameraRig.Base.GlobalPosition = position;
		this.center = position;
		// TODO: Create a ghost clone of the creature being used and move that around instead of the actual creature.
		// TODO: Create a ActionAnimation of some kind to move the creature from where they started to where the target location.
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void SetPath()
	{
		Vector3[] navigation = this.GetNavigationPath(this.center, this.Controller.Body.GlobalPosition);
		Vector3 cellHeight = Vector3.Down * 0.20f; // The navigation mesh has a slight height to it.
		Vector3 prev = navigation.Length > 0 ? navigation[0] : Vector3.Zero;
		bool isFirst = true;
		float distance = 0.0f;
		
		this.Path.Curve.ClearPoints();
		foreach(Vector3 point in navigation)
		{
			float diff = (point - prev).Length();
			
			if(!isFirst && Mathf.IsZeroApprox(diff)) { continue; }
			
			this.Path.Curve.AddPoint(point + cellHeight);
			distance += diff;
			isFirst = false;
			prev = point;
		}
		
		this.IsWithinRange = distance >= this.MinRange && distance <= this.MaxRange * 0.5f;
		this.Mesh.Material = this.IsWithinRange
			? this.GoodMaterial
			: this.BadMaterial;
	}
	
	private Vector3[] GetNavigationPath(Vector3 startingPosition, Vector3 targetPosition)
	{
		if((startingPosition - targetPosition).LengthSquared() < Mathf.Epsilon) { return new Vector3[0]; }
		if(!IsInsideTree()) { return new Vector3[0]; }
		
		Rid defaultMapRid = this.Path.GetWorld3D().NavigationMap;
		Vector3[] path = NavigationServer3D.MapGetPath(
			defaultMapRid,
			startingPosition,
			targetPosition,
			true
		);
		
		return path;
	}
	
	#endregion // Private Methods
}
