
namespace DocNET.Inspections;

using Mono.Cecil;

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>A quick look into the information of the type</summary>
public partial class QuickTypeInspection
{
	#region Properties
	
	// The hash map of the changes from the managed types to primitives to make it easier to read for type
	private static readonly Dictionary<string, string> Changes = new Dictionary<string, string>(new KeyValuePair<string, string>[] {
		new KeyValuePair<string, string>("System.Boolean", "bool"),
		new KeyValuePair<string, string>("System.Byte", "byte"),
		new KeyValuePair<string, string>("System.SByte", "sbyte"),
		new KeyValuePair<string, string>("System.UInt16", "ushort"),
		new KeyValuePair<string, string>("System.Int16", "short"),
		new KeyValuePair<string, string>("System.UInt32", "uint"),
		new KeyValuePair<string, string>("System.Int32", "int"),
		new KeyValuePair<string, string>("System.UInt64", "ulong"),
		new KeyValuePair<string, string>("System.Int64", "long"),
		new KeyValuePair<string, string>("System.Single", "float"),
		new KeyValuePair<string, string>("System.Double", "double"),
		new KeyValuePair<string, string>("System.Decimal", "decimal"),
		new KeyValuePair<string, string>("System.String", "string"),
		new KeyValuePair<string, string>("System.Object", "object"),
		new KeyValuePair<string, string>("System.ValueType", "struct"),
		new KeyValuePair<string, string>("System.Enum", "enum"),
		new KeyValuePair<string, string>("System.Void", "void"),
	});
	
	private string unlocalizedName;
	private string nonInstancedFullName;
	private string name;
	private string fullName;
	private string namespaceName;
	
