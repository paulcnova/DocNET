
namespace Nova.UI;

using Godot;

/// <summary>A transition that fades the control in/out.</summary>
public class FadeTransition : UITransition
{
	#region Properties
	
	/// <summary>A constructor that fades the control in/out.</summary>
	/// <param name="fade">The duration of the fade.</param>
	public FadeTransition(float fade)
	{
		this.TransitionDuration = fade;
		this.PreviousTransitionDuration = fade;
	}
	
	/// <summary>A constructor that fades the control in and previous control out.</summary>
	/// <param name="fade">The duration of the fade in.</param>
	/// <param name="prevFade">The duration of the fade out.</param>
	public FadeTransition(float fade, float prevFade)
	{
		this.TransitionDuration = fade;
		this.PreviousTransitionDuration = prevFade;
	}
	
	/// <summary>A constructor that resets the <see cref="Widget"/></summary>
	/// <param name="shouldReset">Set to true to reset the <see cref="Widget"/></param>
	public FadeTransition(bool shouldReset) : this()
	{
		this.ShouldReset = shouldReset;
	}
	
	/// <summary>A constructor that creates a default fade transition of 1 second.</summary>
	/// <returns></returns>
	public FadeTransition() : this(1.0f) {}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc/>
	public override object GetStartingData(UIControl control) => control.Modulate.A;
	
	/// <inheritdoc/>
	public override object GetEndingData(UIControl control) => control.IsOn ? 1.0f : 0.0f;
	
	/// <inheritdoc/>
	public override void Update(UIControl control, object from, object to, float t)
	{
		control.Call(UIControl.MethodName.SetAlpha, Mathf.Lerp((float)from, (float)to, t));
	}
	
	#endregion // Public Methods
}
