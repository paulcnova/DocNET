
namespace Tactics.Controls;

using Godot;
using Godot.Collections;

using Nova;

using Tactics.Actions;
using Tactics.Controls.Features;
using Tactics.Creatures;

[GlobalClass] public partial class ActionController : ToggleableNode
{
	#region Properties
	
	[Export] public Camera3D Camera { get; set; }
	[Export] public Camera3DRig CameraRig { get; set; }
	[Export] public CreatureBody Body { get; set; }
	
	[ExportGroup("Camera")]
	[Export] private bool IsCameraLocked { get; set; } = false;
	[Export] public float CameraSpeed { get; set; } = 5.0f;
	[Export] public float ConstrainedDistance { get; set; } = 25.0f;
	[Export] public float VerticalDeadZone { get; set; } = 0.24f;
	[Export] public float VerticalCameraSpeed { get; set; } = 0.8f;
	[Export] public float HorizontalDeadZone { get; set; } = 0.24f;
	[Export] public float HorizontalCameraSpeed { get; set; } = 4.5f;
	[Export] public float ZoomCameraSpeed { get; set; } = 1.5f;
	
	[ExportGroup("Input")]
	[Export] public string IA_Forward { get; set; } = "ac_forward";
	[Export] public string IA_Backward { get; set; } = "ac_backward";
	public float VerticalMovement => Input.GetAxis(this.IA_Backward, this.IA_Forward);
	[Export] public string IA_Left { get; set; } = "ac_left";
	[Export] public string IA_Right { get; set; } = "ac_right";
	public float HorizontalMovement => Input.GetAxis(this.IA_Left, this.IA_Right);
	[Export] public string IAC_RotateLeft { get; set; } = "acc_rotate_left";
	[Export] public string IAC_RotateRight { get; set; } = "acc_rotate_right";
	public float YawRotation
	{
		get
		{
			if(Input.IsMouseButtonPressed(MouseButton.Middle))
			{
				float eighthWidth = 8.0f / this.GetWindow().Size.X;
				
				return Mathf.Clamp(-Input.GetLastMouseVelocity().X * eighthWidth, -1.0f, 1.0f);
			}
			return Input.GetAxis(this.IAC_RotateLeft, this.IAC_RotateRight);
		}
	}
	[Export] public string IAC_ShiftUp { get; set; } = "acc_shift_up";
	[Export] public string IAC_ShiftDown { get; set; } = "acc_shift_down";
	[Export] public float VerticalChunks { get; set; } = 4.0f;
	public float ShiftMovement
	{
		get
		{
			if(Input.IsMouseButtonPressed(MouseButton.Middle))
			{
				float eighthHeight = this.VerticalChunks / this.GetWindow().Size.Y;
				
				return Mathf.Clamp(-Input.GetLastMouseVelocity().Y * eighthHeight, -1.0f, 1.0f);
			}
			return Input.GetAxis(this.IAC_ShiftDown, this.IAC_ShiftUp);
		}
	}
	[Export] public string IAC_ZoomIn { get; set; } = "acc_zoom_in";
	[Export] public string IAC_ZoomOut { get; set; } = "acc_zoom_out";
	[Export] public float ScrollChunks { get; set; } = 10.0f;
	public float ZoomMovement
	{
		get
		{
			float zoom = Input.GetAxis(this.IAC_ZoomIn, this.IAC_ZoomOut);
			
			if(zoom == 0.0f)
			{
				if(Input.IsActionJustReleased(this.IAC_ZoomIn)) { zoom = -this.ScrollChunks; }
				if(Input.IsActionJustReleased(this.IAC_ZoomOut)) { zoom = this.ScrollChunks; }
			}
			
			return zoom;
		}
	}
	
	[ExportGroup("Debug")]
	[Export] public Array<ActionControllerFeature> Features { get; private set; } = new Array<ActionControllerFeature>();
	[Export] public ActionState CurrentState { get; set; }
	[Export] public ActionStage CurrentStage { get; set; }
	[Export] public string StageName { get; set; }
	[Export] public int StageIndex { get; set; }
	
	public bool IsValid
	{
		get
		{
			foreach(ActionControllerFeature feature in this.Features)
			{
				if(!feature.IsValid) { return false; }
			}
			return true;
		}
	}
	
	#endregion // Properties
	
	#region Godot Methods
	
	/// <inheritdoc />
	public override void _EnterTree()
	{
		this.CreateCustomFeatures();
	}
	
	/// <inheritdoc />
	public override void _Ready()
	{
		this.SnapToCreature();
		this.GatherFeatures();
		this.DisableAllFeature();
		GM.Sheet = this.Body.Sheet;
	}
	
	public override void _Input(InputEvent ev)
	{
		if(Input.IsKeyPressed(Key.Equal))
		{
			this.LoadState(GD.Load<ActionData>("res://content/game_data/actions/move/move.action.tres").CreateState());
		}
		if(this.CurrentState != null && Input.IsKeyPressed(Key.Minus))
		{
			this.NextStage();
		}
	}
	
	public override void _Process(double delta)
	{
		float yaw = this.YawRotation;
		float pitch = this.ShiftMovement;
		float zoom = this.ZoomMovement * (float)delta * this.ZoomCameraSpeed;
		
		if(Mathf.Abs(yaw) > this.HorizontalDeadZone)
		{
			this.CameraRig.Rotate(yaw * (float)delta * this.HorizontalCameraSpeed);
		}
		
		if(Mathf.Abs(pitch) > this.VerticalDeadZone)
		{
			this.CameraRig.Shift(-pitch * (float)delta * this.VerticalCameraSpeed);
		}
		
		if(zoom != 0.0f)
		{
			this.CameraRig.Zoom(zoom);
		}
		
		if(!this.IsCameraLocked)
		{
			Vector3 movement = this.AlignWithCamera(new Vector3(
				this.HorizontalMovement,
				0.0f,
				this.VerticalMovement
			)) * this.CameraSpeed;
			
			this.CameraRig.Base.Velocity = movement;
			this.CameraRig.Base.MoveAndSlide();
			
			Vector3 distance = this.CameraRig.Base.GlobalPosition - this.Body.GlobalPosition;
			
			if(distance.Length() > this.ConstrainedDistance)
			{
				// TODO: Make this look smoother.
				this.CameraRig.Base.Velocity = -this.CameraSpeed * distance.Normalized();
				this.CameraRig.Base.MoveAndSlide();
			}
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void SetLockCamera(bool lockOut)
	{
		this.IsCameraLocked = lockOut;
		this.CameraRig.FollowToTarget = lockOut;
	}
	
	public Vector3 AlignWithCamera(Vector3 direction)
	{
		if(direction.IsZeroApprox()) { return Vector3.Zero; }
		
		Vector3 normalizedDirection = direction.Normalized() * Mathf.Max(Mathf.Abs(direction.X), Mathf.Abs(direction.Z));
		Vector3 forward = this.Camera.GlobalTransform.Basis.Z;
		Vector3 right = this.Camera.GlobalTransform.Basis.X;
		
		forward.Y = 0.0f;
		right.Y = 0.0f;
		
		return normalizedDirection.X * right + normalizedDirection.Y * Vector3.Up - normalizedDirection.Z * forward;
	}
	
	public void DisableAllFeature()
	{
		foreach(ActionControllerFeature feature in this.Features)
		{
			feature.Disable();
		}
	}
	
	public void NextStage()
	{
		if(this.CurrentState.HasStage(this.StageIndex + 1))
		{
			this.LoadStage(this.StageIndex + 1);
		}
		else
		{
			this.UnloadStage();
		}
	}
	
	public void UnloadStage()
	{
		this.ResetController();
		this.StageIndex = -1;
		this.CurrentStage = null;
		this.CurrentState = null;
		this.StageName = "";
	}
	
	public void LoadStage(int stageIndex)
	{
		this.ResetController();
		this.CurrentStage = this.CurrentState.GetStage(stageIndex);
		this.StageIndex = stageIndex;
		this.StageName = !string.IsNullOrEmpty(this.CurrentStage.Name)
			? this.CurrentStage.Name
			: this.CurrentStage.GetDefaultName();
		
		foreach(string featureID in this.CurrentStage.GetFeaturesToEnable())
		{
			ActionControllerFeature feature = this.GetFeature(featureID);
			
			if(feature == null)
			{
				GDX.PrintWarning("Feature not found: ", featureID);
				continue;
			}
			
			feature.Setup(this, this.CurrentState, this.StageIndex, this.Body.GlobalPosition);
			feature.Enable();
		}
	}
	
	public void LoadState(ActionState state)
	{
		this.CurrentState = state;
		this.LoadStage(0);
	}
	
	public ActionControllerFeature GetFeature(string id)
	{
		foreach(ActionControllerFeature feature in this.Features)
		{
			if(feature.ID == id) { return feature; }
		}
		return null;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void SnapToCreature()
	{
		this.CameraRig.Base.GlobalPosition = this.Body.GlobalPosition;
	}
	
	private void ResetController()
	{
		this.DisableAllFeature();
		this.SetLockCamera(false);
	}
	
	/// <summary>Gathers all the features that it has as children and places them inside the <see cref="Features"/> array.</summary>
	private void GatherFeatures()
	{
		foreach(ActionControllerFeature feature in this.GetChildren<ActionControllerFeature>())
		{
			feature.Controller = this;
			this.Features.Add(feature);
		}
	}
	
	private void AddFeature(ActionControllerFeature feature, string name)
	{
		feature.Name = name;
		this.AddChild(feature);
	}
	
	/// <summary>Creates all the custom and modded features the action controller could use.</summary>
	private void CreateCustomFeatures()
	{
		// TODO: Find a way for loaded mods to splice in custom features
	}
	
	#endregion // Private Methods
}
