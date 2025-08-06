
namespace Nova.UI;

using Godot;

using System.Collections.Generic;

/// <summary>A control that shows UI and only one page is meant to be shown at a time.</summary>
/// <remarks>This is useful for things like a HUD or Menu</remarks>
public abstract partial class Page : UIControl
{
	#region Properties
	
	/// <summary>The coroutine handle for the fade transition.</summary>
	private CoroutineHandle fadeTransition;
	
	/// <summary>An event for when the page gets toggled.</summary>
	/// <param name="page">The page in question.</param>
	[Signal] public delegate void ToggledEventHandler(Page page);
	
	/// <summary>An event for when the page gets toggled on.</summary>
	/// <param name="page">The page in question.</param>
	[Signal] public delegate void ToggledOnEventHandler(Page page);
	
	/// <summary>An event for when the page gets toggled off.</summary>
	/// <param name="page">The page in question.</param>
	[Signal] public delegate void ToggledOffEventHandler(Page page);
	
	#endregion // Properties
	
	#region Private Methods
	
	/// <summary>Toggles the page.</summary>
	/// <param name="viewType">The view to toggle to.</param>
	/// <param name="on">Set to true to toggle the page on.</param>
	/// <param name="transition">The transition to transition between the two pages.</param>
	internal void Toggle(ViewType viewType, bool on, UITransition transition = null)
	{
		if(this.IsOn != on)
		{
			this.IsOn = on;
			this.ViewType = viewType;
			if(transition == null || (
				transition.UseAsyncFades
					? ((this.IsOn && transition.TransitionDuration <= 0.0f) || (!this.IsOn && transition.PreviousTransitionDuration <= 0.0f))
					: transition.TransitionDuration <= 0.0f
			))
			{
				this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
				this.SetActive(this.IsOn);
				this.CallToggledEvents();
			}
			else
			{
				if(this.fadeTransition.IsValid)
				{
					Timing.KillCoroutines(this.fadeTransition);
					this.fadeTransition = default;
				}
				if(transition.UseAsyncFades)
				{
					if(this.IsOn)
					{
						this.BringToFront();
					}
				}
				this.fadeTransition = Timing.RunCoroutine(this.Transition(transition));
			}
			
			if(this.IsOn)
			{
				this.OnEnable();
				this.OnToggle(this.ViewType);
			}
			else
			{
				this.OnDisable();
			}
		}
		else if(this.ViewType != viewType)
		{
			this.ViewType = viewType;
			this.ChangeView(this.ViewType);
		}
	}
	
	/// <summary>Transitions the page.</summary>
	/// <param name="transition">The transition data used to transition the page with.</param>
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
			transition.Update(this, from, to, Mathf.Clamp(time / duration, 0.0f, 1.0f));
			yield return Timing.WaitForOneFrame;
		}
		this.SetActive(this.IsOn);
		this.CallToggledEvents();
		Timing.KillCoroutines(this.fadeTransition);
		this.fadeTransition = default;
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
