
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to an attribute</summary>
public class AttributeInspection
{
	#region Properties
	
	/// <summary>Gets and sets the information of the type that the attribute is</summary>
	public QuickTypeInspection TypeInfo { get; set; }
	
	/// <summary>Gets and sets the list of constructor arguments that the attribute is declaring</summary>
	public List<AttributeFieldData> ConstructorArgs { get; set; } = new List<AttributeFieldData>();
	
	/// <summary>Gets and sets the list of fields and properties that the attribute is declaring</summary>
	public List<AttributeFieldData> Properties { get; set; } = new List<AttributeFieldData>();
	
	/// <summary>Gets and sets the declaration of parameters as seen if looking at the code</summary>
	public string ParameterDeclaration { get; set; }
	
	/// <summary>Gets and sets the declaration of the attribute as a whole, with name and parameters as seen if looking at the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>A constructor that generates data for the attribute</summary>
	public AttributeInspection(CustomAttribute attr)
	{
		int i = 0;
		
		this.TypeInfo = new QuickTypeInspection(attr.AttributeType);
		foreach(CustomAttributeArgument arg in attr.ConstructorArguments)
		{
			AttributeFieldData fieldData = new AttributeFieldData();
			
			fieldData.TypeInfo = new QuickTypeInspection(arg.Type);
			fieldData.Name = attr.Constructor.Parameters[i].Name;
			fieldData.Value = fieldData.TypeInfo.Name != "bool"
				? $"{arg.Value}"
				: $"{arg.Value}".ToLower();
			this.ConstructorArgs.Add(fieldData);
			++i;
		}
		this.Properties = new List<AttributeFieldData>();
		
		foreach(CustomAttributeNamedArgument field in attr.Fields)
		{
			AttributeFieldData fieldData = new AttributeFieldData();
			
			fieldData.TypeInfo = new QuickTypeInspection(field.Argument.Type);
			fieldData.Name = field.Name;
			fieldData.Value = fieldData.TypeInfo.Name != "bool"
				? $"{field.Argument.Value}"
				: $"{field.Argument.Value}".ToLower();
			
			this.Properties.Add(fieldData);
		}
		foreach(CustomAttributeNamedArgument property in attr.Properties)
		{
			AttributeFieldData fieldData = new AttributeFieldData();
			
			fieldData.TypeInfo = new QuickTypeInspection(property.Argument.Type);
			fieldData.Name = property.Name;
			fieldData.Value = fieldData.Name != "bool"
				? $"{property.Argument.Value}"
				: $"{property.Argument.Value}".ToLower();
		}
		this.ParameterDeclaration = string.Join(", ", this.GetParameterDeclaration(this));
		this.FullDeclaration = $"[{this.TypeInfo.FullName}{(
			this.ParameterDeclaration != ""
				? $"({this.ParameterDeclaration})"
				: ""
		)}]";
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of information for all the attributes given a collection of custom attributes</summary>
	/// <param name="attrs">The collection of custom attributes to gather all the information from</param>
	/// <returns>Returns the array of attribute information generated from the collection of custom attributes</returns>
	public static List<AttributeInspection> CreateArray(Collection<CustomAttribute> attrs)
	{
		List<AttributeInspection> results = new List<AttributeInspection>();
		
		foreach(CustomAttribute attr in attrs)
		{
			results.Add(new AttributeInspection(attr));
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the parameter declaration string from the given info</summary>
	/// <param name="info">The information used to retrieve the parameter declaration</param>
	/// <returns>Returns the parameter declaration as a string</returns>
	private List<string> GetParameterDeclaration(AttributeInspection info)
	{
		List<string> declarations = new List<string>();
		
		foreach(AttributeFieldData field in info.ConstructorArgs)
		{
			declarations.Add(field.TypeInfo.Name == "string"
				? $@"""{field.Value}"""
				: field.Value
			);
		}
		foreach(AttributeFieldData field in info.Properties)
		{
			declarations.Add($"{field.Name} = " + (
				field.TypeInfo.Name == "string"
					? $@"""{field.Value}"""
					: field.Value
				)
			);
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
	
	#region Types
	
	/// <summary>All the information relevant to the attribute's fields</summary>
	public class AttributeFieldData
	{
		/// <summary>The name of the attribute field</summary>
		public string Name { get; set; }
		
		/// <summary>The value of the attribute field</summary>
		public string Value { get; set; }
		
		/// <summary>The information of the attribute field's type</summary>
		public QuickTypeInspection TypeInfo { get; set; }
	}
	
	#endregion // Types
}
