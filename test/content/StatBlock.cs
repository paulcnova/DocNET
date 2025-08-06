
namespace Tactics;

using Godot;
using Godot.Collections;

using Nova;

[GlobalClass] public partial class StatBlock : DisplayableResource
{
	#region Properties
	
	[Export] public virtual int LandSpeed { get; set; } = 25;
	
	#endregion // Properties
}
