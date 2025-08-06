
namespace Nova.Boot;

using Godot;
using Godot.Collections;

/// <summary>A debug node that lets you scroll between scenes found within `res://scenes/` using `PageUp` and `PageDown`.</summary>
public partial class DebugSceneSelector : Node
{
	#region Properties
	
	private bool isLeftHeld;
	private bool isRightHeld;
	
	[Export] public Array<string> SceneFiles { get; set; } = new Array<string>();
	[Export] public int SelectedScene { get; set; } = 0;
	[Export] public BootLoader BootLoader { get; set; }
	
	#endregion // Properties
	
	#region Godot Methods
	
	/// <inheritdoc/>
	public override void _Ready()
	{
		this.SceneFiles = ResourceLocator.GetFiles("res://scenes");
	}
	
	/// <inheritdoc/>
	public override void _Input(InputEvent ev)
	{
		if(!this.isLeftHeld && Input.IsKeyPressed(Key.Pagedown))
		{
			this.isLeftHeld = true;
		}
		else if(this.isLeftHeld && !Input.IsKeyPressed(Key.Pagedown))
		{
			this.isLeftHeld = false;
			--this.SelectedScene;
			if(this.SelectedScene < 0)
			{
				this.SelectedScene = this.SceneFiles.Count - 1;
			}
			// Move scene backwards
			GD.Print("Next Scene: ", this.SceneFiles[this.SelectedScene]);
			this.BootLoader.SetScene(this.SceneFiles[this.SelectedScene]);
		}
		
		if(!this.isRightHeld && Input.IsKeyPressed(Key.Pageup))
		{
			this.isRightHeld = true;
		}
		else if(this.isRightHeld && !Input.IsKeyPressed(Key.Pageup))
		{
			this.isRightHeld = false;
			++this.SelectedScene;
			if(this.SelectedScene >= this.SceneFiles.Count)
			{
				this.SelectedScene = 0;
			}
			// Move scene forward
			GD.Print("Next Scene: ", this.SceneFiles[this.SelectedScene]);
			this.BootLoader.SetScene(this.SceneFiles[this.SelectedScene]);
		}
	}
	
	#endregion // Godot Methods
}
