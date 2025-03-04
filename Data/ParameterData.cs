
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to parameters</summary>
public class ParameterData
{
	#region Properties
	
	/// <summary>The name of the parameter</summary>
	public string Name { get; set; }
	
	/// <summary>The default value of the parameter (if it exists)</summary>
	public string DefaultValue { get; set; }
	
	/// <summary>The list of attributes that the parameter contains</summary>
	public List<AttributeData> Attributes { get; set; } = new List<AttributeData>();
	
	/// <summary>Any modifiers to the parameter (such as ref, in, out, params, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>Set to true if the parameter is optional and can be left out when calling the method</summary>
	public bool IsOptional { get; set; }
	
	/// <summary>The information of the parameter's type</summary>
	public QuickTypeData TypeInfo { get; set; }
	
	/// <summary>The list of types used for the generic parameters</summary>
	public List<string> GenericParameterDeclarations { get; set; } = new List<string>();
	
	/// <summary>The full declaration of the parameter as it would be found within the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>A constructor that generates the information for the parameter given the parameter definition</summary>
	/// <param name="parameter">The parameter definition to look into</param>
	/// <returns>Returns the parameter information generated from the parameter definition</returns>
	public ParameterData(ParameterDefinition parameter)
	{
		this.Name = parameter.Name;
		this.TypeInfo = new QuickTypeData(parameter.ParameterType);
		this.Attributes = AttributeData.CreateArray(parameter.CustomAttributes);
		
		if(parameter.IsIn) { this.Modifier = "in"; }
		else if(parameter.IsOut) { this.Modifier = "out"; }
		else if(parameter.ParameterType.IsByReference) { this.Modifier = "ref"; }
		else if(this.HasParamsAttribute(this.Attributes)) { this.Modifier = "params"; }
		else { this.Modifier = ""; }
		
		this.IsOptional = parameter.IsOptional;
		this.DefaultValue = $"{parameter.Constant}";
		this.GenericParameterDeclarations = Utility.GetGenericParametersAsStrings(parameter.ParameterType.FullName);
		this.FullDeclaration = this.GetFullDeclaration();
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of parameter informations from the given collection of parameter definition</summary>
	/// <param name="parameters">The collection of parameters to look into</param>
	/// <returns>Returns the array of parameter informations generated from the collection of parameter definitions</returns>
	public static List<ParameterData> CreateArray(Collection<ParameterDefinition> parameters)
	{
		List<ParameterData> results = new List<ParameterData>();
		
		foreach(ParameterDefinition parameter in parameters)
		{
			results.Add(new ParameterData(parameter));
		}
		
		return results;
	}
	
	/// <summary>Gets the full declaration of the parameter</summary>
	/// <returns>Returns the full declaration of the parameter</returns>
	public string GetFullDeclaration()
	{
		string decl = this.TypeInfo.Name;
		
		if(this.Modifier != "")
		{
			decl = $"{this.Modifier} {decl}";
		}
		decl += $" {this.Name}";
		if(this.DefaultValue != "")
		{
			if(this.TypeInfo.Name == "string")
			{
				decl += $@" = ""{this.DefaultValue}""";
			}
			else
			{
				decl += $" = {this.DefaultValue}";
			}
		}
		
		return decl;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Finds if the parameter has the params attribute (meaning that the parameter is a "params type[] name" kind of parameter)</summary>
	/// <param name="attrs">The list of attributes the parameter has</param>
	/// <returns>Returns true if the parameter contains the params attribute</returns>
	private bool HasParamsAttribute(List<AttributeData> attrs)
	{
		foreach(AttributeData attr in attrs)
		{
			if(attr.TypeInfo.UnlocalizedName == "System.ParamArrayAttribute")
			{
				return true;
			}
		}
		
		return false;
	}
	
	#endregion // Private Methods
}
