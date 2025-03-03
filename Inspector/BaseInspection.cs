
namespace DocNET.Inspections;

/// <summary>The base information used for other info inspector types</summary>
public abstract class BaseInspection : System.IComparable
{
	#region Public Methods
	
	/// <summary>Compares the info with other infos for sorting</summary>
	/// <param name="other">The other object to look into</param>
	/// <returns>Returns a number that finds if it should be shifted or not (-1 and 0 for no shift; 1 for shift)</returns>
	public int CompareTo(object other)
	{
		if(other is FieldInspection)
		{
			return (this as FieldInspection).Name.CompareTo((other as FieldInspection).Name);
		}
		if(other is PropertyInspection)
		{
			return (this as PropertyInspection).Name.CompareTo((other as PropertyInspection).Name);
		}
		if(other is MethodInspection)
		{
			return (this as MethodInspection).Name.CompareTo((other as MethodInspection).Name);
		}
		if(other is EventInspection)
		{
			return (this as EventInspection).Name.CompareTo((other as EventInspection).Name);
		}
		return 0;
	}
	
	#endregion // Public Methods
}
