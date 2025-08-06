
namespace Nova.UI;

/// <summary>A widget view that directly links to the widget and data nodes.</summary>
/// <typeparam name="TControl">The type of widget the view belongs to.</typeparam>
/// <typeparam name="TData">The data the view uses.</typeparam>
public abstract partial class WidgetView<TControl, TData> : WidgetView
	where TControl : Widget
	where TData : WidgetData
{
	#region Public Methods
	
	/// <summary>Gets the widget that the view belongs to.</summary>
	/// <returns>Returns the widget that the view belongs to.</returns>
	public TControl GetWidget() => this.Widget as TControl;
	
	/// <summary>Gets the data that the view uses.</summary>
	/// <returns>Returns the data that the view uses.</returns>
	public TData GetData() => this.DataAs<TData>();
	
	#endregion // Public Methods
}
