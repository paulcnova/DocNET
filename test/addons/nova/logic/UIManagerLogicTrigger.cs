
namespace Nova.Logic;

using Godot;

using Nova;

[GlobalClass] public partial class UIManagerLogicTrigger : LogicTrigger
{
	#region Properties
	
	[Export] public LogicType Logic { get; set; } = LogicType.OpenPage;
	[Export] public string ClassName { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void TriggerLogic()
	{
		System.Type type = this.ClassName.AsType();
		
		switch(this.Logic)
		{
			case LogicType.OpenPage: UIManager.OpenPage(type); break;
			case LogicType.ClosePage: UIManager.ClosePage(); break;
			case LogicType.ShowWidget: UIManager.ShowWidget(type); break;
			case LogicType.HideWidget: UIManager.HideWidget(type); break;
			case LogicType.ToggleWidget: UIManager.ToggleWidget(type); break;
			case LogicType.HideAllWidgets: UIManager.HideAllWidgets(); break;
		}
	}
	
	#endregion // Public Methods
	
	#region Types
	
	public enum LogicType
	{
		OpenPage,
		ClosePage,
		ShowWidget,
		HideWidget,
		ToggleWidget,
		HideAllWidgets,
	}
	
	#endregion // Types
}
