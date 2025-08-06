
namespace Tactics.Creatures;

using Godot;

using Nova;

public partial class CreatureBody : CharacterBody3D
{
	#region Properties
	
	[Export] public float Width { get; set; } = 1.0f;
	[Export] public CreatureSheet Sheet { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual void OnMove() {}
	
	#endregion // Public Methods
}
