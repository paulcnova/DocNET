
namespace Nova.Logic;

using Godot;
using Nova.Boot;


[GlobalClass] public abstract partial class LogicTrigger : Node
{
	#region Properties
	
	[Export] public bool AutoHook { get; set; } = true;
	[ExportGroup("Query Methods")]
	[Export] public TriggerType Type { get; set; } = TriggerType.Hook;
	[Export] public bool OnScenePushed { get; set; } = false;
	[Export] public bool OnScenePopped { get; set; } = false;
	[Export] public StringName HookName { get; set; }
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _EnterTree()
	{
		Node parent = this.GetParent();
		
		if(parent == null)
		{
			this.QueueFree();
			return;
		}
		
		if(this.OnScenePushed)
		{
			BootLoader.Instance.OnScenePushed += this._OnScenePushed;
		}
		if(this.OnScenePopped)
		{
			BootLoader.Instance.OnScenePopped += this._OnScenePopped;
		}
		
		if(this.Type == TriggerType.Hook)
		{
			if(this.AutoHook)
			{
				this.AutoHookIn();
			}
			else if(!string.IsNullOrEmpty(this.HookName))
			{
				parent.Connect(this.HookName, Callable.From(this.TriggerLogic));
			}
		}
		else if(this.Type == TriggerType.EnterTree)
		{
			this.TriggerLogic();
		}
	}
	
	public override void _Ready()
	{
		if(this.Type == TriggerType.Ready)
		{
			this.TriggerLogic();
		}
	}
	
	public override void _ExitTree()
	{
		Node parent = this.GetParent();
		
		if(parent == null)
		{
			return;
		}
		
		if(this.OnScenePushed)
		{
			BootLoader.Instance.OnScenePushed -= this._OnScenePushed;
		}
		if(this.OnScenePopped)
		{
			BootLoader.Instance.OnScenePopped -= this._OnScenePopped;
		}
		
		if(this.Type == TriggerType.Hook)
		{
			if(this.AutoHook)
			{
				this.AutoHookOut();
			}
			else if(!string.IsNullOrEmpty(this.HookName))
			{
				parent.Disconnect(this.HookName, Callable.From(this.TriggerLogic));
			}
		}
		else if(this.Type == TriggerType.ExitTree)
		{
			this.TriggerLogic();
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public abstract void TriggerLogic();
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void _OnScenePushed(Node scene) => this.TriggerLogic();
	private void _OnScenePopped(Node scene) => this.TriggerLogic();
	
	private void AutoHookIn()
	{
		Node parent = this.GetParent();
		
		if(parent is Button btn)
		{
			btn.Pressed += this.TriggerLogic;
		}
	}
	
	private void AutoHookOut()
	{
		Node parent = this.GetParent();
		
		if(parent is Button btn)
		{
			btn.Pressed -= this.TriggerLogic;
		}
	}
	
	#endregion // Private Methods
	
	#region Types
	
	public enum TriggerType
	{
		Hook,
		EnterTree,
		ExitTree,
		Ready,
	}
	
	#endregion // Types
}
