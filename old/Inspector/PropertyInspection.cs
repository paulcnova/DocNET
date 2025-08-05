
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to the property</summary>
public class PropertyInspection : BaseInspection
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
	public List<AttributeInspection> Attributes { get; set; } = new List<AttributeInspection>();
	
	/// <summary>The accessor of the property (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Any modifiers to the property (such as static, virtual, override, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>The information of the property's type</summary>
	public QuickTypeInspection TypeInfo { get; set; }
	
	/// <summary>The information of where the property was implemented</summary>
	public QuickTypeInspection ImplementedType { get; set; }
	
	/// <summary>The parameters the property has (if any)</summary>
	public List<ParameterInspection> Parameters { get; set; } = new List<ParameterInspection>();
	
	/// <summary>The getter method of the property (this can be null, you must check the hasGetter variable)</summary>
	public MethodInspection Getter { get; set; }
	
	/// <summary>The setter method of the property (this can be null, you must check the hasSetter variable)</summary>
	public MethodInspection Setter { get; set; }
	
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
	
	public PropertyInspection(PropertyDefinition property, bool ignorePrivate = true)
	{
		this.HasGetter = property.GetMethod != null;
		this.HasSetter = property.SetMethod != null;
		this.Getter = this.HasGetter
			? new MethodInspection(property.GetMethod)
			: null;
		this.Setter = this.HasSetter
			? new MethodInspection(property.SetMethod)
			: null;
		if(this.Getter != null && InspectorUtility.GetAccessorId(this.Getter.Accessor, true) == 0)
		{
			this.Getter = null;
			this.HasGetter = false;
		}
		if(this.Setter != null && InspectorUtility.GetAccessorId(this.Setter.Accessor, ignorePrivate) == 0)
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
		this.Attributes = AttributeInspection.CreateArray(property.CustomAttributes);
		this.Parameters = ParameterInspection.CreateArray(property.Parameters);
		this.Accessor = this.GetAccessor();
		this.TypeInfo = new QuickTypeInspection(property.PropertyType);
		
		if(!property.HasThis) { this.Modifier = "static"; }
		else { this.Modifier = this.GetModifier(); }
		
		this.ImplementedType = new QuickTypeInspection(property.DeclaringType);
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
	
	public PropertyInspection(PropertyDefinition property, TypeDefinition type, TypeDefinition boundType, TypeReference boundTypeRef, bool ignorePrivate = true)
	{
		
		this.HasGetter = property.GetMethod != null;
		this.HasSetter = property.SetMethod != null;
		this.Getter = this.HasGetter
			? new MethodInspection(type, boundType, boundTypeRef, property.GetMethod)
			: null;
		this.Setter = this.HasSetter
			? new MethodInspection(type, boundType, boundTypeRef, property.SetMethod)
			: null;
		if(this.Getter != null && InspectorUtility.GetAccessorId(this.Getter.Accessor, true) == 0)
		{
			this.Getter = null;
			this.HasGetter = false;
		}
		if(this.Setter != null && InspectorUtility.GetAccessorId(this.Setter.Accessor, ignorePrivate) == 0)
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
		this.Attributes = AttributeInspection.CreateArray(property.CustomAttributes);
		this.Parameters = ParameterInspection.CreateArray(property.Parameters);
		this.Accessor = this.GetAccessor();
		
		if(this.HasGetter)
		{
			this.TypeInfo = this.Getter.ReturnType;
			this.Parameters = this.Getter.Parameters;
		}
		else
		{
			this.TypeInfo = this.Setter.Parameters[this.Setter.Parameters.Count - 1].TypeInfo;
			this.Parameters = new List<ParameterInspection>(this.Setter.Parameters);
		}
		
		if(!property.HasThis) { this.Modifier = "static"; }
		else { this.Modifier = this.GetModifier(); }
		
		this.ImplementedType = new QuickTypeInspection(property.DeclaringType);
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
	public static List<PropertyInspection> CreateArray(TypeDefinition type, bool recursive, bool isStatic, bool ignorePrivate = true)
	{
		if(!recursive)
		{
			List<PropertyInspection> results = CreateArray(type.Properties, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, true);
			
			return results;
		}
		
		List<PropertyInspection> properties = new List<PropertyInspection>();
		List<PropertyInspection> temp = new List<PropertyInspection>();
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
	public static List<PropertyInspection> CreateArray(Collection<PropertyDefinition> properties, bool ignorePrivate = true)
	{
		List<PropertyInspection> results = new List<PropertyInspection>();
		
		foreach(PropertyDefinition property in properties)
		{
			PropertyInspection info = new PropertyInspection(property, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			
			results.Add(info);
		}
		
		return results;
	}
	
	public static List<PropertyInspection> CreateArray(Collection<PropertyDefinition> properties, TypeDefinition type, TypeDefinition boundType, TypeReference boundTypeRef, bool ignorePrivate = true)
	{
		List<PropertyInspection> results = new List<PropertyInspection>();
		
		foreach(PropertyDefinition property in properties)
		{
			PropertyInspection info = new PropertyInspection(property, type, boundType, boundTypeRef, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			
			results.Add(info);
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the get / set declaration of the property</summary>
	/// <returns>Returns the get / set declaration of the property</returns>
	private string GetGetSetDeclaration(bool ignorePrivate)
	{
		int infoId = InspectorUtility.GetAccessorId(this.Accessor, ignorePrivate);
		int getterId = this.Getter != null
			? InspectorUtility.GetAccessorId(this.Getter.Accessor, ignorePrivate)
			: 0;
		int setterId = this.Setter != null
			? InspectorUtility.GetAccessorId(this.Setter.Accessor, ignorePrivate)
			: 0;
		string declaration = "";
		
		if(getterId >= infoId) { declaration = "get;"; }
		else if(getterId > 0) { declaration = $"{InspectorUtility.GetAccessorFromId(getterId)} get;"; }
		
		if(setterId >= infoId)
		{
			declaration += $"{(declaration != "" ? " " : "")}set;";
		}
		else if(setterId > 0)
		{
			declaration += $"{(declaration != "" ? " " : "")}{InspectorUtility.GetAccessorFromId(setterId)} set;";
		}
		
		return declaration;
	}
	
	/// <summary>Removes any unwanted elements within the array of property informations</summary>
	/// <param name="properties">The array of property informations to remove from</param>
	/// <param name="isStatic">Set to true to remove all non-static members</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(List<PropertyInspection> properties, bool isStatic, bool isOriginal)
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
	private static void RemoveDuplicates(List<PropertyInspection> properties, List<PropertyInspection> listProperties)
	{
		for(int i = properties.Count - 1; i >= 0; i--)
		{
			foreach(PropertyInspection property in listProperties)
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
			? InspectorUtility.GetAccessorId(this.Getter.Accessor, false)
			: 0;
		int setterId = this.Setter != null
			? InspectorUtility.GetAccessorId(this.Setter.Accessor, false)
			: 0;
		
		return InspectorUtility.GetAccessorFromId(System.Math.Max(getterId, setterId));
	}
	
	/// <summary>Gets the modifier of the property from either the getter or the setting</summary>
	/// <returns>Returns the modifier of the property</returns>
	private string GetModifier()
	{
		int getterId = this.Getter != null
			? InspectorUtility.GetAccessorId(this.Getter.Accessor, false)
			: 0;
		int setterId = this.Setter != null
			? InspectorUtility.GetAccessorId(this.Setter.Accessor, false)
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
		
		foreach(ParameterInspection parameter in this.Parameters)
		{
			declarations.Add(parameter.FullDeclaration);
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
}
