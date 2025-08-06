
namespace Nova.UI;

/// <summary>A page view that directly links to the page and data nodes.</summary>
/// <typeparam name="TControl">The type of page the view belongs to.</typeparam>
/// <typeparam name="TData">The data the view uses.</typeparam>
public abstract partial class PageView<TControl, TData> : PageView
	where TControl : Page
	where TData : PageData
{
	#region Public Methods
	
	/// <summary>Gets the page that the view belongs to.</summary>
	/// <returns>Returns the page that the view belongs to.</returns>
	public TControl GetPage() => this.Page as TControl;
	
	/// <summary>Gets the data that the view uses.</summary>
	/// <returns>Returns the data that the view uses.</returns>
	public TData GetData() => this.DataAs<TData>();
	
	#endregion // Public Methods
}