	/// <summary>The name of the type as found within the library's IL code</summary>
	/// <remarks>The character ` means that it has generic parameters</remarks>
	public string UnlocalizedName { get => this.unlocalizedName; set => this.unlocalizedName = value; }
	/// <summary>The name of the type that is slightly localized but not generically instanced</summary>
	public string NonInstancedFullName { get => this.nonInstancedFullName; set => this.nonInstancedFullName = value; }
	/// <summary>The name of the type as found when looking at the code</summary>
	/// <remarks>
	/// If there are any generic parameters, it will display it as a developer would declare it
	/// </remarks>
	public string Name { get => this.name; set => this.name = value; }
	/// <summary>
	/// The full name of the type as found when looking at the code.
	/// Includes the namespace and the name within this variable
	/// </summary>
	public string FullName { get => this.fullName; set => this.fullName = value; }
	/// <summary>The name of the namespace where the type is located in</summary>
	public string NamespaceName { get => this.namespaceName; set => this.namespaceName = value; }
	/// <summary>The list of generic parameters that the type contains</summary>
	public GenericParametersInspection[] GenericParameters { get; set; }
	/// <summary>Set to true if the type is a generic type</summary>
	public bool IsGenericType { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates the information for a quick look into the type</summary>
	/// <param name="type">The type definition to look into</param>
	/// <returns>Returns a quick look at the type information</returns>
	public static QuickTypeInspection GenerateInfo(TypeDefinition type)
	{
		QuickTypeInspection info = new QuickTypeInspection();
		string[] generics = TypeInspection.GetGenericParametersString(
			type.GenericParameters.ToArray()
		);
		
		GetNames(
			type.FullName.Replace("&", ""),
			GetNamespace(type),
			generics,
			out info.unlocalizedName,
			out info.nonInstancedFullName,
			out info.fullName,
			out info.namespaceName,
			out info.name
		);
		info.GenericParameters = GenericParametersInspection.GenerateInfoArray(type.GenericParameters);
		if(info.GenericParameters.Length == 0)
		{
			info.GenericParameters = GetGenericParameters(type.FullName);
		}
		info.IsGenericType = type.IsGenericParameter && (info.UnlocalizedName == info.NonInstancedFullName);
		
		return info;
	}
	
	/// <summary>Generates the information for a quick look into the type</summary>
	/// <param name="type">The type reference to look into</param>
	/// <returns>Returns a quick look at the type information</returns>
	public static QuickTypeInspection GenerateInfo(TypeReference type)
	{
		QuickTypeInspection info = new QuickTypeInspection();
		string[] generics = TypeInspection.GetGenericParametersString(
			type.GenericParameters.ToArray()
		);
		
		GetNames(
			type.FullName.Replace("&", ""),
			GetNamespace(type),
			generics,
			out info.unlocalizedName,
			out info.nonInstancedFullName,
			out info.fullName,
			out info.namespaceName,
			out info.name
		);
		info.GenericParameters = GenericParametersInspection.GenerateInfoArray(type.GenericParameters);
		if(info.GenericParameters.Length == 0)
		{
			info.GenericParameters = GetGenericParameters(type.FullName);
		}
		info.IsGenericType = type.IsGenericParameter && (info.UnlocalizedName == info.NonInstancedFullName);
		
		return info;
	}
	
	/// <summary>Makes the names of managed types of primitives into the names of primitives</summary>
	/// <param name="name">The name of the type</param>
	/// <returns>
	/// Returns the name of the primitive or the type depending if it's a managed
	/// version of the type
	/// </returns>
	public static string MakeNameFriendly(string name)
	{
		string temp = name;
		
		foreach(KeyValuePair<string, string> keyVal in Changes)
		{
			temp = temp.Replace(keyVal.Key, keyVal.Value);
		}
		
		return temp;
	}
	
	/// <summary>Deletes the namespace full the given name</summary>
	/// <param name="name">The name of the type</param>
	/// <returns>Returns a string with any namespaces being removed</returns>
	public static string DeleteNamespaceFromType(string name)
	{
		return InspectionRegex.TypeFromCulture().Replace(InspectionRegex.Namespace().Replace(name, ""), ", $1");
	}
	
	/// <summary>Gets the list of generic parameter names from the full name of the type</summary>
	/// <param name="fullName">The full name of the type</param>
	/// <returns>Returns the list of generic parameter names</returns>
	public static string[] GetGenericParametersAsStrings(string fullName)
	{
		GenericParametersInspection[] infos = GetGenericParameters(fullName);
		string[] results = new string[infos.Length];
		int i = 0;
		
		foreach(GenericParametersInspection info in infos)
		{
			results[i] = info.Name.Replace(",", ", ");
			foreach(KeyValuePair<string, string> keyVal in Changes)
			{
				results[i] = results[i].Replace(keyVal.Key, keyVal.Value);
			}
			i++;
		}
		
		return results;
	}
	
	/// <summary>Gets the list of information of generic parameters from the full name of the type</summary>
	/// <param name="fullName">The full name of the type</param>
	/// <returns>Returns the list of information of generic parameters</returns>
	public static GenericParametersInspection[] GetGenericParameters(string fullName)
	{
		int lt = fullName.IndexOf('<');
		
		if(lt == -1) { return new GenericParametersInspection[0]; }
		
		List<GenericParametersInspection> results = new List<GenericParametersInspection>();
		GenericParametersInspection info;
		int gt = fullName.LastIndexOf('>');
		int scope = 0;
		int curr = lt + 1;
		
		for(int i = curr; i < fullName.Length; i++)
		{
			if(fullName[i] == '<') { scope++; }
			else if(fullName[i] == '>') { scope--; }
			if(scope < 0)
			{
				gt = i;
				break;
			}
			else if(fullName[i] == ',' && scope == 0)
			{
				info = new GenericParametersInspection();
				info.Name = InspectionRegex.GenericNotation()
					.Replace(fullName.Substring(curr, i - curr), "");
				info.UnlocalizedName = GenericParametersInspection.UnlocalizeName(info.Name);
				info.Name = MakeNameFriendly(info.Name);
				info.Constraints = new QuickTypeInspection[0];
				results.Add(info);
				curr = i + 1;
			}
		}
		
		info = new GenericParametersInspection();
		info.Name = InspectionRegex.GenericNotation()
			.Replace(fullName.Substring(curr, gt - curr), "");
		info.UnlocalizedName = GenericParametersInspection.UnlocalizeName(info.Name);
		info.Name = MakeNameFriendly(info.Name);
		info.Constraints = new QuickTypeInspection[0];
		results.Add(info);
		
		return results.ToArray();
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>
	/// Gathers all the names for the type information using the type's full
	/// name and namespace
	/// </summary>
	/// <param name="typeFullName">The full name of the type</param>
	/// <param name="typeNamespace">The namespace of the type</param>
	/// <param name="generics">The list of generic strings</param>
	/// <param name="unlocalizedName">The resulting unlocalized name of the type</param>
	/// <param name="nonInstancedFullName">The resulting full name for non-instanced types</param>
	/// <param name="fullName">The resulting full name of the type</param>
	/// <param name="namespaceName">The resulting namespace of the type</param>
	/// <param name="name">The resulting name of the type</param>
	private static void GetNames(
		string typeFullName, string typeNamespace, string[] generics,
		out string unlocalizedName, out string nonInstancedFullName,
		out string fullName, out string namespaceName, out string name
	)
	{
		int index = typeFullName.IndexOf('<');
		
		unlocalizedName = (index == -1 ? typeFullName : typeFullName.Substring(0, index)).Replace("[]", "");
		fullName = InspectionRegex.GenericNotation()
			.Replace(TypeInspection.LocalizeName(typeFullName, generics), "");
		name = DeleteNamespaceFromType(MakeNameFriendly(fullName));
		name = name.Replace("/", ".");
		fullName = fullName.Replace("/", ".");
		nonInstancedFullName = fullName;
		if(unlocalizedName.Contains('.'))
		{
			namespaceName = InspectionRegex.NamespaceName().Replace(unlocalizedName, "$1");
		}
		else
		{
			namespaceName = "";
		}
	}
	
	/// <summary>Gets the namespace from the type</summary>
	/// <param name="type">The type to look into</param>
	/// <returns>Returns the namespace of the type</returns>
	private static string GetNamespace(TypeDefinition type)
	{
		TypeDefinition nestedType = type;
		
		while(nestedType.IsNested)
		{
			nestedType = nestedType.DeclaringType;
		}
		
		return nestedType.Namespace;
	}
	
	/// <summary>Gets the namespace from the type</summary>
	/// <param name="type">The type to look into</param>
	/// <returns>Returns the namespace of the type</returns>
	private static string GetNamespace(TypeReference type)
	{
		TypeReference nestedType = type;
		
		while(nestedType.IsNested)
		{
			nestedType = nestedType.DeclaringType;
		}
		
		return nestedType.Namespace;
	}
	
	#endregion // Private Methods
}
