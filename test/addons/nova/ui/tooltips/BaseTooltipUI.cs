
namespace Nova.Tooltips;

using Godot;

/// <summary>The basis for a custom tooltip control.</summary>
[GlobalClass] public abstract partial class BaseTooltipUI : Control
{
	#region Properties
	
	private const string TooltipPrefabBase = "res://interface/tooltips";
	private const string GeneralTooltipPrefabBase = $"{TooltipPrefabBase}/general_tooltip.tscn";
	
	private Timer delayTimer;
	private Vector2 tooltipSize;
	private bool isFadingIn = false;
	private bool isInspecting = false;
	internal bool isTryingToExit = false;
	internal bool isTryingToFree = true;
	
	/// <summary>Gets and sets the fade in time of the tooltip.</summary>
	protected float fadeInTime;
	
	/// <summary>Gets and sets the backdrop that the tooltip uses when inspecting it.</summary>
	[Export] public ColorRect Backdrop { get; set; }
	
	/// <summary>Gets and sets the panel container that the tooltip uses to contain it's contents.</summary>
	[Export] public PanelContainer Container { get; set; }
	
	/// <summary>Gets and sets the name label of the tooltip.</summary>
	[Export] public Label TooltipName { get; set; }
	
	/// <summary>Gets and sets the tooltip description rich text label that enables nesting tooltips.</summary>
	[Export] public NestedTooltipRichTextLabel TooltipDescription { get; set; }
	
	/// <summary>Gets and sets the delay the tooltip takes before it appears.</summary>
	[ExportGroup("Tooltip Settings")]
	[Export] public float DelayTime { get; set; } = 0.0f;
	
	/// <summary>Gets and sets if the tooltip should follow the mouse.</summary>
	[Export] public bool FollowMouse { get; set; } = true;
	
	/// <summary>Gets and sets the offset of the tooltip from the mouse.</summary>
	[Export] public Vector2 Offset { get; set; } = new Vector2(24.0f, -16.0f);
	
	/// <summary>Gets and sets the padding from the edges of the screen that the tooltip will try and avoid.</summary>
	[Export] public Vector2 Padding { get; set; } = 32.0f * Vector2.One;
	
	/// <summary>Gets and sets the fade in duration of the tooltip.</summary>
	[Export] public float FadeInDuration { get; set; } = 0.15f;
	
	/// <summary>Gets and sets the action name of the player trying to inspect the tooltip.</summary>
	[ExportGroup("Input")]
	[Export] public string InspectAction { get; set; } = "tooltip_inspect";
	
	/// <summary>Gets and sets the action name of the player trying to exit the tooltip from focus.</summary>
	[Export] public string ExitAction { get; set; } = "tooltip_exit";
	
	/// <summary>Gets if the tooltip is currently being inspected.</summary>
	public bool IsInspecting => this.isInspecting;
	
	/// <summary>Gets if this tooltip is nested.</summary>
	public bool IsNestedTooltip
	{
		get
		{
			Node parent = this.GetParent();
			
			while(parent != null)
			{
				if(parent is BaseTooltipUI) { return true; }
				
				parent = parent.GetParent();
			}
			
			return false;
		}
	}
	
	#endregion // Properties
	
	#region Godot Methods
	
	/// <inheritdoc/>
	public override void _Ready()
	{
		this.MouseFilter = MouseFilterEnum.Ignore;
		this.Backdrop.Visible = false;
		this.delayTimer = new Timer();
		this.delayTimer.Timeout += this.ShowTooltip;
		this.AddChild(this.delayTimer);
		
		this.Container.ResetSize();
		this.tooltipSize = this.Container.GetRect().Size;
		this.PositionTooltip();
	}
	
	/// <inheritdoc/>
	public override void _Process(double delta)
	{
		if(!this.isInspecting)
		{
			this.Container.ResetSize();
			this.tooltipSize = this.Container.GetRect().Size;
			this.PositionTooltip();
		}
		if(this.Visible)
		{
			if(this.isFadingIn)
			{
				this.fadeInTime += (float)delta;
				this.SetAlpha(Mathf.Lerp(0.0f, 1.0f, (this.fadeInTime / this.FadeInDuration)));
				if(this.fadeInTime >= this.FadeInDuration)
				{
					this.isFadingIn = false;
				}
			}
			this.tooltipSize = this.Container.GetRect().Size;
			if(!this.isInspecting)
			{
				if(Input.IsActionJustPressed(this.InspectAction))
				{
					this.isInspecting = true;
					this.MouseFilter = MouseFilterEnum.Stop;
					this.Backdrop.Visible = !this.IsNestedTooltip;
				}
			}
			else
			{
				if(Input.IsActionJustPressed(this.ExitAction))
				{
					this.isInspecting = false;
					this.MouseFilter = MouseFilterEnum.Ignore;
					this.Backdrop.Visible = false;
					if(this.isTryingToExit)
					{
						this.Hide();
						this.delayTimer.Stop();
						this.isTryingToExit = false;
						if(this.isTryingToFree)
						{
							this.QueueFree();
						}
					}
				}
				return;
			}
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	/// <summary>Creates a new tooltip UI.</summary>
	/// <param name="entryID">The ID of the tooltip entry to find within the <see cref="TooltipEncyclopediaNode"/>.</param>
	/// <param name="prefabPath">The partial resource path to the tooltip. The path is: res://interface/tooltips/{prefabPath}_tooltip.tscn.</param>
	/// <returns>Returns the newly created tooltip UI.</returns>
	public static BaseTooltipUI Create(string entryID, string prefabPath)
	{
		DisplayableResource entry = TooltipEncyclopedia.FindEntry(entryID);
		
		if(entry == null)
		{
			GDX.PrintError($"Entry [{entryID}] does not exist, no tooltip is being rendered");
			return null;
		}
		return Create(entry, prefabPath);
	}
	
	/// <summary>Creates a new tooltip UI.</summary>
	/// <param name="entry">The displayable resource to display on the tooltip.</param>
	/// <param name="prefabPath">The partial resource path to the tooltip. The path is: res://interface/tooltips/{prefabPath}_tooltip.tscn.</param>
	/// <returns>Returns the newly created tooltip UI.</returns>
	public static BaseTooltipUI Create(DisplayableResource entry, string prefabPath)
	{
		BaseTooltipUI tooltip = GDX.Instantiate<BaseTooltipUI>($"{TooltipPrefabBase}/{prefabPath}_tooltip.tscn");
		
		if(tooltip == null)
		{
			tooltip = GDX.Instantiate<BaseTooltipUI>(GeneralTooltipPrefabBase);
		}
		
		if(tooltip == null)
		{
			GDX.PrintError($"General Tooltip prefab doesn't exist: could not create tooltip");
			return null;
		}
		
		tooltip.TopLevel = true;
		tooltip.Container.CustomMinimumSize = new Vector2(
			entry.RecommendedTooltipWidth > 0
				? entry.RecommendedTooltipWidth
				: tooltip.Container.CustomMinimumSize.X,
			entry.RecommendedTooltipHeight > 0
				? entry.RecommendedTooltipHeight
				: tooltip.Container.CustomMinimumSize.Y
		);
		tooltip.Setup(entry);
		tooltip.Container.ResetSize();
		
		return tooltip;
	}
	
	/// <summary>Sets up the tooltip UI.</summary>
	/// <param name="entry">The resource to set up the tooltip with.</param>
	public abstract void Setup(DisplayableResource entry);
	
	/// <summary>Called when the user is trying to enter the hit box that would render the tooltip.</summary>
	public void TryToEnter() => this.isTryingToExit = false;
	
	/// <summary>Called when the user is trying to exit the hit box that would de-render the tooltip.</summary>
	public void TryToExit() => this.isTryingToExit = true;
	
	/// <summary>Called when the tooltip is trying to de-render.</summary>
	public void TryToHide()
	{
		if(this.isInspecting) { return; }
		
		this.Hide();
	}
	
	/// <summary>Called when the tooltip is trying to be queue'd free.</summary>
	public void TryToQueueFree()
	{
		this.isTryingToExit = true;
		this.isTryingToFree = true;
		if(!this.isInspecting)
		{
			this.QueueFree();
		}
	}
	
	/// <summary>Shows the tooltip.</summary>
	public void ShowTooltip()
	{
		this.SetAlpha(0.0f);
		this.fadeInTime = 0.0f;
		this.isFadingIn = true;
		this.Container.ResetSize();
		this.tooltipSize = this.Container.GetRect().Size;
		this.PositionTooltip();
		this.Show();
		this.delayTimer.Stop();
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Repositions the tooltip towards the mouse.</summary>
	private void PositionTooltip()
	{
		Vector2 border = this.GetViewport().GetVisibleRect().Size - this.Padding;
		Vector2 position = this.GetTooltipPosition();
		float finalX = position.X + this.Offset.X;
		float finalY = position.Y + this.Offset.Y;
		
		if(finalX + this.tooltipSize.X > border.X)
		{
			finalX = position.X - this.Offset.X - this.tooltipSize.X;
		}
		if(finalY + this.tooltipSize.Y > border.Y)
		{
			finalY = position.Y - this.Offset.Y - this.tooltipSize.Y;
		}
		
		this.Container.Position = new Vector2(finalX, finalY);
	}
	
	/// <summary>Called when the tooltip enter a hit box that renders it.</summary>
	private void OnEntered()
	{
		if(this.isInspecting)
		{
			this.isTryingToExit = false;
			return;
		}
		if(this.DelayTime > 0.0f)
		{
			this.delayTimer.Start(this.DelayTime);
		}
		else
		{
			this.ShowTooltip();
		}
	}
	
	/// <summary>Called when the tooltip exits a hit box that de-renders it.</summary>
	private void OnExited()
	{
		if(this.isInspecting)
		{
			this.isTryingToExit = true;
			return;
		}
		this.delayTimer.Stop();
		this.Hide();
	}
	
	/// <summary>Gets the tooltip's position.</summary>
	/// <returns>Returns the tooltip's position.</returns>
	private Vector2 GetTooltipPosition()
	{
		if(this.FollowMouse)
		{
			return this.GetViewport().GetMousePosition();
		}
		
		Vector2 position = Vector2.Zero;
		Node parent = this.GetParent();
		
		if(parent is Node2D parent2D)
		{
			position = parent2D.GetGlobalTransformWithCanvas().Origin;
		}
		else if(parent is Control parentControl)
		{
			position = parentControl.GetGlobalTransformWithCanvas().Origin;
		}
		
		
		return position;
	}
	
	/// <summary>Sets the alpha of the tooltip.</summary>
	/// <param name="alpha">The alpha value to set the tooltip.</param>
	private void SetAlpha(float alpha)
	{
		Color modulate = this.Modulate;
		
		modulate.A = alpha;
		
		this.Modulate = modulate;
	}
	
	#endregion // Private Methods
}
