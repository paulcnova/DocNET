
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to the property</summary>
public class PropertyData : BaseData
{
	#region Properties
	
	/// <summary>The name of the property</summary>
	public string Name { get; set; }
	
	/// <summary>Set to true if the property is static</summary>
	public bool IsStatic { get; set; }
	
	/// <summary>Set to true if the property has a getter method</summary>
	public bool HasGetter { get; set; }
	
	/// <summary>Set to true if the property has a setter method</summary>
	public bool HasSetter { get; set; }
	
	/// <summary>The list of attributes associated with the property</summary>
	public List<AttributeData> Attributes { get; set; } = new List<AttributeData>();
	
	/// <summary>The accessor of the property (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Any modifiers to the property (such as static, virtual, override, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>The information of the property's type</summary>
	public QuickTypeData TypeInfo { get; set; }
	
	/// <summary>The information of where the property was implemented</summary>
	public QuickTypeData ImplementedType { get; set; }
	
	/// <summary>The parameters the property has (if any)</summary>
	public List<ParameterData> Parameters { get; set; } = new List<ParameterData>();
	
	/// <summary>The getter method of the property (this can be null, you must check the hasGetter variable)</summary>
	public MethodData Getter { get; set; }
	
	/// <summary>The setter method of the property (this can be null, you must check the hasSetter variable)</summary>
	public MethodData Setter { get; set; }
	
	/// <summary>The partial declaration of the property as can be found in the code</summary>
	public string Declaration { get; set; }
	
	/// <summary>The partial declaration of the property's parameters (if any) as can be found in the code</summary>
	public string ParameterDeclaration { get; set; }
	
	/// <summary>The partial declaration of the property that determines the accessibility of the get and set methods as can be found in the code</summary>
	public string GetSetDeclaration { get; set; }
	
	/// <summary>The full declaration of the property as can be found in the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>Set to true to delete the property when able to.</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	// Used to find duplicates
	private string PartialFullName { get; set; }
	
