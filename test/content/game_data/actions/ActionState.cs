
namespace Tactics.Actions;

using Godot;
using Godot.Collections;

using Nova;

public sealed partial class ActionState : Resource
{
	#region Properties
	
	[Export] public Array<ActionStage> Stages { get; private set; } = new Array<ActionStage>();
	
	#endregion // Properties
	
	#region Public Methods
	
	public bool HasStage(int index)
	{
		if(index < 0 || index >= this.Stages.Count) { return false; }
		return true;
	}
	
	public ActionStage GetStage(int index)
	{
		if(!this.HasStage(index)) { return null; }
		return this.Stages[index];
	}
	
	public void FillMetaData(ActionData data)
	{
		Array<StringName> keys = data.GetMetaList();
		
		foreach(StringName key in keys)
		{
			if(key.IsEmpty) { continue; }
			
			this.SetMeta(key, data.GetMeta(key));
		}
	}
	
	#endregion // Public Methods
}
