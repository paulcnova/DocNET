
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;

using System.Collections.Generic;

/// <summary>A quick look into the information of the type</summary>
public partial class QuickTypeInspection
{
	#region Properties
	
	/// <summary>The name of the type as found within the library's IL code</summary>
	/// <remarks>The character ` means that it has generic parameters</remarks>
	public string UnlocalizedName { get; set; } = "";
	
	/// <summary>The name of the type that is slightly localized but not generically instanced</summary>
	public string NonInstancedFullName { get; set; } = "";
	
	/// <summary>The name of the type as found when looking at the code</summary>
	/// <remarks>
	/// If there are any generic parameters, it will display it as a developer would declare it
	/// </remarks>
	public string Name { get; set; } = "";
	
	/// <summary>
	/// The full name of the type as found when looking at the code.
	/// Includes the namespace and the name within this variable
	/// </summary>
	public string FullName { get; set; } = "";
	
	/// <summary>The name of the namespace where the type is located in</summary>
	public string NamespaceName { get; set; } = "";
	
	/// <summary>The list of generic parameters that the type contains</summary>
	public List<GenericParameterInspection> GenericParameters { get; set; } = new List<GenericParameterInspection>();
	
	/// <summary>Set to true if the type is a generic type</summary>
	public bool IsGenericType { get; set; } = false;
	
	/// <summary>A constructor that doesn't generate anything and is meant to be a dud</summary>
	public QuickTypeInspection() {}
	
	/// <summary>A constructor that generates the information for a quick look into the type</summary>
	/// <param name="type">The type definition to look into</param>
	public QuickTypeInspection(TypeDefinition type)
	{
		List<string> generics = Utility.GetGenericParametersString(type.GenericParameters);
		
		this.FillNames(type.FullName.Replace("&", ""), Utility.GetNamespace(type), generics);
		this.GenericParameters = GenericParameterInspection.CreateArray(type.GenericParameters);
		if(this.GenericParameters.Count == 0)
		{
			this.GenericParameters = Utility.GetGenericParameters(type.FullName);
		}
		this.IsGenericType = type.IsGenericParameter && this.UnlocalizedName == this.NonInstancedFullName;
		
	}
	
	/// <summary>A constructor that generates the information for a quick look into the type</summary>
	/// <param name="type">The type reference to look into</param>
	public QuickTypeInspection(TypeReference type)
	{
		List<string> generics = Utility.GetGenericParametersString(type.GenericParameters);
		
		this.FillNames(type.FullName.Replace("&", ""), Utility.GetNamespace(type), generics);
		this.GenericParameters = GenericParameterInspection.CreateArray(type.GenericParameters);
		if(this.GenericParameters.Count == 0)
		{
			this.GenericParameters = Utility.GetGenericParameters(type.FullName);
		}
		this.IsGenericType = type.IsGenericParameter && this.UnlocalizedName == this.NonInstancedFullName;
		
	}
	
	#endregion // Properties
	
	#region Private Methods
	
	/// <summary>
	/// Gathers all the names for the type information using the type's full
	/// name and namespace
	/// </summary>
	/// <param name="typeFullName">The full name of the type</param>
	/// <param name="typeNamespace">The namespace of the type</param>
	/// <param name="generics">The list of generic strings</param>
	private void FillNames(string typeFullName, string typeNamespace, List<string> generics)
	{
		int index = typeFullName.IndexOf('<');
		
		this.UnlocalizedName = (index == -1
			? typeFullName
			: typeFullName.Substring(0, index)
		).Replace("[]", "");
		this.FullName = InspectionRegex.GenericNotation()
			.Replace(Utility.LocalizeName(typeFullName, generics), "");
		this.Name = Utility.RemoveNamespaceFromType(Utility.MakeNameFriendly(this.FullName));
		this.Name = this.Name.Replace("/", ".");
		this.FullName = this.FullName.Replace("/", ".");
		this.NonInstancedFullName = this.FullName;
		this.NamespaceName = this.UnlocalizedName.Contains('.')
			? InspectionRegex.NamespaceName().Replace(this.UnlocalizedName, "$1")
			: "";
	}
	
	#endregion // Private Methods
}
