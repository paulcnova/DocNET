
namespace Taco.DocNET.Inspector;

/// <summary>The base information used for other info inspector types</summary>
public abstract class BaseInfo : System.IComparable
{
	#region Public Methods
	
	/// <summary>Compares the info with other infos for sorting</summary>
	/// <param name="other">The other object to look into</param>
	/// <returns>Returns a number that finds if it should be shifted or not (-1 and 0 for no shift; 1 for shift)</returns>
	public int CompareTo(object other)
	{
		if(other is FieldInfo)
		{
			return (this as FieldInfo).Name.CompareTo((other as FieldInfo).Name);
		}
		if(other is PropertyInfo)
		{
			return (this as PropertyInfo).Name.CompareTo((other as PropertyInfo).Name);
		}
		if(other is MethodInfo)
		{
			return (this as MethodInfo).Name.CompareTo((other as MethodInfo).Name);
		}
		if(other is EventInfo)
		{
			return (this as EventInfo).Name.CompareTo((other as EventInfo).Name);
		}
		return 0;
	}
	
	#endregion // Public Methods
}
