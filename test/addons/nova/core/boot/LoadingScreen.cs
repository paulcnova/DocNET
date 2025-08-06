
namespace Nova.UI;

using Godot;

/// <summary>A loading screen control that is used on boot to load in every displayable resource.</summary>
[GlobalClass] public abstract partial class LoadingScreen : Control
{
	#region Public Methods
	
	/// <summary>Called when updating the loading bar.</summary>
	/// <param name="resource">The resource that's getting loaded in.</param>
	/// <param name="current">The current count of resources being loaded.</param>
	/// <param name="max">The maximum count of resources being loaded.</param>
	public abstract void UpdateLoadingBar(DisplayableResource resource, int current, int max);
	
	/// <summary>Called when loading is completed.</summary>
	/// <param name="completedCallback">The callback to call to continue with the game after everything is loaded and completed.</param>
	public abstract void LoadingIsCompleted(Callable completedCallback);
	
	#endregion // Public Methods
}