	public PropertyData(PropertyDefinition property, bool ignorePrivate = true)
	{
		this.HasGetter = property.GetMethod != null;
		this.HasSetter = property.SetMethod != null;
		this.Getter = this.HasGetter
			? new MethodData(property.GetMethod)
			: null;
		this.Setter = this.HasSetter
			? new MethodData(property.SetMethod)
			: null;
		if(this.Getter != null && Utility.GetAccessorId(this.Getter.Accessor, true) == 0)
		{
			this.Getter = null;
			this.HasGetter = false;
		}
		if(this.Setter != null && Utility.GetAccessorId(this.Setter.Accessor, ignorePrivate) == 0)
		{
			this.Setter = null;
			this.HasSetter = false;
		}
		if(!this.HasGetter && !this.HasSetter)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.Name = property.Name;
		this.PartialFullName = property.FullName.Split("::")[1].Replace(",", ", ");
		this.IsStatic = !property.HasThis;
		this.Attributes = AttributeData.CreateArray(property.CustomAttributes);
		this.Parameters = ParameterData.CreateArray(property.Parameters);
		this.Accessor = this.GetAccessor();
		this.TypeInfo = new QuickTypeData(property.PropertyType);
		
		if(!property.HasThis) { this.Modifier = "static"; }
		else { this.Modifier = this.GetModifier(); }
		
		this.ImplementedType = new QuickTypeData(property.DeclaringType);
		this.GetSetDeclaration = this.GetGetSetDeclaration(ignorePrivate);
		this.Declaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{this.TypeInfo.Name} {(this.Parameters.Count == 0 ? this.Name : "this")}";
		this.ParameterDeclaration = string.Join(", ", this.GetParameterDeclaration());
		this.FullDeclaration = $"{this.Declaration}{(
			this.ParameterDeclaration != ""
				? $"[{this.ParameterDeclaration}]"
				: ""
		)} {{ {this.GetSetDeclaration}}}";
	}
	
	public PropertyData(PropertyDefinition property, TypeDefinition type, TypeDefinition boundType, TypeReference boundTypeRef, bool ignorePrivate = true)
	{
		
		this.HasGetter = property.GetMethod != null;
		this.HasSetter = property.SetMethod != null;
		this.Getter = this.HasGetter
			? new MethodData(type, boundType, boundTypeRef, property.GetMethod)
			: null;
		this.Setter = this.HasSetter
			? new MethodData(type, boundType, boundTypeRef, property.SetMethod)
			: null;
		if(this.Getter != null && Utility.GetAccessorId(this.Getter.Accessor, true) == 0)
		{
			this.Getter = null;
			this.HasGetter = false;
		}
		if(this.Setter != null && Utility.GetAccessorId(this.Setter.Accessor, ignorePrivate) == 0)
		{
			this.Setter = null;
			this.HasSetter = false;
		}
		if(!this.HasGetter && !this.HasSetter)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.Name = property.Name;
		this.PartialFullName = property.FullName.Split("::")[1].Replace(",", ", ");
		this.IsStatic = !property.HasThis;
		this.Attributes = AttributeData.CreateArray(property.CustomAttributes);
		this.Parameters = ParameterData.CreateArray(property.Parameters);
		this.Accessor = this.GetAccessor();
		
		if(this.HasGetter)
		{
			this.TypeInfo = this.Getter.ReturnType;
			this.Parameters = this.Getter.Parameters;
		}
		else
		{
			this.TypeInfo = this.Setter.Parameters[this.Setter.Parameters.Count - 1].TypeInfo;
			this.Parameters = new List<ParameterData>(this.Setter.Parameters);
		}
		
		if(!property.HasThis) { this.Modifier = "static"; }
		else { this.Modifier = this.GetModifier(); }
		
		this.ImplementedType = new QuickTypeData(property.DeclaringType);
		this.GetSetDeclaration = this.GetGetSetDeclaration(ignorePrivate);
		this.Declaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{this.TypeInfo.Name} {(this.Parameters.Count == 0 ? this.Name : "this")}";
		this.ParameterDeclaration = string.Join(", ", this.GetParameterDeclaration());
		this.FullDeclaration = $"{this.Declaration}{(
			this.ParameterDeclaration != ""
				? $"[{this.ParameterDeclaration}]"
				: ""
		)} {{ {this.GetSetDeclaration}}}";
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of property informations from the given type and booleans</summary>
	/// <param name="type">The type to gather information from</param>
	/// <param name="recursive">Set to true to recursively look through base types</param>
	/// <param name="isStatic">Set to true to record only static members</param>
	/// <returns>Returns an array of property informations</returns>
	public static List<PropertyData> CreateArray(TypeDefinition type, bool recursive, bool isStatic, bool ignorePrivate = true)
	{
		if(!recursive)
		{
			List<PropertyData> results = CreateArray(type.Properties, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, true);
			
			return results;
		}
		
		List<PropertyData> properties = new List<PropertyData>();
		List<PropertyData> temp = new List<PropertyData>();
		TypeDefinition currType = type;
		TypeReference currTypeRef = type.Resolve();
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			if(currTypeRef.IsGenericInstance)
			{
				temp = CreateArray(currType.Properties, type, currType, currTypeRef, ignorePrivate);
			}
			else
			{
				temp = CreateArray(currType.Properties, ignorePrivate);
			}
			RemoveUnwanted(temp, isStatic, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(temp, properties);
			}
			properties.AddRange(temp);
			baseType = currType.BaseType;
			
			if(baseType == null) { break; }
			
			currTypeRef = baseType;
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return properties;
	}
	
	/// <summary>Generates an array of property informations from the given collection of property definitions</summary>
	/// <param name="properties">The collection of property definitions</param>
	/// <returns>Returns an array of property informations</returns>
	public static List<PropertyData> CreateArray(Collection<PropertyDefinition> properties, bool ignorePrivate = true)
	{
		List<PropertyData> results = new List<PropertyData>();
		
		foreach(PropertyDefinition property in properties)
		{
			PropertyData info = new PropertyData(property, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			
			results.Add(info);
		}
		
		return results;
	}
	
	public static List<PropertyData> CreateArray(Collection<PropertyDefinition> properties, TypeDefinition type, TypeDefinition boundType, TypeReference boundTypeRef, bool ignorePrivate = true)
	{
		List<PropertyData> results = new List<PropertyData>();
		
		foreach(PropertyDefinition property in properties)
		{
			PropertyData info = new PropertyData(property, type, boundType, boundTypeRef, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			
			results.Add(info);
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the get / set declaration of the property</summary>
	/// <param name="getter">The getter method (can be null)</param>
	/// <param name="setter">The setter method (can be null)</param>
	/// <param name="accessor">The accessor of the property (should be highest visible accessor)</param>
	/// <returns>Returns the get / set declaration of the property</returns>
	private string GetGetSetDeclaration(bool ignorePrivate)
	{
		int infoId = Utility.GetAccessorId(this.Accessor, ignorePrivate);
		int getterId = this.Getter != null
			? Utility.GetAccessorId(this.Getter.Accessor, ignorePrivate)
			: 0;
		int setterId = this.Setter != null
			? Utility.GetAccessorId(this.Setter.Accessor, ignorePrivate)
			: 0;
		string declaration = "";
		
		if(getterId >= infoId) { declaration = "get;"; }
		else if(getterId > 0) { declaration = $"{Utility.GetAccessorFromId(getterId)} get;"; }
		
		if(setterId >= infoId)
		{
			declaration += $"{(declaration != "" ? " " : "")}set;";
		}
		else if(setterId > 0)
		{
			declaration += $"{(declaration != "" ? " " : "")}{Utility.GetAccessorFromId(setterId)} set;";
		}
		
		return declaration;
	}
	
	/// <summary>Removes any unwanted elements within the array of property informations</summary>
	/// <param name="properties">The array of property informations to remove from</param>
	/// <param name="isStatic">Set to true to remove all non-static members</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(List<PropertyData> properties, bool isStatic, bool isOriginal)
	{
		for(int i = properties.Count - 1; i >= 0; i--)
		{
			if(properties[i].ShouldIgnore)
			{
				properties.RemoveAt(i);
			}
			else if(properties[i].IsStatic != isStatic)
			{
				properties.RemoveAt(i);
			}
			else if(!isOriginal && properties[i].Accessor == "private")
			{
				properties.RemoveAt(i);
			}
		}
	}
	
	/// <summary>Removes any duplicates from the array of property informations</summary>
	/// <param name="properties">The array of property informations to remove from</param>
	/// <param name="listProperties">The list of properties recursive-ordered to reference if there are any duplicates</param>
	private static void RemoveDuplicates(List<PropertyData> properties, List<PropertyData> listProperties)
	{
		for(int i = properties.Count - 1; i >= 0; i--)
		{
			foreach(PropertyData property in listProperties)
			{
				if(properties[i].PartialFullName == property.PartialFullName)
				{
					properties.RemoveAt(i);
					break;
				}
			}
		}
	}
	
	/// <summary>Gets the accessor from the getter or setter methods</summary>
	/// <returns>Returns the accessor</returns>
	private string GetAccessor()
	{
		int getterId = this.Getter != null
			? Utility.GetAccessorId(this.Getter.Accessor, false)
			: 0;
		int setterId = this.Setter != null
			? Utility.GetAccessorId(this.Setter.Accessor, false)
			: 0;
		
		return Utility.GetAccessorFromId(System.Math.Max(getterId, setterId));
	}
	
	/// <summary>Gets the modifier of the property from either the getter or the setting</summary>
	/// <returns>Returns the modifier of the property</returns>
	private string GetModifier()
	{
		int getterId = this.Getter != null
			? Utility.GetAccessorId(this.Getter.Accessor, false)
			: 0;
		int setterId = this.Setter != null
			? Utility.GetAccessorId(this.Setter.Accessor, false)
			: 0;
		
		if(getterId != 0 && getterId >= setterId) { return this.Getter.Modifier; }
		else if(setterId != 0 && setterId >= getterId) { return this.Setter.Modifier; }
		
		return "";
	}
	
	/// <summary>Gets an array of parameter declarations</summary>
	/// <returns>Returns the array of parameter declarations</returns>
	private List<string> GetParameterDeclaration()
	{
		List<string> declarations = new List<string>();
		
		foreach(ParameterData parameter in this.Parameters)
		{
			declarations.Add(parameter.FullDeclaration);
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
}
