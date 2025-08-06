
namespace Tactics.Actions;

using Godot;
using Godot.Collections;

using Nova;

[GlobalClass] public partial class ActionData : DisplayableResource
{
	#region Properties
	
	[Export] public Array<ActionStage> Stages { get; private set; } = new Array<ActionStage>();
	
	#endregion // Properties
	
	#region Public Methods
	
	public ActionState CreateState()
	{
		ActionState state = new ActionState();
		
		state.Stages.AddRange(this.Stages.Duplicate(true));
		state.FillMetaData(this);
		
		return state;
	}
	
	#endregion // Public Methods
}
