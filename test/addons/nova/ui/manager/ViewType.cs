
namespace Nova.UI;

/// <summary>An enumeration for the different types of views that a <see cref="UIControl"/> can hold and a <see cref="UIView"/> can be.</summary>
public enum ViewType
{
	/// <summary>The user uses a keyboard and mouse and the view accommodates that.</summary>
	Keyboard,
	/// <summary>The user uses a gamepad and the view accommodates that.</summary>
	Gamepad,
	/// <summary>The user is on mobile and the view accommodates that.</summary>
	Mobile,
}
