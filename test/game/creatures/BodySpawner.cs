
namespace Tactics.Creatures;

using Godot;
using Godot.Collections;

using Nova;

[GlobalClass] public partial class BodySpawner : Node3D
{
	#region Properties
	
	[Export] public CreatureSheet Sheet { get; private set; }
	[Export] public PackedScene PackedBody { get; private set; }
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		this.CallDeferred(MethodName.OnLoaded);
	}
	
	#endregion // Godot Methods
	
	#region Private Methods
	
	private void OnLoaded()
	{
		CreatureBody body = GDX.Instantiate<CreatureBody>(this.PackedBody);
		
		if(body == null) { return; }
		
		this.AddSibling(body);
		body.Sheet = this.Sheet;
		body.GlobalPosition = this.GlobalPosition;
	}
	
	#endregion // Private Methods
}
