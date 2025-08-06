
namespace Nova.UI;

using Godot;

/// <summary>A simple loading screen that uses a progress bar to show the progress while loading.</summary>
[GlobalClass] public partial class ProgressBarLoadingScreen : LoadingScreen
{
	#region Properties
	
	[Export] public ProgressBar ProgressBar { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc/>
	public override void UpdateLoadingBar(DisplayableResource resource, int current, int max)
	{
		this.ProgressBar.MaxValue = max;
		this.ProgressBar.Value = current;
	}
	
	/// <inheritdoc/>
	public override void LoadingIsCompleted(Callable completedCallback)
	{
		completedCallback.Call();
	}
	
	#endregion // Public Methods
}
