
namespace Nova.Logic;

using Godot;

using Nova.Boot;

[GlobalClass] public partial class SceneManagerLogicTrigger : LogicTrigger
{
	#region Properties
	
	[Export] public LogicType Logic { get; set; } = LogicType.PushScene;
	[Export] public PackedScene Scene { get; set; }
	[ExportGroup("Query Methods")]
	[Export] public string SceneLocation { get; set; } = "";
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void TriggerLogic()
	{
		BootLoader loader = BootLoader.Instance;
		
		switch(this.Logic)
		{
			case LogicType.PushScene:
				if(this.Scene != null) { loader.PushScene(this.Scene); }
				else { loader.PushScene(this.SceneLocation); }
				break;
			case LogicType.SetScene:
				if(this.Scene != null) { loader.SetScene(this.Scene); }
				else { loader.SetScene(this.SceneLocation); }
				break;
			case LogicType.PopScene: loader.PopScene(); break;
		}
	}
	
	#endregion // Public Methods
	
	#region Types
	
	public enum LogicType
	{
		PushScene,
		PopScene,
		SetScene,
	}
	
	#endregion // Types
}
