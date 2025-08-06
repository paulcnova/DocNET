
namespace Nova;

using Godot;

using System.Diagnostics;

/// <summary>A helper class that helps with general Godot stuff but expanded.</summary>
public static class GDX
{
	#region Properties
	
	/// <summary>Gets and sets the focused control.</summary>
	/// <remarks>This is helpful to disable any in-game input if the player is focused on a GUI item such as a text box.</remarks>
	public static Control FocusedControl { get; set; }
	
	/// <summary>Gets if there is a control that has any focus on it.</summary>
	public static bool ControlHasFocus => FocusedControl != null;
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Instantiates a scene from the given path.</summary>
	/// <param name="path">The path to load the scene from.</param>
	/// <typeparam name="T">The type to return the instantiated scene as.</typeparam>
	/// <returns>Returns the instantiated scene or null.</returns>
	public static T Instantiate<T>(string path) where T : Node
		=> ResourceLoader.Exists(path)
			? (GD.Load<PackedScene>(path)?.InstantiateOrNull<T>() ?? null)
			: null;
	
	/// <summary>Instantiated a scene from the given packed scene.</summary>
	/// <param name="scene">The packed scene to instantiate.</param>
	/// <typeparam name="T">The type to return the instantiated scene as.</typeparam>
	/// <returns>Returns the instantiated scene or null.</returns>
	public static T Instantiate<T>(PackedScene scene) where T : Node
		=> scene != null
			? scene.InstantiateOrNull<T>()
			: null;
	
	/// <summary>Prints the objects with a timestamp.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void Print(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join("", objs);
		
		GD.PrintRich(content);
	}
	
	/// <summary>Prints the objects with a timestamp and a space in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintS(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join(' ', objs);
		
		GD.PrintRich(content);
	}
	
	/// <summary>Prints the objects with a timestamp and a tab in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintT(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join('\t', objs);
		
		GD.PrintRich(content);
	}
	
	/// <summary>Prints a warning of the objects with a location of where the error occurred.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintWarning(params object[] objs)
	{
		string content = string.Join("", objs);
		
		GD.PrintErr(GetFileHint() + ": " + content);
		GD.PushWarning(content);
	}
	
	/// <summary>Prints a warning of the objects with a location of where the error occurred and a space in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintWarningS(params object[] objs)
	{
		string content = string.Join(' ', objs);
		
		GD.PrintErr(GetFileHint() + ": " + content);
		GD.PushWarning(content);
	}
	
	/// <summary>Prints a warning of the objects with a location of where the error occurred and a tab in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintWarningT(params object[] objs)
	{
		string content = string.Join('\t', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushWarning(content);
	}
	
	/// <summary>Prints an error of the objects with a location of where the error occurred.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintError(params object[] objs)
	{
		string content = string.Join("", objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	/// <summary>Prints an error of the objects with a location of where the error occurred and a space in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintErrorS(params object[] objs)
	{
		string content = string.Join(' ', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	/// <summary>Prints an error of the objects with a location of where the error occurred and a tab in between each object.</summary>
	/// <param name="objs">The list of objects to print.</param>
	public static void PrintErrorT(params object[] objs)
	{
		string content = string.Join('\t', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the current time as a string.</summary>
	/// <param name="level">The level of verbosity for the printer.</param>
	/// <returns>Returns the timestamp with a tooltip to the file that called the print.</returns>
	private static string GetTimestamp(char level)
	{
		System.TimeSpan time = System.DateTime.Now.TimeOfDay;
		bool isPM = time.Hours >= 12;
		string hours = time.Hours == 12 || time.Hours == 0
			? "12"
			: (time.Hours % 12).ToString().PadLeft(2, '0');
		string minutes = time.Minutes.ToString().PadLeft(2, '0');
		string timeStr = $"{hours}:{minutes} {(isPM ? "PM" : "AM")}";
		string hint = GetFileLink();
		
		return $"[hint=\"{hint}\"]{level} [{timeStr}][/hint]: ";
	}
	
	/// <summary>Gets the file hint.</summary>
	/// <param name="frameIndex">The frame index to go back to in the stack trace.</param>
	/// <returns>Returns the file hint.</returns>
	private static string GetFileHint(int frameIndex = 3)
	{
		StackTrace trace = new StackTrace(true);
		StackFrame frame = trace.GetFrame(frameIndex);
		
		if(frame == null) { return ""; }
		
		System.Reflection.MethodBase method = frame.GetMethod();
		
		if(method == null) { return ""; }
		
		return $"{method.DeclaringType?.Name}.{method.Name}";
	}
	
	/// <summary>Gets the file link.</summary>
	/// <param name="frameIndex">The frame index to go back to in the stack trace.</param>
	/// <returns>Returns the file link.</returns>
	private static string GetFileLink(int frameIndex = 3)
	{
		StackTrace trace = new StackTrace(true);
		StackFrame frame = trace.GetFrame(frameIndex);
		
		if(frame == null) { return ""; }
		
		System.Reflection.MethodBase method = frame.GetMethod();
		
		if(method == null) { return ""; }
		
		return $"{ProjectSettings.LocalizePath(frame.GetFileName())}:{frame.GetFileLineNumber()}";
	}
	
	#endregion // Private Methods
}
