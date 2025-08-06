
namespace Nova;

using Godot;

/// <summary>A resource meant to be displayable in-game.</summary>
[GlobalClass] public partial class DisplayableResource : Resource
{
	#region Properties
	
	/// <summary>Gets if this displayable resource is finished and ready to be displayed.</summary>
	/// <remarks>This is meant to be an indicator in-editor to know if the resource is ready to be used or still needs to be worked on.</remarks>
	[Export] public bool IsFinished { get; private set; }
	
	/// <summary>Gets and sets the icon used to display this resource.</summary>
	[Export] public Texture2D Icon { get; set; }
	
	/// <summary>Gets and sets the name of this resource to be displayed.</summary>
	[Export] public string Name { get; set; }
	
	/// <summary>Gets and sets the description of this resource to be displayed.</summary>
	[Export(PropertyHint.MultilineText)] public string Description { get; set; }
	
	/// <summary>Gets and sets the ID of the expansion this resource belongs to.</summary>
	/// <remarks>This is to give differentiation if it comes from the base game, seasonal updates, DLC, expansions, or mods.</remarks>
	[ExportGroup("IDs")]
	[Export] public string ExpansionID { get; set; }
	
	/// <summary>Gets and sets the unique identifier for this particular resource.</summary>
	[Export] public string ID { get; set; }
	
	/// <summary>Gets and sets the ID/path of the tooltip for the tooltip encyclopedia to find.</summary>
	[ExportGroup("Tooltips")]
	[Export] public string TooltipID { get; set; }
	
	/// <summary>Gets and sets the tooltip category used to load in the particular scene, typically {category}_tooltip.tscn.</summary>
	[Export] public string TooltipCategory { get; set; }
	
	/// <summary>Gets and sets the width to override the tooltip's size with.</summary>
	[Export] public int RecommendedTooltipWidth { get; set; } = -1;
	
	/// <summary>Gets and sets the height to override the tooltip's size with.</summary>
	[Export] public int RecommendedTooltipHeight { get; set; } = -1;
	
	/// <summary>Gets and sets the category to override, used only to load in the scene: {category}_tooltip.tscn.</summary>
	/// <remarks>This is used in case <see cref="TooltipCategory"/> is being used as a displayed identifier within the tooltip itself and you want to select a different file to work with.</remarks>
	[Export] public string OverrideTooltipCategory { get; set; }
	
	#endregion // Properties
}
