
namespace DocNET.Inspector;

using Mono.Cecil;
using Mono.Collections.Generic;

/// <summary>All the information relevant to an attribute</summary>
public class AttributeInfo
{
	#region Properties
	
	/// <summary>Gets and sets the information of the type that the attribute is</summary>
	public QuickTypeInfo TypeInfo { get; set; }
	/// <summary>Gets and sets the list of constructor arguments that the attribute is declaring</summary>
	public AttributeFieldInfo[] ConstructorArgs { get; set; }
	/// <summary>Gets and sets the list of fields and properties that the attribute is declaring</summary>
	public AttributeFieldInfo[] Properties { get; set; }
	/// <summary>Gets and sets the declaration of parameters as seen if looking at the code</summary>
	public string ParameterDeclaration { get; set; }
	/// <summary>
	/// Gets and sets the declaration of the attribute as a whole, with name and parameters as seen
	/// if looking at the code
	/// </summary>
	public string FullDeclaration { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of information for all the attributes given a collection of custom attributes</summary>
	/// <param name="attrs">The collection of custom attributes to gather all the information from</param>
	/// <returns>
	/// Returns the array of attribute information generated from the collection of custom attributes
	/// </returns>
	public static AttributeInfo[] GenerateInfoArray(Collection<CustomAttribute> attrs)
	{
		AttributeInfo[] results = new AttributeInfo[attrs.Count];
		int i = 0;
		
		foreach(CustomAttribute attr in attrs)
		{
			results[i++] = GenerateInfo(attr);
		}
		
		return results;
	}
	
	/// <summary>Generates the information for an attribute from the given Mono.Cecil custom attribute class</summary>
	/// <param name="attr">The attribute to gather the information from</param>
	/// <returns>Returns the attribute information generated from the custom attribute</returns>
	public static AttributeInfo GenerateInfo(CustomAttribute attr)
	{
		AttributeInfo info = new AttributeInfo();
		int i = 0;
		
		info.TypeInfo = QuickTypeInfo.GenerateInfo(attr.AttributeType);
		info.ConstructorArgs = new AttributeFieldInfo[attr.ConstructorArguments.Count];
		foreach(CustomAttributeArgument arg in attr.ConstructorArguments)
		{
			info.ConstructorArgs[i] = new AttributeFieldInfo();
			info.ConstructorArgs[i].typeInfo = QuickTypeInfo.GenerateInfo(arg.Type);
			info.ConstructorArgs[i].name = attr.Constructor.Parameters[i].Name;
			info.ConstructorArgs[i].value = (info.ConstructorArgs[i].typeInfo.Name != "bool"
				? $"{ arg.Value }"
				: $"{ arg.Value }".ToLower()
			);
			i++;
		}
		i = 0;
		info.Properties = new AttributeFieldInfo[
			attr.Fields.Count +
			attr.Properties.Count
		];
		foreach(CustomAttributeNamedArgument field in attr.Fields)
		{
			info.Properties[i] = new AttributeFieldInfo();
			info.Properties[i].typeInfo = QuickTypeInfo.GenerateInfo(field.Argument.Type);
			info.Properties[i].name = field.Name;
			info.Properties[i].value = (info.Properties[i].typeInfo.Name != "bool"
				? $"{ field.Argument.Value }"
				: $"{ field.Argument.Value }".ToLower()
			);
			i++;
		}
		foreach(CustomAttributeNamedArgument property in attr.Properties)
		{
			info.Properties[i] = new AttributeFieldInfo();
			info.Properties[i].typeInfo = QuickTypeInfo.GenerateInfo(property.Argument.Type);
			info.Properties[i].name = property.Name;
			info.Properties[i].value = (info.Properties[i].typeInfo.Name != "bool"
				? $"{ property.Argument.Value }"
				: $"{ property.Argument.Value }".ToLower()
			);
			i++;
		}
		info.ParameterDeclaration = string.Join(", ", GetParameterDeclaration(info));
		info.FullDeclaration = (
			$"[{ info.TypeInfo.FullName }" +
			(info.ParameterDeclaration != "" ? $"({ info.ParameterDeclaration })" : "") +
			"]"
		);
		
		return info;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the parameter declaration string from the given info</summary>
	/// <param name="info">The information used to retrieve the parameter declaration</param>
	/// <returns>Returns the parameter declaration as a string</returns>
	private static string[] GetParameterDeclaration(AttributeInfo info)
	{
		string[] declarations = new string[
			info.ConstructorArgs.Length +
			info.Properties.Length
		];
		int i = 0;
		
		foreach(AttributeFieldInfo field in info.ConstructorArgs)
		{
			declarations[i++] = (field.typeInfo.Name == "string" ? $@"""{ field.value }""" : field.value);
		}
		foreach(AttributeFieldInfo field in info.Properties)
		{
			declarations[i++] = $"{ field.name } = " + (field.typeInfo.Name == "string" ? $@"""{ field.value }""" : field.value);
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
	
	#region Types
	
	/// <summary>All the information relevant to the attribute's fields</summary>
	public class AttributeFieldInfo
	{
		/// <summary>The name of the attribute field</summary>
		public string name;
		/// <summary>The value of the attribute field</summary>
		public string value;
		/// <summary>The information of the attribute field's type</summary>
		public QuickTypeInfo typeInfo;
	}
	
	#endregion // Types
}
