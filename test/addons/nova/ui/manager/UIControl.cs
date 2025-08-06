
namespace Nova.UI;

using Godot;

/// <summary>A <see cref="Godot.Control"/> used by the <see cref="UIManagerNode"/> to maintain the control.</summary>
public abstract partial class UIControl : Control
{
	#region Properties
	
	/// <summary>Gets the data that the control uses</summary>
	/// <remarks>All data should be decoupled from the control code completely</remarks>
	[Export] public UIData Data { get; private set; }
	
	/// <summary>Gets and sets the type of view this control belongs to.</summary>
	[Export] public ViewType ViewType { get; set; }
	
	/// <summary>Gets the view for keyboard and mouse users.</summary>
	[Export] public UIView KeyboardView { get; private set; }
	
	/// <summary>Gets the view for gamepad users.</summary>
	[Export] public UIView GamepadView { get; private set; }
	
	/// <summary>Gets the view for mobile users.</summary>
	[Export] public UIView MobileView { get; private set; }
	
	/// <summary>Gets if the control should always update even when not in focus.</summary>
	[Export] public bool AlwaysUpdate { get; protected set; } = false;
	
	/// <summary>Gets if the control is on.</summary>
	public bool IsOn { get; protected set; } = false;
	
	/// <summary>An event for when the view gets changed.</summary>
	/// <param name="control">The control that got it's view changed.</param>
	/// <param name="oldView">The old view before the change.</param>
	/// <param name="newView">The new view being changed to.</param>
	[Signal] public delegate void ViewChangedEventHandler(UIControl control, UIView oldView, UIView newView);
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Gets the data as the given type.</summary>
	/// <typeparam name="T">The type to convert the data as.</typeparam>
	/// <returns>Returns the converted data.</returns>
	public T DataAs<T>() where T : UIData => this.Data as T;
	
	/// <summary>Gets the keyboard view as the given type.</summary>
	/// <typeparam name="T">The type to convert the keyboard view as.</typeparam>
	/// <returns>Returns the converted keyboard view.</returns>
	public T KeyboardViewAs<T>() where T : UIView => this.KeyboardView as T;
	
	/// <summary>Gets the gamepad view as the given type.</summary>
	/// <typeparam name="T">The type to convert the gamepad view as.</typeparam>
	/// <returns>Returns the converted gamepad view.</returns>
	public T GamepadViewAs<T>() where T : UIView => this.KeyboardView as T;
	
	/// <summary>Gets the mobile view as the given type.</summary>
	/// <typeparam name="T">The type to convert the mobile view as.</typeparam>
	/// <returns>Returns the converted mobile view.</returns>
	public T MobileViewAs<T>() where T : UIView => this.KeyboardView as T;
	
	/// <summary>Brings the control to the front of the screen.</summary>
	public virtual void BringToFront()
	{
		Node parent = this.GetParent();
		
		parent.MoveChild(this, parent.GetChildCount());
	}
	
	/// <summary>Changes the view to the given view type.</summary>
	/// <param name="nextViewType">The view type to change to.</param>
	public virtual void ChangeView(ViewType nextViewType)
	{
		UIView oldView = this.GetView(this.ViewType);
		UIView newView = this.GetView(nextViewType);
		
		this.OnViewChanged(oldView, newView);
		this.EmitSignal(SignalName.ViewChanged, this, oldView, newView);
		this.OnChangeView(nextViewType);
	}
	
	#endregion // Public Methods
	
	#region Godot Methods
	
