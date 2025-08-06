
namespace Nova.UI;

using Godot;

using System.Collections.Generic;

/// <summary>A control that shows UI where multiple widgets can be shown at any time.</summary>
/// <remarks>This is useful for popup and notifications without going away from the page.</remarks>
public abstract partial class Widget : UIControl
{
	#region Properties
	
	/// <summary>The coroutine handle for the fade transition.</summary>
	private CoroutineHandle fadeTransition;
	
	/// <summary>Gets if the widget should show up when the scene starts up.</summary>
	[Export] public bool ShowOnStartup { get; private set; } = false;
	
	/// <summary>Gets the priority of the widget</summary>
	/// <remarks>Higher priority makes the widget show above other widgets even if they were brought to the front. Lower priority makes the widget show behind other widgets even if it was brought to the front.</remarks>
	[Export] public int Priority { get; private set; } = 0;
	
	/// <summary>An event for when the widget gets toggled.</summary>
	/// <param name="widget">The widget in question.</param>
	[Signal] public delegate void ToggledEventHandler(Widget widget);
	
	/// <summary>An event for when the widget gets toggled on.</summary>
	/// <param name="widget">The widget in question.</param>
	[Signal] public delegate void ToggledOnEventHandler(Widget widget);
	
	/// <summary>An event for when the widget gets toggled off.</summary>
	/// <param name="widget">The widget in question.</param>
	[Signal] public delegate void ToggledOffEventHandler(Widget widget);
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc/>
	public override void BringToFront()
	{
		int index = this.GetIndex();
		int nextIndex = index + 1;
		
		while(nextIndex < this.GetParent().GetChildCount())
		{
			Widget widget = this.GetParent().GetChild(nextIndex) as Widget;
			
			if(widget == null) { break; }
			if(widget.Priority != this.Priority) { break; }
			
			index = nextIndex;
			++nextIndex;
		}
		
		this.GetParent().MoveChild(this, index);
	}
	
	/// <summary>Brings the control to the back of the screen.</summary>
	public void BringToBack()
	{
		int index = this.GetIndex();
		int nextIndex = index - 1;
		
		while(nextIndex >= 0)
		{
			Widget widget = this.GetParent().GetChild(nextIndex) as Widget;
			
			if(widget == null) { break; }
			if(widget.Priority != this.Priority) { break; }
			
			index = nextIndex;
			--nextIndex;
		}
		
		this.GetParent().MoveChild(this, index);
	}
	
	/// <summary>Brings the widget one forward closer to the front of the screen.</summary>
	public void MoveForwardOne()
	{
		int index = this.GetIndex();
		
		if(index + 1 >= this.GetParent().GetChildCount()) { return; }
		
		Widget widget = this.GetParent().GetChild(index + 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		this.GetParent().MoveChild(this, index + 1);
	}
	
	/// <summary>Brings the widget one back closer to the back of the screen.</summary>
	public void MoveBackOne()
	{
		int index = this.GetIndex();
		
		if(index - 1 < 0) { return; }
		
		Widget widget = this.GetParent().GetChild(index - 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		
		this.GetParent().MoveChild(this, index - 1);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Toggles the widget.</summary>
	/// <param name="viewType">The view to toggle to.</param>
	/// <param name="on">Set to true to toggle the widget on.</param>
	/// <param name="transition">The transition to transition the widget.</param>
	internal void Toggle(ViewType viewType, bool on, UITransition transition = null)
	{
		if(this.IsOn != on || (transition != null && transition.ShouldReset))
		{
			this.IsOn = on;
			this.ViewType = viewType;
			if(this.IsOn && transition != null && transition.ShouldBeBroughtToFront)
			{
				this.BringToFront();
			}
			if(this.fadeTransition.IsValid)
			{
				Timing.KillCoroutines(this.fadeTransition);
				this.fadeTransition = default;
			}
			if(transition == null || transition.TransitionDuration <= 0.0f)
			{
				this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
				this.SetActive(this.IsOn);
				this.CallToggledEvents();
			}
			else
			{
				this.fadeTransition = Timing.RunCoroutine(this.Transition(transition));
			}
			this.OnToggle(this.ViewType);
		}
		else if(this.ViewType != viewType)
		{
			this.ViewType = viewType;
			this.ChangeView(this.ViewType);
		}
	}
	
	/// <summary>Transitions the widget.</summary>
	/// <param name="transition">The transition data used to transition the widget with.</param>
	/// <returns>Returns the enumerator used for coroutines.</returns>
	private IEnumerator<double> Transition(UITransition transition)
	{
		float time = 0.0f;
		float duration = transition.UseAsyncFades
			? this.IsOn
				? transition.TransitionDuration
				: transition.PreviousTransitionDuration
			: transition.TransitionDuration;
		object from = transition.GetStartingData(this);
		object to = transition.GetEndingData(this);
		
		if(!this.IsOn)
		{
			this.SetActive(false);
		}
		
		yield return Timing.WaitForOneFrame;
		while(time <= duration)
		{
			time += (float)Timing.DeltaTime;
			transition.Update(this, from, to, time);
			yield return Timing.WaitForOneFrame;
		}
		this.SetActive(this.IsOn);
		this.CallToggledEvents();
		Timing.KillCoroutines(this.fadeTransition);
		this.fadeTransition = default;
	}
	
	/// <inheritdoc/>
	protected override void HideAway()
	{
		this.SetAlpha(this.ShowOnStartup ? 1.0f : 0.0f);
		this.SetActive(this.ShowOnStartup);
		this.IsOn = this.ShowOnStartup;
	}
	
	/// <summary>Call all the toggle events.</summary>
	private void CallToggledEvents()
	{
		this.EmitSignal(SignalName.Toggled, this);
		if(this.IsOn)
		{
			this.EmitSignal(SignalName.ToggledOn, this);
		}
		else
		{
			this.EmitSignal(SignalName.ToggledOff, this);
		}
	}
	
	#endregion // Private Methods
}
