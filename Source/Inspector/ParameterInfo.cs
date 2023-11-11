
namespace Taco.DocNET.Inspector;

using Mono.Cecil;
using Mono.Collections.Generic;

/// <summary>All the information relevant to parameters</summary>
public class ParameterInfo
{
	#region Properties
	
	/// <summary>The name of the parameter</summary>
	public string Name { get; set; }
	/// <summary>The default value of the parameter (if it exists)</summary>
	public string DefaultValue { get; set; }
	/// <summary>The list of attributes that the parameter contains</summary>
	public AttributeInfo[] Attributes { get; set; }
	/// <summary>Any modifiers to the parameter (such as ref, in, out, params, etc.)</summary>
	public string Modifier { get; set; }
	/// <summary>Set to true if the parameter is optional and can be left out when calling the method</summary>
	public bool IsOptional { get; set; }
	/// <summary>The information of the parameter's type</summary>
	public QuickTypeInfo TypeInfo { get; set; }
	/// <summary>The list of types used for the generic parameters</summary>
	public string[] GenericParameterDeclarations { get; set; }
	/// <summary>The full declaration of the parameter as it would be found within the code</summary>
	public string FullDeclaration { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of parameter informations from the given collection of parameter definition</summary>
	/// <param name="parameters">The collection of parameters to look into</param>
	/// <returns>Returns the array of parameter informations generated from the collection of parameter definitions</returns>
	public static ParameterInfo[] GenerateInfoArray(Collection<ParameterDefinition> parameters)
	{
		ParameterInfo[] results = new ParameterInfo[parameters.Count];
		int i = 0;
		
		foreach(ParameterDefinition parameter in parameters)
		{
			results[i++] = GenerateInfo(parameter);
		}
		
		return results;
	}
	
	/// <summary>Generates the information for the parameter given the parameter definition</summary>
	/// <param name="parameter">The parameter definition to look into</param>
	/// <returns>Returns the parameter information generated from the parameter definition</returns>
	public static ParameterInfo GenerateInfo(ParameterDefinition parameter)
	{
		ParameterInfo info = new ParameterInfo();
		
		info.Name = parameter.Name;
		info.TypeInfo = QuickTypeInfo.GenerateInfo(parameter.ParameterType);
		info.Attributes = AttributeInfo.GenerateInfoArray(parameter.CustomAttributes);
		
		if(parameter.IsIn) { info.Modifier = "in"; }
		else if(parameter.IsOut) { info.Modifier = "out"; }
		else if(parameter.ParameterType.IsByReference)
		{
			info.Modifier = "ref";
		}
		else if(HasParamsAttribute(info.Attributes))
		{
			info.Modifier = "params";
		}
		else { info.Modifier = ""; }
		info.IsOptional = parameter.IsOptional;
		info.DefaultValue = $"{ parameter.Constant }";
		info.GenericParameterDeclarations = QuickTypeInfo.GetGenericParametersAsStrings(parameter.ParameterType.FullName);
		info.FullDeclaration = GetFullDeclaration(info);
		
		return info;
	}
	
	/// <summary>Gets the full declaration of the parameter</summary>
	/// <param name="parameter">The parameter info to look into</param>
	/// <returns>Returns the full declaration of the parameter</returns>
	public static string GetFullDeclaration(ParameterInfo parameter)
	{
		string decl = parameter.TypeInfo.Name;
		
		if(parameter.Modifier != "")
		{
			decl = parameter.Modifier + " " + decl;
		}
		decl += $" { parameter.Name }";
		if(parameter.DefaultValue != "")
		{
			if(parameter.TypeInfo.Name == "string")
			{
				decl += $@" = ""{ parameter.DefaultValue }""";
			}
			else
			{
				decl += $" = { parameter.DefaultValue }";
			}
		}
		
		return decl;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Finds if the parameter has the params attribute (meaning that the parameter is a "params type[] name" kind of parameter)</summary>
	/// <param name="attrs">The list of attributes the parameter has</param>
	/// <returns>Returns true if the parameter contains the params attribute</returns>
	private static bool HasParamsAttribute(AttributeInfo[] attrs)
	{
		foreach(AttributeInfo attr in attrs)
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