	/// <inheritdoc/>
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(this.IsOn || this.AlwaysUpdate)
		{
			this.OnProcess((float)delta);
		}
	}
	
	#endregion // Godot Methods
	
	#region Private Methods
	
	/// <summary>Called when the view gets changed.</summary>
	/// <param name="oldView">The old view being changed from.</param>
	/// <param name="newView">The new view being changed to.</param>
	protected virtual void OnViewChanged(UIView oldView, UIView newView) {}
	
	/// <inheritdoc cref="Godot.Node._EnterTree"/>
	protected virtual void OnEnterTree()
	{
		this.KeyboardView?.OnEnterTree();
		this.GamepadView?.OnEnterTree();
		this.MobileView?.OnEnterTree();
	}
	
	/// <inheritdoc cref="Godot.Node._ExitTree"/>
	protected virtual void OnExitTree()
	{
		this.KeyboardView?.OnExitTree();
		this.GamepadView?.OnExitTree();
		this.MobileView?.OnExitTree();
	}
	
	/// <inheritdoc cref="Godot.Node._Input"/>
	protected virtual void OnInput(InputEvent ev)
	{
		this.GetView(this.ViewType)?.OnInput(ev);
	}
	
	/// <inheritdoc cref="Godot.Node._Process"/>
	protected virtual void OnProcess(float delta) => this.GetView(this.ViewType)?.OnProcess(delta);
	
	/// <summary>Called when the view gets changed.</summary>
	/// <param name="nextViewType">The view type to change to.</param>
	protected virtual void OnChangeView(ViewType nextViewType)
	{
		if(nextViewType != this.ViewType)
		{
			UIView view = this.GetView(this.ViewType);
			
			view?.OnDisable();
			view?.SetActive(false);
		}
		
		UIView newView = this.GetView(nextViewType);
		
		this.ViewType = nextViewType;
		newView?.OnEnable();
		newView?.SetActive(true);
	}
	
	/// <summary>Called when the view gets toggled.</summary>
	/// <param name="nextViewType">The view type to toggle into.</param>
	protected virtual void OnToggle(ViewType nextViewType)
	{
		UIView newView = this.GetView(nextViewType);
		
		this.ViewType = nextViewType;
		newView?.OnEnable();
		newView?.SetActive(true);
	}
	
	/// <summary>Called when the control gets enabled.</summary>
	protected virtual void OnEnable() {}
	
	/// <summary>Called when the control gets disabled.</summary>
	protected virtual void OnDisable() {}
	
	/// <summary>Find topmost parent.</summary>
	/// <returns>Returns the topmost parent.</returns>
	protected Node FindTopmostParent()
	{
		Node parent = this.GetParent();
		
		while(parent.GetParent() != null && parent.GetParent() as UIManagerNode == null)
		{
			parent = parent.GetParent();
		}
		
		if(parent.GetParent() == null)
		{
			return this.GetParent();
		}
		
		return parent;
	}
	
	/// <summary>Sets up the control to be within focus.</summary>
	protected void SetupFocus()
	{
		Node parent = this.FindTopmostParent();
		
		if(this.GetParent() != parent)
		{
			this.GetParent().RemoveChild(this);
			parent.AddChild(this);
		}
		else
		{
			this.BringToFront();
		}
	}
	
	/// <summary>Sets the control to be active or not.</summary>
	/// <param name="isActive">Set to true to make the control active.</param>
	protected void SetActive(bool isActive)
	{
		this.Visible = isActive;
		this.ProcessMode = isActive
			? Node.ProcessModeEnum.Inherit
			: Node.ProcessModeEnum.Disabled;
	}
	
	/// <summary>Sets the alpha of the control.</summary>
	/// <param name="alpha">The alpha value to set.</param>
	protected void SetAlpha(float alpha)
	{
		Color color = this.Modulate;
		
		color.A = alpha;
		this.Modulate = color;
	}
	
	/// <summary>Hides the control away.</summary>
	protected virtual void HideAway()
	{
		this.SetAlpha(0.0f);
		this.SetActive(false);
		this.IsOn = false;
	}
	
	/// <summary>Gets the current view as a given type.</summary>
	/// <typeparam name="T">The type to convert to.</typeparam>
	/// <returns>Returns the converted current view.</returns>
	protected T GetCurrentView<T>() where T : UIView => this.GetView<T>(this.ViewType);
	
	/// <summary>Gets the given view as a given type.</summary>
	/// <param name="type">The view to get.</param>
	/// <typeparam name="T">The type to convert to.</typeparam>
	/// <returns>Returns the converted view.</returns>
	protected T GetView<T>(ViewType type) where T : UIView => this.GetView(this.ViewType) as T;
	
	/// <summary>Gets the current view.</summary>
	/// <returns>Returns the current view.</returns>
	protected UIView GetCurrentView() => this.GetView(this.ViewType);
	
	/// <summary>Gets the view of the given type.</summary>
	/// <param name="type">The view to get.</param>
	/// <returns>Returns the view.</returns>
	protected UIView GetView(ViewType type) => type switch
	{
		ViewType.Keyboard => this.KeyboardView,
		ViewType.Gamepad => this.GamepadView,
		ViewType.Mobile => this.MobileView,
		_ => this.KeyboardView,
	};
	
	#endregion // Private Methods
}
