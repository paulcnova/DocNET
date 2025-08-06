
namespace Nova.UI;

/// <summary>A view meant for widgets.</summary>
public abstract partial class WidgetView : UIView
{
	#region Properties
	
	/// <summary>Gets the widget that the view is associated with.</summary>
	public Widget Widget => this.GetParentOrNull<Widget>();
	
	#endregion // Properties
}
