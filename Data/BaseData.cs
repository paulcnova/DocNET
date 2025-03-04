
namespace DocNET.Inspections;

/// <summary>The base information used for other info inspector types</summary>
public abstract class BaseData : System.IComparable
{
	#region Public Methods
	
	/// <summary>Compares the info with other infos for sorting</summary>
	/// <param name="other">The other object to look into</param>
	/// <returns>Returns a number that finds if it should be shifted or not (-1 and 0 for no shift; 1 for shift)</returns>
	public int CompareTo(object other)
	{
		if(other is FieldData)
		{
			return (this as FieldData).Name.CompareTo((other as FieldData).Name);
		}
		if(other is PropertyData)
		{
			return (this as PropertyData).Name.CompareTo((other as PropertyData).Name);
		}
		if(other is MethodData)
		{
			return (this as MethodData).Name.CompareTo((other as MethodData).Name);
		}
		if(other is EventData)
		{
			return (this as EventData).Name.CompareTo((other as EventData).Name);
		}
		return 0;
	}
	
	#endregion // Public Methods
}
