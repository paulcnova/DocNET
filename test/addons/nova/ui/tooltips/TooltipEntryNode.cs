
namespace Nova.Tooltips;

using Godot;

/// <summary>A node thats used for the <see cref="TooltipEncyclopediaNode"/> to categorize tooltips within itself.</summary>
public partial class TooltipEntryNode : Node
{
	#region Properties
	
	/// <summary>Gets and sets the resource path to where the tooltip data exists (a DisplayableResource).</summary>
	[Export] public string TooltipPath { get; set; }
	
	#endregion // Properties
}
