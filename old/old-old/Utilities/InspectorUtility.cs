
namespace DocNET.Utilities;

using DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;
using System.IO;

/// <summary>A static class used for utility</summary>
public static class InspectorUtility
{
	#region Properties
	
	/// <summary>The hash map of the changes from the managed types to primitives to make it easier to read for type</summary>
	private static readonly Dictionary<string, string> FriendlyNameChanges = new Dictionary<string, string>(new KeyValuePair<string, string>[] {
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
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc cref="IUtilitySet.CreateSystemLink(string, string)"/>
	public static string CreateSystemLink(string typePath, string linkName) => "";//UtilitySet?.CreateSystemLink(typePath, linkName) ?? "";
	/// <inheritdoc cref="IUtilitySet.CreateInternalLink(string, string)"/>
	public static string CreateInternalLink(string typePath, string linkName) => "";//UtilitySet?.CreateInternalLink(typePath, linkName) ?? "";
	
	/// <summary>Renders the given markdown</summary>
	/// <param name="markdown">The markdown content to render</param>
	/// <returns>Returns the rendered markdown</returns>
	public static string RenderMarkdown(string markdown) => markdown;//Markdown.ToHtml(markdown, Pipeline);
	
	/// <summary>Makes the unlocalized name friendly, transforming something like System.Int64 into long to be more friendly towards what developers recognize</summary>
	/// <param name="name">The name of the type to transform</param>
	/// <returns>Returns a type that developers more easily recognize</returns>
	public static string MakeNameFriendly(string name)
	{
		string temp = name;
		
		foreach(KeyValuePair<string, string> keyVal in FriendlyNameChanges)
		{
			temp = temp.Replace(keyVal.Key, keyVal.Value);
		}
		
		return temp;
	}
	
	/// <summary>Unlocalizes the name to look like what the IL code would look like</summary>
	/// <param name="name">The name to unlocalize</param>
	/// <returns>Returns the unlocalized name of the type</returns>
	public static string UnlocalizeName(string name)
	{
		int lt = name.IndexOf('<');
		
		if(lt == -1) { return name; }
		
		int gt = name.LastIndexOf('>');
		int scope = 0;
		int count = 1;
		
		for(int i = lt + 1; i < gt; i++)
		{
			if(name[i] == '<') { scope++; }
			else if(name[i] == '>') { scope--; }
			else if(name[i] == ',' && scope == 0) { count++; }
		}
		
		return $"{ name.Substring(0, lt) }`{ count }";
	}
	
	/// <summary>Gets the namespace from the type</summary>
	/// <param name="type">The type to look into</param>
	/// <returns>Returns the namespace of the type</returns>
	public static string GetNamespace(TypeDefinition type)
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
	public static string GetNamespace(TypeReference type)
	{
		TypeReference nestedType = type;
		
		while(nestedType.IsNested)
		{
			nestedType = nestedType.DeclaringType;
		}
		
		return nestedType.Namespace;
	}
	
	/// <summary>Gets the list of generic parameter names from the full name of the type</summary>
	/// <param name="fullName">The full name of the type</param>
	/// <returns>Returns the list of generic parameter names</returns>
	public static List<string> GetGenericParametersAsStrings(string fullName)
	{
		List<GenericParameterInspection> infos = GetGenericParameters(fullName);
		List<string> results = new List<string>();
		
		foreach(GenericParameterInspection info in infos)
		{
			results.Add(InspectorUtility.MakeNameFriendly(info.Name.Replace(",", ", ")));
		}
		
		return results;
	}
	
	/// <summary>Gets the list of information of generic parameters from the full name of the type</summary>
	/// <param name="fullName">The full name of the type</param>
	/// <returns>Returns the list of information of generic parameters</returns>
	public static List<GenericParameterInspection> GetGenericParameters(string fullName)
	{
		int lt = fullName.IndexOf('<');
		
		if(lt == -1) { return new List<GenericParameterInspection>(); }
		
		List<GenericParameterInspection> results = new List<GenericParameterInspection>();
		GenericParameterInspection data;
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
				data = new GenericParameterInspection();
				data.Name = InspectionRegex.GenericNotation()
					.Replace(fullName.Substring(curr, i - curr), "");
				data.UnlocalizedName = UnlocalizeName(data.Name);
				data.Name = InspectorUtility.MakeNameFriendly(data.Name);
				results.Add(data);
				curr = i + 1;
			}
		}
		
		data = new GenericParameterInspection();
		data.Name = InspectionRegex.GenericNotation()
			.Replace(fullName.Substring(curr, gt - curr), "");
		data.UnlocalizedName = UnlocalizeName(data.Name);
		data.Name = InspectorUtility.MakeNameFriendly(data.Name);
		results.Add(data);
		
		return results;
	}
	
