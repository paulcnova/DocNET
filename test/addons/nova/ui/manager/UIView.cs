
namespace Nova.UI;

using Godot;

/// <summary>A control that is a specific view for use of a specific <see cref="ViewType"/>.</summary>
public abstract partial class UIView : Control
{
	#region Properties
	
	/// <summary>Gets and sets the view type that this view is associated with.</summary>
	[Export] public ViewType ViewType { get; set; }
	
	/// <summary>Gets the parent <see cref="UIControl"/>.</summary>
	public UIControl Parent => this.GetParentOrNull<UIControl>();
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc cref="Godot.Node._EnterTree"/>
	public virtual void OnEnterTree() {}
	
	/// <inheritdoc cref="Godot.Node._ExitTree"/>
	public virtual void OnExitTree() {}
	
	/// <inheritdoc cref="Godot.Node._Process"/>
	public virtual void OnProcess(float delta) {}
	
	/// <inheritdoc cref="Godot.Node._Input"/>
	public virtual void OnInput(InputEvent ev) {}
	
	/// <summary>Called when the view gets enabled.</summary>
	public virtual void OnEnable() {}
	
	/// <summary>Called when the view gets disabled.</summary>
	public virtual void OnDisable() {}
	
	/// <summary>Gets the data as the given type.</summary>
	/// <typeparam name="T">The type to convert the data as.</typeparam>
	/// <returns>Returns the converted data.</returns>
	public T DataAs<T>() where T : UIData => this.Parent.DataAs<T>();
	
	/// <summary>Sets the view to be active or not.</summary>
	/// <param name="isActive">Set to true to make the view active.</param>
	public void SetActive(bool isActive)
	{
		this.Visible = isActive;
		this.ProcessMode = isActive
			? Node.ProcessModeEnum.Inherit
			: Node.ProcessModeEnum.Disabled;
	}
	
	/// <summary>Sets the alpha of the view.</summary>
	/// <param name="alpha">The alpha value to set.</param>
	public void SetAlpha(float alpha)
	{
		Color color = this.Modulate;
		
		color.A = alpha;
		this.Modulate = color;
	}
	
	#endregion // Public Methods
}
