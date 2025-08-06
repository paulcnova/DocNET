
namespace Nova;

using Godot;

[GlobalClass] public partial class Camera3DRig : Node
{
	#region Properties
	
	[Export] public bool FollowToTarget { get; set; } = false;
	[Export] public float Delay { get; set; } = 2.0f;
	[Export] public float Distance { get; set; } = 10.0f;
	[Export] public float Angle { get; set; } = 45.0f;
	[Export] public CharacterBody3D Base { get; set; }
	[Export] public Node3D Target { get; set; }
	[Export] public Node3D CameraPoint { get; set; }
	[Export] public Node3D FocusPoint { get; set; }
	
	[ExportGroup("Constraints")]
	[Export] public float MinDistance { get; set; } = 2.0f;
	[Export] public float MaxDistance { get; set; } = 20.0f;
	[Export] public float MinAngle { get; set; } = 32.0f;
	[Export] public float MaxAngle { get; set; } = 114.0f;
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Process(double delta)
	{
		Vector3 distance = this.GetCameraPointFromAngle();
		if(this.FollowToTarget)
		{
			this.Base.GlobalPosition = this.Target.GlobalPosition;
		}
		this.CameraPoint.Position = this.FocusPoint.Position + distance * this.Distance;
		this.CameraPoint.LookAtFromPosition(this.CameraPoint.GlobalPosition, this.FocusPoint.GlobalPosition, Vector3.Up);
	}
	
	public override void _Ready()
	{
		this.Angle = Mathf.Clamp(this.Angle, this.MinAngle, this.MaxAngle);
		this.Distance = Mathf.Clamp(this.Distance, this.MinDistance, this.MaxDistance);
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void Shift(float delta)
	{
		this.Angle = Mathf.Clamp(this.Angle + delta * 90.0f, this.MinAngle, this.MaxAngle);
	}
	
	public void Zoom(float delta)
	{
		this.Distance = Mathf.Clamp(this.Distance + delta, this.MinDistance, this.MaxDistance);
	}
	
	public void Rotate(float delta)
	{
		this.Base.RotateY(delta);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private Vector3 GetCameraPointFromAngle()
	{
		float angle = Mathf.DegToRad(this.Angle);
		
		return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0.0f);
	}
	
	#endregion // Private Methods
}
