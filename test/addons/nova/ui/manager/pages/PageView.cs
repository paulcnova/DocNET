
namespace Nova.UI;

/// <summary>A view meant for pages.</summary>
public abstract partial class PageView : UIView
{
	#region Properties
	
	/// <summary>Gets the page that the view is associated with.</summary>
	public Page Page => this.GetParentOrNull<Page>();
	
	#endregion // Properties
}
