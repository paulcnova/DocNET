
namespace Taco.DocNET.Inspector;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to fields</summary>
public class FieldInfo : BaseInfo
{
	#region Properties
	
	/// <summary>Gets and sets the name of the field</summary>
	public string Name { get; set; }
	/// <summary>Gets and sets the value of the field (if it's a constant)</summary>
	public string Value { get; set; }
	/// <summary>Gets and sets if the field is constant</summary>
	public bool IsConstant { get; set; }
	/// <summary>Gets and sets if the field is static</summary>
	public bool IsStatic { get; set; }
	/// <summary>Gets and sets if the field is readonly</summary>
	public bool IsReadonly { get; set; }
	/// <summary>Gets and sets the list of attributes that the field contains</summary>
	public AttributeInfo[] Attributes { get; set; }
	/// <summary>Gets and sets the accessor of the field (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	/// <summary>Gets and sets any modifiers to the field (such as static, const, static readonly, etc)</summary>
	public string Modifier { get; set; }
	/// <summary>Gets and sets the type information of the field's type</summary>
	public QuickTypeInfo TypeInfo { get; set; }
	/// <summary>Gets and sets the type the field is implemented in</summary>
	public QuickTypeInfo ImplementedType { get; set; }
	/// <summary>Gets and sets the declaration of the field as it is found within the code</summary>
	public string FullDeclaration { get; set; }
	/// <summary>If it's true then the info should not be printed out and should be deleted</summary>
	internal bool shouldDelete = false;
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>
	/// Generates an array of field informations by locating it recursively and
	/// excluding or including static members
	/// </summary>
	/// <param name="type">The type definition to look into the fields</param>
	/// <param name="recursive">
	/// Set to true if it should look into the base type recursively to find
	/// base members
	/// </param>
	/// <param name="isStatic">Set to true to look for only static members</param>
	/// <returns>Returns the list of field informations</returns>
	public static FieldInfo[] GenerateInfoArray(TypeDefinition type, bool recursive, bool isStatic)
	{
		if(!recursive)
		{
			FieldInfo[] results = GenerateInfoArray(type.Fields);
			
			RemoveUnwanted(ref results, isStatic, true);
			
			return results;
		}
		
		List<FieldInfo> methods = new List<FieldInfo>();
		FieldInfo[] temp;
		TypeDefinition currType = type;
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			temp = GenerateInfoArray(currType.Fields);
			RemoveUnwanted(ref temp, isStatic, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(ref temp, methods);
			}
			methods.AddRange(temp);
			baseType = currType.BaseType;
			if(baseType == null)
			{
				break;
			}
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return methods.ToArray();
	}
	
	/// <summary>Generates an array of field informations from a collection of field definitions</summary>
	/// <param name="fields">The field definition to look into</param>
	/// <returns>Returns the list of field informations</returns>
	public static FieldInfo[] GenerateInfoArray(Collection<FieldDefinition> fields)
	{
		List<FieldInfo> results = new List<FieldInfo>();
		FieldInfo info;
		
		foreach(FieldDefinition field in fields)
		{
			if(field.Name == "value__")
			{
				continue;
			}
			else if(IsCompilerGenerated(field))
			{
				continue;
			}
			info = GenerateInfo(field);
			if(info.shouldDelete)
			{
				continue;
			}
			results.Add(info);
		}
		
		return results.ToArray();
	}
	
	/// <summary>Generates the information for the field from the field definition</summary>
	/// <param name="field">The field definition to look into</param>
	/// <returns>Returns the information of the field</returns>
	public static FieldInfo GenerateInfo(FieldDefinition field)
	{
		FieldInfo info = new FieldInfo();
		string val = System.Text.ASCIIEncoding.ASCII.GetString(field.InitialValue);
		
		if(field.IsAssembly) { info.Accessor = "internal"; }
		else if(field.IsFamily) { info.Accessor = "protected"; }
		else if(field.IsPrivate) { info.Accessor = "private"; }
		else { info.Accessor = "public"; }
		if(Inspector.TypeInfo.ignorePrivate && PropertyInfo.GetAccessorId(info.Accessor) == 0)
		{
			info.shouldDelete = true;
			return info;
		}
		info.Name = field.Name;
		info.TypeInfo = QuickTypeInfo.GenerateInfo(field.FieldType);
		info.ImplementedType = QuickTypeInfo.GenerateInfo(field.DeclaringType);
		info.Value = $"{ field.Constant ?? val }";
		info.IsConstant = field.HasConstant;
		info.IsStatic = field.IsStatic;
		info.IsReadonly = field.IsInitOnly;
		info.Attributes = AttributeInfo.GenerateInfoArray(field.CustomAttributes);
		if(field.HasConstant) { info.Modifier = "const"; }
		else if(field.IsStatic && field.IsInitOnly) { info.Modifier = "static readonly"; }
		else if(field.IsStatic) { info.Modifier = "static"; }
		else if(field.IsInitOnly) { info.Modifier = "readonly"; }
		else { info.Modifier = ""; }
		info.FullDeclaration = (
			$"{ info.Accessor } " +
			(info.Modifier != "" ? info.Modifier + " " : "") +
			$"{ info.TypeInfo.Name } " +
			info.Name
		);
		if(info.IsConstant)
		{
			info.FullDeclaration += $" = { info.Value }";
		}
		
		return info;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>
	/// Finds if the field is compiler generated, meaning it's used for
	/// properties
	/// </summary>
	/// <param name="field">The field information to look into</param>
	/// <returns>Returns true if the field is compiler generated</returns>
	private static bool IsCompilerGenerated(FieldDefinition field)
	{
		foreach(CustomAttribute attr in field.CustomAttributes)
		{
			if(attr.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
			{
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>Removes any unwanted elements within the list</summary>
	/// <param name="temp">The list of field informations to look into</param>
	/// <param name="isStatic">Set to true to remove any non-static members</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(ref FieldInfo[] temp, bool isStatic, bool isOriginal)
	{
		List<FieldInfo> fields = new List<FieldInfo>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
		{
			if(fields[i].shouldDelete)
			{
				fields.RemoveAt(i);
			}
			else if(fields[i].IsStatic != isStatic)
			{
				fields.RemoveAt(i);
			}
			else if(!isOriginal && fields[i].Accessor == "private")
			{
				fields.RemoveAt(i);
			}
		}
		
		temp = fields.ToArray();
	}
	
	/// <summary>Removes any duplicates within the list</summary>
	/// <param name="temp">The list of field informations to remove duplicates from</param>
	/// <param name="listFields">The list of fields that have already been recorded</param>
	private static void RemoveDuplicates(ref FieldInfo[] temp, List<FieldInfo> listFields)
	{
		List<FieldInfo> fields = new List<FieldInfo>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
		{
			foreach(FieldInfo field in listFields)
			{
				if(fields[i].Name == field.Name)
				{
					fields.RemoveAt(i);
					break;
				}
			}
		}
		
		temp = fields.ToArray();
	}
	
	#endregion // Private Methods
}
