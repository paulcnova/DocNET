
namespace Nova.UI;

using Godot;

/// <summary>A class that holds data for a transition for the UI.</summary>
public abstract class UITransition
{
	#region Properties
	
	/// <summary>Gets and sets the duration (in seconds) of the transition.</summary>
	public float TransitionDuration { get; set; } = 0.0f;
	
	/// <summary>Gets and sets the duration (in seconds) of the transition of the <see cref="Page"/> that is being transitioned away.</summary>
	/// <remarks>This is used for <see cref="Page"/>, this does nothing for <see cref="Widget"/></remarks>
	public float PreviousTransitionDuration { get; set; } = 0.0f;
	
	/// <summary>Gets if the transition should use asynchronous durations.</summary>
	public bool UseAsyncFades => Mathf.IsEqualApprox(this.TransitionDuration, this.PreviousTransitionDuration);
	
	/// <summary>Gets and sets if the <see cref="Widget"/> should toggle on even if it's already toggled on.</summary>
	/// <remarks>This is used for <see cref="Widget"/>, this does nothing for <see cref="Page"/></remarks>
	public bool ShouldReset { get; set; } = false;
	
	/// <summary>Gets and sets if the <see cref="Widget"/> should be brought to the front.</summary>
	/// <remarks>This is used for <see cref="Widget"/>, this does nothing for <see cref="Page"/></remarks>
	public bool ShouldBeBroughtToFront { get; set; } = true;
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Gets the starting data of the transition.</summary>
	/// <param name="control">The control used to get the data from.</param>
	/// <returns>Returns the starting data of the transition.</returns>
	/// <remarks>Typically this is the starting location such as 0.0.</remarks>
	public abstract object GetStartingData(UIControl control);
	
	/// <summary>Gets the ending data of the transition</summary>
	/// <param name="control">The control used to get the data from.</param>
	/// <returns>Returns the ending data of the transition.</returns>
	/// <remarks>Typically this is the ending location such as 1.0.</remarks>
	public abstract object GetEndingData(UIControl control);
	
	/// <summary>Updates the transition.</summary>
	/// <param name="control">The control to update.</param>
	/// <param name="from">The starting location to update from.</param>
	/// <param name="to">The ending location to update to.</param>
	/// <param name="t">The time to update at, between 0.0 and 1.0.</param>
	public abstract void Update(UIControl control, object from, object to, float t);
	
	#endregion // Public Methods
}
