
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to generic parameters</summary>
public class GenericParameterInspection
{
	#region Properties
	
	/// <summary>The unlocalized name of the generic parameter as it would appear in the IL code</summary>
	public string UnlocalizedName { get; set; } = "";
	
	/// <summary>The name of the generic parameter</summary>
	public string Name { get; set; } = "";
	
	/// <summary>The list of constraints of what type the generic parameter should be</summary>
	public List<QuickTypeInspection> Constraints { get; set; } = new List<QuickTypeInspection>();
	
	/// <summary>A constructor meant for the class to be filled out later.</summary>
	public GenericParameterInspection() {}
	
	/// <summary>A constructor to store the data for the generic parameter</summary>
	public GenericParameterInspection(GenericParameter parameter)
	{
		this.UnlocalizedName = InspectorUtility.UnlocalizeName(parameter.Name);
		this.Name = InspectorUtility.MakeNameFriendly(parameter.Name);
		foreach(GenericParameterConstraint constraint in parameter.Constraints)
		{
			this.Constraints.Add(new QuickTypeInspection(constraint.ConstraintType));
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of generic parameter informations from the given collection generic parameter</summary>
	/// <param name="generics">The collection of generic parameters to look into</param>
	/// <returns>Returns an array of generic parameter informations</returns>
	public static List<GenericParameterInspection> CreateArray(Collection<GenericParameter> generics)
	{
		List<GenericParameterInspection> array = new List<GenericParameterInspection>();
		
		foreach(GenericParameter parameter in generics)
		{
			array.Add(new GenericParameterInspection(parameter));
		}
		
		return array;
	}
	
	#endregion // Public Methods
}