	/// <summary>Removes the namespace full the given name</summary>
	/// <param name="name">The name of the type</param>
	/// <returns>Returns a string with any namespaces being removed</returns>
	public static string RemoveNamespaceFromType(string name)
		=> InspectionRegex.TypeFromCulture().Replace(InspectionRegex.Namespace().Replace(name, ""), ", $1");
	
	/// <summary>Gets an array of generic parameter names from the given array of generic parameters</summary>
	/// <param name="generics">The array of generic parameters</param>
	/// <returns>Returns an array of generic parameter names</returns>
	public static List<string> GetGenericParametersString(Collection<GenericParameter> generics)
	{
		if(generics == null) { return new List<string>(); }
		
		List<string> results = new List<string>();
		
		foreach(GenericParameter parameter in generics)
		{
			results.Add(parameter.Name);
		}
		
		return results;
	}
	
	/// <summary>Gets the list of generics from the given name</summary>
	/// <param name="name">The name to get the generics from</param>
	/// <returns>Returns the list of generics</returns>
	public static List<string> GetGenerics(string name)
	{
		int left = name.IndexOf("<");
		int scope = 0;
		string temp = "";
		List<string> generics = new List<string>();
		
		for(int i = left + 1; i < name.Length; i++)
		{
			if(name[i] == '<') { scope++; }
			else if(name[i] == '>') { scope--; }
			if(scope < 0) { break; }
			if(scope == 0 && name[i] == ',')
			{
				generics.Add(temp);
				temp = "";
			}
			else { temp += name[i]; }
		}
		
		generics.Add(temp);
		
		return generics;
	}
	
	/// <summary>Localizes the name using the list of generic parameter names</summary>
	/// <param name="name">The name of the type</param>
	/// <param name="generics">The array of generic parameter names</param>
	/// <returns>Returns the localized name</returns>
	public static string LocalizeName(string name, List<string> generics)
	{
		if(generics.Count == 0 && name.LastIndexOf('<') == -1)
		{
			return name;
		}
		
		if(name.LastIndexOf('<') != -1)
		{
			generics = GetGenerics(name);
			name = name.Substring(0, name.IndexOf('<'));
		}
		
		int index = 0;
		string newName = InspectionRegex.GenericNotation().Replace(name, match => {
			int count;
			string result = "";
			
			if(int.TryParse(match.Groups[1].Value, out count))
			{
				string[] localGenerics = new string[count];
				
				for(int i = 0; i < count; i++)
				{
					localGenerics[i] = generics[index++];
				}
				
				result = $"<{ string.Join(",", localGenerics) }>";
			}
			
			return result;
		});
		
		return newName;
	}
	
	/// <summary>Gets the generic parameter constraints (if any)</summary>
	/// <param name="generics">The generic parameter to look into</param>
	/// <returns>Returns the string of the generic parameter constraints</returns>
	public static string GetGenericParameterConstraints(List<GenericParameterInspection> generics)
	{
		string results = "";
		
		foreach(GenericParameterInspection generic in generics)
		{
			if(generic.Constraints.Count == 0) { continue; }
			
			results += $" where {generic.Name} : ";
			for(int i = 0; i < generic.Constraints.Count; i++)
			{
				results += $"{generic.Constraints[i].Name}{(
					i != generic.Constraints.Count - 1
						? ","
						: ""
				)}";
			}
		}
		
		return results;
	}
	
	/// <summary>
	/// Gets a quantitative id with the given accessor, with:
	/// * 4 - public
	/// * 3 - protected
	/// * 2 - private
	/// * 1 - internal
	/// * 0 - non-existent
	/// </summary>
	/// <param name="accessor">The accessor to get an id from</param>
	/// <param name="ignorePrivate">Set to true to ignore any private accessors</param>
	/// <returns>Returns an integer that represents the visibility levels of the accessor</returns>
	public static int GetAccessorId(string accessor, bool ignorePrivate)
	{
		switch(accessor)
		{
			case "internal": return ignorePrivate ? 0 : 1;
			case "private": return ignorePrivate ? 0 : 2;
			case "protected": return 3;
			case "public": return 4;
		}
		return 0;
	}
	
	/// <summary>Gets the accessor string from a given id (ranging from 0 to 4)</summary>
	/// <param name="id">The id to get the accessor with</param>
	/// <returns>Returns the accessor</returns>
	public static string GetAccessorFromId(int id)
	{
		switch(id)
		{
			case 1: return "internal";
			case 2: return "private";
			case 3: return "protected";
			case 4: return "public";
		}
		
		return "";
	}
	
	/// <summary>Finds if the method is an extension</summary>
	/// <param name="method">The method to look into</param>
	/// <returns>Returns true if the method is an extension by having the extension attribute</returns>
	public static bool HasExtensionAttribute(List<AttributeInspection> attributes)
	{
		foreach(AttributeInspection attr in attributes)
		{
			if(attr.TypeInfo.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
			{
				return true;
			}
		}
		
		return false;
	}
	
	#endregion // Public Methods
}
