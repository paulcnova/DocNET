
namespace Nova.UI;

using Godot;
using Godot.Collections;

/// <summary>A page that knows what type the data is and the general view is.</summary>
/// <typeparam name="TData">The type of the data that the page uses.</typeparam>
/// <typeparam name="TView">The type of general view that the page uses.</typeparam>
public abstract partial class Page<TData, [MustBeVariant] TView> : Page
	where TData : PageData
	where TView : PageView
{
	#region Public Methods
	
	/// <summary>Gets the data for the page.</summary>
	/// <returns>Returns the data for the page.</returns>
	public TData GetData() => this.DataAs<TData>();
	
	/// <summary>Gets the current view of the page.</summary>
	/// <returns>Returns the current view of the page.</returns>
	public TView CurrentView() => this.GetCurrentView<TView>();
	
	/// <summary>Gets a list of all the views the page contains.</summary>
	/// <returns>Returns a list of all the views the page contains.</returns>
	public Array<TView> GetAllViews()
	{
		Array<TView> views = new Array<TView>();
		TView view;
		
		view = this.KeyboardViewAs<TView>();
		if(view != null) { views.Add(view); }
		view = this.GamepadViewAs<TView>();
		if(view != null) { views.Add(view); }
		view = this.MobileViewAs<TView>();
		if(view != null) { views.Add(view); }
		
		return views;
	}
	
	#endregion // Public Methods
}
