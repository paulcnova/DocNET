
namespace Tactics.Controls.Features;

using Godot;

using Nova;

using Tactics.Actions;

[GlobalClass] public partial class RangeACF : ActionControllerFeature
{
	#region Properties
	
	public const string AnimationID = "spread";
	
	[Export] private float MaxRange { get; set; } = 25.0f;
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public Decal Decal { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void Setup(ActionController controller, ActionState state, int stageIndex, Vector3 position)
	{
		base.Setup(controller, state, stageIndex, position);
		
		Animation anim = this.AnimationPlayer.GetAnimation(AnimationID);
		Vector3 spread = anim.TrackGetKeyValue(0, 1).AsVector3();
		
		this.MaxRange = this.GetData(state, stageIndex, MID_MaxRange, 25.0f);
		anim.TrackSetKeyValue(0, 1, new Vector3(this.MaxRange, spread.Y, this.MaxRange));
		this.Decal.GlobalPosition = position;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	protected override void OnEnable()
	{
		this.AnimationPlayer.Play(AnimationID);
	}
	
	#endregion // Private Methods
}
