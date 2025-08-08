
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to fields</summary>
public class FieldInspection : BaseInspection
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
	public List<AttributeInspection> Attributes { get; set; } = new List<AttributeInspection>();
	
	/// <summary>Gets and sets the accessor of the field (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Gets and sets any modifiers to the field (such as static, const, static readonly, etc)</summary>
	public string Modifier { get; set; }
	
	/// <summary>Gets and sets the type information of the field's type</summary>
	public QuickTypeInspection TypeInfo { get; set; }
	
	/// <summary>Gets and sets the type the field is implemented in</summary>
	public QuickTypeInspection ImplementedType { get; set; }
	
	/// <summary>Gets and sets the declaration of the field as it is found within the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>If it's true then the info should not be printed out and should be deleted</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	/// <summary>A constructor for generating field data</summary>
	/// <param name="field">The field definition to look into</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	public FieldInspection(FieldDefinition field, bool ignorePrivate = true)
	{
		if(field.IsAssembly) { this.Accessor = "internal"; }
		else if(field.IsFamily) { this.Accessor = "protected"; }
		else if(field.IsPrivate) { this.Accessor = "private"; }
		else { this.Accessor = "public"; }
		
		if(ignorePrivate && InspectorUtility.GetAccessorId(this.Accessor, ignorePrivate) == 0)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		string val = System.Text.ASCIIEncoding.ASCII.GetString(field.InitialValue);
		
		this.Name = field.Name;
		this.TypeInfo = new QuickTypeInspection(field.FieldType);
		this.ImplementedType = new QuickTypeInspection(field.DeclaringType);
		this.Value = $"{field.Constant ?? val}";
		this.IsConstant = field.HasConstant;
		this.IsStatic = field.IsStatic;
		this.IsReadonly = field.IsInitOnly;
		this.Attributes = AttributeInspection.CreateArray(field.CustomAttributes);
		
		if(field.HasConstant) { this.Modifier = "const"; }
		else if(field.IsStatic && field.IsInitOnly) { this.Modifier = "static readonly"; }
		else if(field.IsStatic) { this.Modifier = "static"; }
		else if(field.IsInitOnly) { this.Modifier = "readonly"; }
		else { this.Modifier = ""; }
		
		this.FullDeclaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{this.TypeInfo.Name} {this.Name}";
		
		if(this.IsConstant) { this.FullDeclaration += $" = {this.Value}"; }
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of field informations by locating it recursively and excluding or including static members</summary>
	/// <param name="type">The type definition to look into the fields</param>
	/// <param name="recursive">Set to true if it should look into the base type recursively to find base members</param>
	/// <param name="isStatic">Set to true to look for only static members</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	/// <returns>Returns the list of field informations</returns>
	public static List<FieldInspection> CreateArray(TypeDefinition type, bool recursive, bool isStatic, bool ignorePrivate = true)
	{
		if(!recursive)
		{
			List<FieldInspection> results = CreateArray(type.Fields, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, true);
			
			return results;
		}
		
		List<FieldInspection> fields = new List<FieldInspection>();
		List<FieldInspection> temp;
		TypeDefinition currType = type;
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			temp = CreateArray(currType.Fields, ignorePrivate);
			RemoveUnwanted(temp, isStatic, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(temp, fields);
			}
			fields.AddRange(temp);
			baseType = currType.BaseType;
			
			if(baseType == null) { break; }
			
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return fields;
	}
	
	/// <summary>Generates an array of field informations from a collection of field definitions</summary>
	/// <param name="fields">The field definition to look into</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	/// <returns>Returns the list of field informations</returns>
	public static List<FieldInspection> CreateArray(Collection<FieldDefinition> fields, bool ignorePrivate = true)
	{
		List<FieldInspection> results = new List<FieldInspection>();
		
		foreach(FieldDefinition field in fields)
		{
			if(field.Name == "value__") { continue; }
			else if(IsCompilerGenerated(field)) { continue; }
			
			FieldInspection info = new FieldInspection(field, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			
			results.Add(info);
		}
		
		return results;
	}
	
	public override string GetXmlNameID() => $"F:{this.ImplementedType.UnlocalizedName}.{this.Name}";
	
	public override BaseInspection GetBaseVersion(SiteMap siteMap)
	{
		TypeInspection baseType = siteMap.TryGetBaseInspection(this.ImplementedType.UnlocalizedName);
		
		return this.IsStatic
			? baseType.StaticFields.Find(field => field.Name == this.Name)
			: baseType.Fields.Find(field => field.Name == this.Name);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Finds if the field is compiler generated, meaning it's used for properties</summary>
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
	/// <param name="fields">The list of field informations to look into</param>
	/// <param name="isStatic">Set to true to remove any non-static members</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(List<FieldInspection> fields, bool isStatic, bool isOriginal)
	{
		for(int i = fields.Count - 1; i >= 0; i--)
		{
			if(fields[i].ShouldIgnore) { fields.RemoveAt(i); }
			else if(fields[i].IsStatic != isStatic) { fields.RemoveAt(i); }
			else if(!isOriginal && fields[i].Accessor == "private") { fields.RemoveAt(i); }
		}
	}
	
	/// <summary>Removes any duplicates within the list</summary>
	/// <param name="list">The list of field informations to remove duplicates from</param>
	/// <param name="listFields">The list of fields that have already been recorded</param>
	private static void RemoveDuplicates(List<FieldInspection> list, List<FieldInspection> listFields)
	{
		for(int i = list.Count - 1; i >= 0; i--)
		{
			foreach(FieldInspection field in listFields)
			{
				if(list[i].Name == field.Name)
				{
					list.RemoveAt(i);
					break;
				}
			}
		}
	}
	
	#endregion // Private Methods
}
