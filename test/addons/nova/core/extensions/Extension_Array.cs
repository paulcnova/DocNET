
namespace Nova;

using Godot;
using Godot.Collections;

using System.Collections.Generic;

/// <summary>An extension for <see cref="Godot.Collections.Array{T}"/> to add helpful methods.</summary>
public static class Extension_Array
{
	#region Public Methods
	
	/// <summary>Converts this array into a static array.</summary>
	/// <param name="arr">The array to convert.</param>
	/// <typeparam name="T">The data type being listed.</typeparam>
	/// <returns>Returns the converted static array.</returns>
	public static T[] ToArray<[MustBeVariant] T>(this Array<T> arr)
	{
		List<T> list = new List<T>(arr);
		
		return list.ToArray();
	}
	
	#endregion // Public Methods
}
