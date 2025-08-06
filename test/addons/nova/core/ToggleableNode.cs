
namespace Nova;

using Godot;

/// <inheritdoc/>
[GlobalClass] public partial class ToggleableNode : Node
{
	#region Properties
	
	private ProcessModeEnum defaultProcessMode;
	
	/// <summary>Gets and sets if the node is enabled</summary>
	[Export] public bool Enabled { get; private set; } = true;
	
	[Signal] public delegate void OnEnabledEventHandler();
	[Signal] public delegate void OnDisabledEventHandler();
	[Signal] public delegate void OnToggledEventHandler(bool enabled);
	
	#endregion // Properties
	
	#region Godot Methods
	
	/// <inheritdoc />
	public override void _Ready()
	{
		this.defaultProcessMode = this.ProcessMode;
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	/// <summary>Enables the node</summary>
	public void Enable() => this.SetEnabled(true);
	
	/// <summary>Disables the node</summary>
	public void Disable() => this.SetEnabled(false);
	
	/// <summary>Sets if the node is enabled or disabled</summary>
	/// <param name="enabled">Set to true to make the node enabled</param>
	public void SetEnabled(bool enabled)
	{
		if(this.Enabled == enabled) { return; }
		
		this.Enabled = enabled;
		this.ProcessMode = enabled ? this.defaultProcessMode : ProcessModeEnum.Disabled;
		
		if(enabled)
		{
			this.OnEnable();
			this.EmitSignalOnEnabled();
		}
		else
		{
			this.OnDisable();
			this.EmitSignalOnDisabled();
		}
		this.EmitSignalOnToggled(enabled);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	protected virtual void OnEnable() {}
	protected virtual void OnDisable() {}
	
	#endregion // Private Methods
}
