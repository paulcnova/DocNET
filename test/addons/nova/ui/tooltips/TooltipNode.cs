
namespace Nova.Tooltips;

using Godot;

/// <summary>A node that makes a custom tooltip appear.</summary>
[GlobalClass] public sealed partial class TooltipNode : Node
{
	#region Properties
	
	private BaseTooltipUI tooltip;
	private Timer delayTimer;
	
	/// <summary>Gets and sets the tooltip entry ID for the <see cref="TooltipEncyclopedia"/>.</summary>
	[Export] public string TooltipEntryID { get; set; }
	
	/// <summary>Gets and sets the prefab category used to instantiate the tooltip scene.</summary>
	[Export] public string TooltipPrefabCategory { get; set; }
	
	/// <summary>Gets and sets the delay (in seconds) before the tooltip appears.</summary>
	[Export] public float DelayDuration { get; set; } = 0.0f;
	
	/// <summary>Gets and sets the direct tooltip data to use. If this is not filled, it will use <see cref="TooltipEntryID"/> to get the data instead.</summary>
	[Export] public DisplayableResource TooltipData { get; set; }
	
	/// <summary>Gets and set to true to make the tooltip follow the mouse.</summary>
	[Export] public bool FollowMouse { get; set; } = true;
	
	/// <summary>Gets and sets the offset of the tooltip from the mouse.</summary>
	[Export] public Vector2 Offset { get; set; } = new Vector2(24.0f, -16.0f);
	
	#endregion // Properties
	
	#region Godot Methods
	
	/// <inheritdoc/>
	public override void _Ready()
	{
		Control parent = this.GetParentOrNull<Control>();
		
		if(parent != null)
		{
			parent.MouseEntered += this.OnEntered;
			parent.MouseExited += this.OnExited;
		}
		
		this.delayTimer = new Timer();
		this.delayTimer.Timeout += this.ShowTooltip;
		this.AddChild(this.delayTimer);
		
		this.tooltip = this.CreateTooltip();
		if(this.tooltip == null)
		{
			this.QueueFree();
			return;
		}
		this.tooltip.isTryingToFree = false;
		this.tooltip.TopLevel = true;
		this.tooltip.Container.ResetSize();
		this.tooltip.Hide();
		this.tooltip.FollowMouse = this.FollowMouse;
		this.tooltip.Offset = this.Offset;
		this.AddChild(this.tooltip);
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	/// <summary>Sets up the tooltip.</summary>
	/// <param name="resource">The resource to display onto the tooltip.</param>
	public void Setup(DisplayableResource resource)
	{
		this.TooltipEntryID = resource.TooltipID;
		this.TooltipPrefabCategory = resource.TooltipCategory;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private BaseTooltipUI CreateTooltip() => this.TooltipData != null
		? BaseTooltipUI.Create(this.TooltipData, this.TooltipPrefabCategory)
		: BaseTooltipUI.Create(this.TooltipEntryID, this.TooltipPrefabCategory);
	
	private void ShowTooltip()
	{
		if(this.tooltip == null)
		{
			GDX.PrintError("Cannot show tooltip, tooltip is null!");
			return;
		}
		this.delayTimer.Stop();
		this.tooltip.ShowTooltip();
	}
	
	private void OnEntered()
	{
		if(this.tooltip.IsInspecting)
		{
			this.tooltip.TryToEnter();
			return;
		}
		if(this.DelayDuration > 0.0f)
		{
			this.delayTimer.Start(this.DelayDuration);
		}
		else
		{
			this.ShowTooltip();
		}
	}
	
	private void OnExited()
	{
		if(this.tooltip.IsInspecting)
		{
			this.tooltip.TryToExit();
			return;
		}
		this.delayTimer.Stop();
		this.tooltip.TryToHide();
	}
	
	#endregion // Private Methods
}