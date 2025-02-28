
namespace DocNET.Inspector;

using Mono.Cecil;
using Mono.Collections.Generic;

/// <summary>All the information relevant to generic parameters</summary>
public class GenericParametersInfo
{
	#region Properties
	
	/// <summary>The unlocalized name of the generic parameter as it would appear in the IL code</summary>
	public string UnlocalizedName { get; set; }
	/// <summary>The name of the generic parameter</summary>
	public string Name { get; set; }
	/// <summary>The list of constraints of what type the generic parameter should be</summary>
	public QuickTypeInfo[] Constraints { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of generic parameter informations from the given collection generic parameter</summary>
	/// <param name="generics">The collection of generic parameters to look into</param>
	/// <returns>Returns an array of generic parameter informations</returns>
	public static GenericParametersInfo[] GenerateInfoArray(Collection<GenericParameter> generics)
	{
		GenericParametersInfo[] results = new GenericParametersInfo[generics.Count];
		int i = 0;
		
		foreach(GenericParameter generic in generics)
		{
			results[i++] = GenerateInfo(generic);
		}
		
		return results;
	}
	
	/// <summary>Generates a generic parameter information of the given generic parameter</summary>
	/// <param name="generic">The generic parameter to look into</param>
	/// <returns>Returns the generic parameter information</returns>
	public static GenericParametersInfo GenerateInfo(GenericParameter generic)
	{
		GenericParametersInfo info = new GenericParametersInfo();
		int i = 0;
		
		info.UnlocalizedName = UnlocalizeName(generic.Name);
		info.Name = QuickTypeInfo.MakeNameFriendly(generic.Name);
		info.Constraints = new QuickTypeInfo[generic.Constraints.Count];
		foreach(GenericParameterConstraint constraint in generic.Constraints)
		{
			info.Constraints[i++] = QuickTypeInfo.GenerateInfo(constraint.ConstraintType);
		}
		
		return info;
	}
	
	/// <summary>Unlocalizes the name to look like what the IL code would look like</summary>
	/// <param name="name">The name to unlocalize</param>
	/// <returns>Returns the unlocalized name of the type</returns>
	public static string UnlocalizeName(string name)
	{
		int lt = name.IndexOf('<');
		
		if(lt == -1) { return name; }
		
		int gt = name.LastIndexOf('>');
		int scope = 0;
		int count = 1;
		
		for(int i = lt + 1; i < gt; i++)
		{
			if(name[i] == '<') { scope++; }
			else if(name[i] == '>') { scope--; }
			else if(name[i] == ',' && scope == 0) { count++; }
		}
		
		return $"{ name.Substring(0, lt) }`{ count }";
	}
	
	#endregion // Public Methods
}
