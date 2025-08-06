
namespace Tactics.Actions;

using Godot;
using Godot.Collections;

using Nova;

using Tactics.Controls.Features;

[GlobalClass] public partial class MoveActionStage : ActionStage
{
	#region Public Methods
	
	public override string GetDefaultName() => "Move";
	
	public override Array<string> GetFeaturesToEnable()
	{
		Array<string> features = base.GetFeaturesToEnable();
		
		features.Add(nameof(RangeACF));
		features.Add(nameof(MovementACF));
		
		return features;
	}
	
	#endregion // Public Methods
}
